#include <iostream>
#include "aint.hpp"

/*  ----- Arbitrary* length container ------
 * (0) The length isn't *really* arbitrary - the container can only contain a number of bytes that fits into size_t.
 *      This seems to me like a reasonable limit, as I don't know how to allocate more memory than that and
 *      we're not really at the point where accessing an address space >= 2^64 is taken for granted anyway
 *      (or really possible, for that matter).
 * (1) The number is held in a continuous space of uint32_t blocks. This space is marked by the pointers *begin
 *      (points at first allocated block), *end (points just beyond last allocated block), and *val_end
 *      (points just beyond last actually used block).
 * (2) The blocks are in "little endian" order, meaning *begin points at the least significant block
 * (3) A value of 0 is represented by all the pointers equalling nullptr. The consistency of this is ensured by the
 *      trim_zeros() function, which is performed after every operation which could potentially zero out some number
 *      of the most significant blocks (and maybe some places where it couldn't, but the check is almost instant in
 *      that case, so what the hell). This representation saves time and makes zero checks near instantaneous.
 * (4) Nonempty containers start with at least four blocks in size. This makes them reasonably small while also
 *      reducing the number of copies made.
 * (5) All of the 'x' arithmetic operators are implemented using their 'x=' counterparts.
 * (6) For some reason, Clang-Tidy yells at me whenever I shift bits on uint32_t, uint8_t or uint64_t numbers,
 *      claiming that I'm using a shifting operation on a signed type variable. I'm not sure why it does this,
 *      since time I checked, the 'u' in 'uint' didn't stand for 'signed'. Bizarrely enough, shifting by
 *      a result of a subtraction causes no problems whatsoever.
 * (7) Everything else should be either commented on or self-explanatory.
*/
//<editor-fold desc="private">
#pragma region priv
size_t aint::block_count() const {
    size_t ret(0);
    for (auto i = begin; i != val_end; ++i)
        ++ret;
    return ret;
}

void aint::clear() {
    free(begin);
    begin = val_end = end = nullptr;
}

void aint::create_empty() {
    val_end = begin = static_cast<uint32_t *>(malloc(sizeof(uint32_t) * BASE_SIZE));
    end = begin + BASE_SIZE;
}

void aint::copy(const aint& from) {
    size_t bc = from.block_count();
    begin = static_cast<uint32_t *>(malloc(sizeof(uint32_t) * bc));
    end = begin + bc;
    val_end = begin;
    for (auto i = from.begin; i != from.val_end; ++i)
        *(val_end++) = *i;
}

// when a container is full, we create a new one twice the size and copy the value into it
void aint::double_size() {
    size_t bc = block_count() * 2;
    auto *tmp = static_cast<uint32_t *>(malloc(sizeof(uint32_t) * bc));
    uint32_t *tmp_ve = tmp;
    for (auto i = begin; i != val_end; ++i) {
        *tmp_ve = *i;
        ++tmp_ve;
    }
    free(begin);
    begin = tmp;
    val_end = tmp_ve;
    end = begin + bc;
}

// safely adds a block of value, resizing the container if necessary
void aint::add_block(uint32_t val) {
    if (!begin) create_empty();
    if (val_end == end) double_size();
    *val_end = val;
    ++val_end;
}

// some operations may zero out most sig. blocks. we want to move val_end accordingly
aint& aint::trim_zeros() {
    while (val_end != begin && !(*(val_end-1)))
        --val_end;
    if (val_end == begin) clear();
    return *this;
}
#pragma endregion priv
//</editor-fold>

//<editor-fold desc="constructors">
#pragma region constructors

// following the rule of 5 (7)
aint::aint() {
    begin = nullptr;
    val_end = nullptr;
    end = nullptr;
}
aint::~aint() {
    free(begin);
}
aint::aint(uint32_t val) {
    if (!val) {
        begin = val_end = end = nullptr;
        return;
    }
    create_empty();
    val_end = begin + 1;
    *begin = val;
}

