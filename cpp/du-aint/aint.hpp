#include <cstdint>
#include <cstddef>
#include <iostream>

#define BASE_SIZE 4
#define BLK_SIZE 32

class aint {
private:
    uint32_t *begin, *val_end, *end;
    size_t block_count() const;
    void clear();
    void create_empty();
    void copy(const aint& from);
    void double_size();
    void add_block(uint32_t val);
    aint& trim_zeros();
public:
    aint();
    ~aint();
    aint(uint32_t val);
    aint(const aint& from);
    aint(aint&& from) noexcept;
    aint& operator =(uint32_t val);
    aint& operator =(const aint& from);
    aint& operator =(aint&& from) noexcept;

    bool operator <(const aint& other) const;
    bool operator >(const aint& other) const;
    bool operator ==(const aint& other) const;
    bool operator !=(const aint& other) const;
    bool operator <=(const aint& other) const;
    bool operator >=(const aint& other) const;

    aint& operator +=(const aint& other);
    aint& operator -=(const aint& other);
    aint& operator *=(const aint& other);
    aint& operator /=(const aint& other);
    aint& operator %=(const aint& other);
    aint& operator <<=(size_t shift);
    aint& operator >>=(size_t shift);

    aint operator +(const aint& other) const;
    aint operator -(const aint& other) const;
    aint operator *(const aint& other) const;
    aint operator /(const aint& other) const;
    aint operator %(const aint& other) const;
    aint operator <<(size_t shift) const;
    aint operator >>(size_t shift) const;

    bool zero() const;
    void swap(aint& other);
    void read(std::istream& in);
    void print(std::ostream& out) const;
};

std::istream& operator >>(std::istream& in, aint& val);
std::ostream& operator <<(std::ostream& out, const aint& val);

