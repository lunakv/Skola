#ifndef MOVABLE_PTR_HPP
#define MOVABLE_PTR_HPP

template<typename T> class movable_ptr; // forward declaration

// Base class for objects referencable by movable_ptr.
// Pointers are tracked as a cyclic doubly linked list
template<typename T>
class enable_movable_ptr {

friend class movable_ptr<T>;
private:
    movable_ptr<T> *first;  // first in list of pointers to this item

    // point all movable pointers from e to this
    void redirectToThis(enable_movable_ptr<T> &&e) {
        // we assume no movable_ptr to this exist when calling this function
        first = e.first;
        if (first == nullptr) return;

        first->trg = this;
        for (auto i = first->next; i != first; i = i->next)
            i->trg = this;

        e.first = nullptr;  // otherwise we'd lose our pointers when e dies
    }

    // reset all movable_ptrs pointing to this object
    void resetMovables() {
        auto i = first;
        if (!i) return;
        // save the reference to next, then reset current. repeat for each node.
        do {
            auto next = i->next;
            i->trg = nullptr;    // we don't use the reset() function for performance reasons
            i->prev = i;         // it would unnecessarily redirect the temporarily wrong links
            i->next = i;
            i = next;
        } while (i != first);

        first = nullptr;
    }

protected:      // this class isn't intended for direct instantiation
    enable_movable_ptr() noexcept : first(nullptr) {};

    // pointers aren't interested in copies
    enable_movable_ptr(const enable_movable_ptr<T> &) noexcept : first(nullptr) {}

    enable_movable_ptr(enable_movable_ptr<T> &&e) noexcept {
        redirectToThis(std::move(e));
    }

public:
    // since assignment removes the current object, we reset all pointers to it.
    enable_movable_ptr<T> &operator=(const enable_movable_ptr<T> &obj) {
        if (&obj != this)
            resetMovables();
        return *this;
    }

    enable_movable_ptr<T> &operator=(enable_movable_ptr<T> &&obj) noexcept {
        if (&obj != this)
            resetMovables();
        redirectToThis(std::move(obj));
        return *this;
    }

    virtual ~enable_movable_ptr() {
        resetMovables();
    }
};

// Pointer tracking movements of target object
template<typename T>
class movable_ptr {

friend class enable_movable_ptr<T>; // we assume T inherits from enable_movable_ptr<T>
private:
    // we track all pointers to the same object as a cyclic doubly linked list.
    // the list is doubly linked to allow for constant-time removal of pointers
    // if this is the only pointer to an object (or trg == nullptr), prev == next == this.
    movable_ptr<T> *prev;
    movable_ptr<T> *next;
    enable_movable_ptr<T> *trg;

    // add this to the list of pointers to trg
    void registerMovable() {
        if (!trg) return;
        auto old_fst = trg->first;
        trg->first = this;
        if (!old_fst) return;
        next = old_fst;
        prev = old_fst->prev;
        next->prev = this;
        prev->next = this;
    }

public:
    movable_ptr() {
        trg = nullptr;
        prev = next = this;
    }

    explicit movable_ptr(T *ptr) {
        prev = next = this;
        trg = dynamic_cast<enable_movable_ptr<T> *>(ptr);
        registerMovable();
    }

    explicit movable_ptr(enable_movable_ptr<T> &obj) {
        prev = next = this;
        trg = &obj;
        registerMovable();
    }

    movable_ptr(movable_ptr<T> &ptr) {
        prev = next = this;
        trg = ptr.trg;
        registerMovable();
    }

    movable_ptr(movable_ptr<T> &&ptr) noexcept : trg(ptr.trg) {
        prev = next = this;
        registerMovable();
        ptr.reset();
    }

    movable_ptr<T> &operator=(const movable_ptr<T> &ptr) {
        reset(dynamic_cast<T *>(ptr.trg));
        return *this;
    }

    movable_ptr<T> &operator=(movable_ptr<T> &&ptr) noexcept {
        if (&ptr != this) {
            reset(dynamic_cast<T *>(ptr.trg));
            ptr.reset();
        }
        return *this;
    }

    ~movable_ptr() {
        reset();
    }

    T * get() const { return dynamic_cast<T*>(trg); }

    void reset() {
        if (trg == nullptr) return;
        // clear trg->first if this is the last pointer to it
        trg->first = (next == this) ? nullptr : next;
        next->prev = prev;
        prev->next = next;
        next = prev = this;
        trg = nullptr;
    }

    void reset(T *ptr) {
        reset();
        trg = dynamic_cast<enable_movable_ptr<T> *>(ptr);
        registerMovable();
    }

    /** OPERATORS **/

    T &operator*() const { return *get(); }

    T *operator->() const { return get(); }

    bool operator!() const { return !trg; }

    explicit operator bool() const { return trg != nullptr; }

    bool operator==(const movable_ptr<T> &other) const {
        return trg == other.trg;
    }

    bool operator !=(const movable_ptr<T> &other) const {
        return !(*this == other);
    }

    bool operator==(T *ptr) const {
        return ptr == trg;
    }

    bool operator!=(T *ptr) const {
        return !(*this == ptr);
    }

};

// create a movable_ptr from object
template<typename T>
movable_ptr<T> get_movable(enable_movable_ptr<T> &trg) {
    return movable_ptr<T>(trg);
}

#endif //MOVABLE_PTR_HPP