// Clang-Tidy might warn you that this constructor doesn't initialize any of the pointers.
// Clang-Tidy is lying.
aint::aint(const aint& from) {
    copy(from);
}
aint::aint(aint&& from) noexcept{
    begin = from.begin;
    end = from.end;
    val_end = from.val_end;
    from.begin = from.val_end = from.end = nullptr;
}
aint& aint::operator =(aint&& from) noexcept {
    free(begin);
    begin = from.begin;
    end = from.end;
    val_end = from.val_end;
    from.begin = from.val_end = from.end = nullptr;
    return *this;
}
aint& aint::operator =(const aint& from) {
    if (from.begin == begin) return *this;
    free(begin);
    copy(from);
    return *this;
}
aint& aint::operator =(const uint32_t val) {
    free(begin);
    if(!val) {
        begin = val_end = end = nullptr;
        return *this;
    }
    create_empty();
    val_end = begin + 1;
    *begin = val;
    return *this;
}
#pragma endregion constructors
//</editor-fold>

//<editor-fold desc="comparators">
#pragma region comparators
bool aint::operator <(const aint& other) const {
    // zero checks are O(1). no reason not to try them
    if (zero() && other.zero()) return false;
    if (zero() && !other.zero()) return true;
    if (!zero() && other.zero()) return false;

    size_t tbc = block_count();
    size_t obc = other.block_count();
    if (tbc < obc) return true;
    if (tbc > obc) return false;

    // valid pointers, since both are non-zero
    auto ti = val_end - 1;
    auto oi = other.val_end - 1;
    while (ti != begin && oi != other.begin) {
        if (*ti < *oi) return true;
        if (*ti > *oi) return false;

        --ti; --oi;
    }
    return *ti < *oi;
}
bool aint::operator >(const aint& other) const {
    return !(*this <= other);
}
bool aint::operator ==(const aint& other) const {
    auto this_i = begin;
    auto other_i = other.begin;
    while (this_i != val_end && other_i != other.val_end)
        if (*this_i != *other_i) return false;
        else {
            ++this_i;
            ++other_i;
        }

    return this_i == val_end && other_i == val_end;
}

bool aint::operator !=(const aint& other) const {
    return !(*this == other);
}
bool aint::operator <=(const aint& other) const {
    //same as '<' except for the last check
    if (zero() && other.zero()) return true;
    if (zero() && !other.zero()) return true;
    if (!zero() && other.zero()) return false;

    size_t tbc = block_count();
    size_t obc = other.block_count();
    if (tbc < obc) return true;
    if (tbc > obc) return false;

    auto ti = val_end - 1;
    auto oi = other.val_end - 1;
    while (ti != begin && oi != other.begin) {
        if (*ti < *oi) return true;
        if (*ti > *oi) return false;

        --ti; --oi;
    }
    return *ti <= *oi;
}
bool aint::operator >=(const aint& other) const {
    return !(*this < other);
}
#pragma endregion comparators
//</editor-fold>

//<editor-fold desc="operators">
#pragma region operators
aint& aint::operator +=(const aint& other) {
    if (other.zero()) return *this;
    // using a 64b intermediary to handle overflows
    uint64_t tmp;
    uint8_t carry = 0;
    auto ti = begin;
    auto oi = other.begin;
    while (ti != val_end && oi != other.val_end) {
        tmp = *ti;
        tmp += *oi;
        tmp += carry;
        *ti = static_cast<uint32_t>(tmp);
        carry = static_cast<uint8_t>(tmp >> BLK_SIZE);

        ++ti; ++oi;
    }

    // at most one of these cycles will happen
    for (; ti != val_end; ++ti) {
        tmp = *ti;
        tmp += carry;
        *ti = static_cast<uint32_t>(tmp);
        carry = static_cast<uint8_t>(tmp >> BLK_SIZE);
    }

    for (; oi != other.val_end; ++oi) {
        tmp = *oi;
        tmp += carry;
        add_block(static_cast<uint32_t>(tmp));
        carry = static_cast<uint8_t>(tmp >> BLK_SIZE);
    }

    if (carry) add_block(carry);
    return *this;
}

aint& aint::operator -=(const aint& other) {
    // we don't allow underflow
    if (other.zero() || zero()) return *this;
    uint8_t borrow = 0;
    auto ti = begin;
    auto oi = other.begin;
    while (ti != val_end && oi != other.val_end) {
        // borrow checks are simpler than carry checks - no need for the 64b buffer
        if (borrow && (*ti)--)
            borrow = 0;

        if (*oi > *ti)
            borrow = 1;

        *ti -= *oi;

        ++ti; ++oi;
    }

    // reached end of this and some numbers are still to be subtracted -> other > this
    if (ti == val_end && (borrow || oi != other.val_end)) {
        clear();
        return *this;
    }

    // we have at least one nonzero block left (see above), so this cycle cannot run out of bounds
    for (; borrow; ++ti) {
        if (*ti) borrow = 0;
        *ti -= 1;
    }

    return trim_zeros();
}

aint& aint::operator *=(const aint& other) {
    if (zero() || other.zero()) {
        clear();
        return *this;
    }
    // the result is stored in product, whose value is then taken
    aint product;

    // adding enough blocks to hold entire result
    for (auto i = begin; i != val_end; ++i)
        product.add_block(0);
    for (auto i = other.begin; i != other.val_end; ++i)
        product.add_block(0);

    // elementary level long multiplication

    auto p = product.begin;     // index of first byte in result of current line (same index as oi)
    uint32_t carry = 0;
    for (auto oi = other.begin; oi != other.val_end; ++oi) {
        carry = 0;
        auto pi = p;            // index of currently processed result block (index of oi + ti)
        for (auto ti = begin; ti != val_end; ++ti) {
            uint64_t t = *ti;   // temp value holds overflows
            t *= *oi;
            t += *pi;
            t += carry;         // due to how multiplying works, *oi + *pi + carry always fits in 64b
            carry = static_cast<uint32_t>(t >> BLK_SIZE);
            *pi = static_cast<uint32_t>(t);

            ++pi;
        }

        if (carry)
            if (pi == product.val_end)
                product.add_block(carry);
            else {
                *pi = carry;
            }

        ++p;
    }

    // potential carry is handled inside the cycle
    clear();
    swap(product);

    return trim_zeros();
}

aint& aint::operator %=(const aint& other) {
    // same algorithm as '/=', except we care about a different value
    if (other.zero() || zero()) {
        clear();
        return *this;
    }

    size_t obc = other.block_count();

    for (size_t tbc = block_count(); *this >= other; tbc = block_count()) {
        size_t blk_offset = tbc - obc;
        size_t offset = blk_offset * BLK_SIZE;

        uint32_t msb = *(val_end - 1);
        int small_shift = 0;
        while (msb) {
            msb >>= 1;
            ++small_shift;
        }
        msb = *(other.val_end - 1);
        while (msb) {
            msb >>= 1;
            --small_shift;
        }

        offset += small_shift;
        aint shifted(other << offset);

        if (shifted > *this) {
            shifted >>= 1;
        }
        *this -= shifted;
    }

    return trim_zeros();
}

aint& aint::operator /=(const aint& other) {
    // division by zero results in zero
    if (other.zero() || zero()) {
        clear();
        return *this;
    }

    size_t obc = other.block_count();   // &other stays constant throughout the algorithm
    aint result(0);
    aint one(1);                   // helper value that we'll shift as necessary

    for (size_t tbc = block_count(); *this >= other; tbc = block_count()) {

        // 1) calculating the offset between this and other
        size_t blk_offset = tbc - obc;
        size_t offset = blk_offset * BLK_SIZE;

        uint32_t msb = *(val_end - 1);
        int small_shift = 0;
        while (msb) {
            msb >>= 1;
            ++small_shift;
        }
        msb = *(other.val_end - 1);
        while (msb) {
            msb >>= 1;
            --small_shift;
        }

        offset += small_shift;
        aint shifted(other << offset);  // 2) aligning other to the exact length of this

        if (shifted > *this) {          // we might have overshot by one bit
            shifted >>= 1;
            offset -= 1;
        }

        *this -= shifted;               // 3) subtract shifted value from this
        result += (one << offset);      // 4) result is increased based on how far we needed to shift
        //5) repeat until other > this
    }

    // result holds "this"/other, this holds "this"%other
    swap(result);
    return trim_zeros();
}

aint& aint::operator <<=(size_t shift) {
    if (zero()) return *this;
    size_t block_offset = shift / BLK_SIZE;
    auto inner_offset = static_cast<uint8_t>(shift % BLK_SIZE);

    // add enough space
    for (size_t i = 0; i < block_offset; ++i)
        add_block(0);

    if (block_offset) {
        // get the original val_end (we can't get it before adding blocks, since we may move the container)
        trim_zeros();
        auto og_ve = val_end;
        val_end += block_offset;

        // moving from the back, shift all blocks...
        for (auto i = og_ve - 1; i != begin; --i)
            *(i + block_offset) = *i;
        *(begin + block_offset) = *begin;

        //...and zero out the shifted places
        for (auto i = begin + block_offset - 1; i != begin; --i)
            *i = 0;
        *begin = 0;
    }

    if (inner_offset) {
        add_block(0);
        for (auto i = val_end - 2; i != begin; --i) {
            *(i+1) += *i >> (BLK_SIZE - inner_offset); // add the overflowing part to the block above
            *i <<= inner_offset;                       // shift this container
        }
        *(begin+1) += *begin >> (BLK_SIZE - inner_offset); // same for begin
        *begin <<= inner_offset;
    }

    return trim_zeros();
}

aint& aint::operator >>=(size_t shift) {
    size_t block_offset = shift / BLK_SIZE;
    auto inner_offset = static_cast<uint8_t>(shift % BLK_SIZE);

    if (block_offset) {
        // we can't just move the begin pointer, since we wouldn't have any way to free the memory
        auto new_b = begin;              // points to the block that will be shifted to begin
        for (size_t i = 0; i < block_offset; ++i) {
            ++new_b;
            if (new_b == val_end) {     // shift is larger than this value
                clear();
                return *this;
            }
        }

        for (auto i = begin; new_b != val_end;)     // shifting blocks
            *(i++) = *(new_b++);

        for (size_t i = 0; i < block_offset; ++i)   // moving val_end accordingly
            --val_end;
    }
    if (!inner_offset) return *this;

    (*begin) >>= inner_offset;
    for (auto curr = begin+1; curr != val_end; ++curr) {
        uint32_t to_prev = (*curr) << (BLK_SIZE - inner_offset); // adding the outshifted bits to previous block
        *(curr-1) += to_prev;
        *curr >>= inner_offset;
    }

    return trim_zeros();
}
#pragma endregion operators
//</editor-fold>

//<editor-fold desc="copy-operators">
#pragma region copy-operators
aint aint::operator+(const aint& other) const {
    aint res(*this);
    res += other;
    return res;
}
aint aint::operator-(const aint& other) const {
    aint res(*this);
    res -= other;
    return res;
}
aint aint::operator*(const aint& other) const {
    aint res(*this);
    res *= other;
    return res;
}
aint aint::operator%(const aint& other) const {
    aint res(*this);
    res %= other;
    return res;
}
aint aint::operator/(const aint& other) const {
    aint res(*this);
    res /= other;
    return res;
}
aint aint::operator<<(size_t shift) const {
    aint res(*this);
    res <<= shift;
    return res;
}
aint aint::operator>>(size_t shift) const {
    aint res(*this);
    res >>= shift;
    return res;
}
#pragma endregion copy-operators
//</editor-fold>

bool aint::zero() const {
    return begin == nullptr;
}

void aint::swap(aint& other) {
    std::swap(begin, other.begin);
    std::swap(end, other.end);
    std::swap(val_end, other.val_end);
}

void aint::read(std::istream& in) {
    clear();
    char c = static_cast<char>(in.get());
    while (in && isspace(c)) c = static_cast<char>(in.get());

    uint32_t block = 0;
    uint32_t bits = 0;
    for (; in && !isspace(c); c = static_cast<char>(in.get())) {
        if (c != '0' && c !='1') {
            in.setstate(std::ios::failbit);
            free(begin);
            begin = val_end = end = nullptr;
            return;
        }

        block += (c - '0') << bits++;
        if (bits == BLK_SIZE) {
            add_block(block);
            block = 0;
            bits = 0;
        }
    }

    add_block(block);
    trim_zeros();
}

void aint::print(std::ostream& out) const {
    if (zero()) {
        out << 0;
        return;
    }

    for (auto i = begin; i != val_end; ++i) {
        auto block = *i;
        for (size_t j = 0; j < BLK_SIZE; ++j) {
            if (block % 2) out << '1';
            else out << '0';
            block >>= 1;

            if (!block && (i+1 == val_end)) return;     // skipping the MSb 0s on last number
        }
    }
}

std::istream& operator>>(std::istream& in, aint& val) {
    val.read(in);
    return in;
}

std::ostream& operator<<(std::ostream& out, const aint& val) {
    val.print(out);
    return out;
}