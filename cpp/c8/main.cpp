#include <iostream>
#include <vector>

using namespace std;

template <typename T>
class SVector : public vector<T> {
    class RefProxy {
        T* ref;
    public:
        RefProxy(T* r = nullptr) : ref(r) {}
        RefProxy& operator=(const T& t) {
            if (ref) *ref = t;
            return *this;
        }
        RefProxy& operator=(const T&& t) {
            if (ref) *ref = move(t);
            return *this;
        }
        operator T() {
            if (ref) return *ref;
            return T();
        }
    };

    class SizeProxy {
        SVector* parent_ref() {
            return reinterpret_cast<SVector*>(reinterpret_cast<char*>(this) - offsetof(SVector<T>, count));
        }
    public:
        SizeProxy& operator=(size_t s) {
            parent_ref()->resize(s);
            return *this;
        }
        operator size_t() {
            return parent_ref()->size();
        }
    };

public:
    T operator[](size_t index) {
        if (index < this->size()) return RefProxy(this->data() + index);
        else return T();
    }
    SizeProxy count;
};

void f(int i, float j) {
    cout << i << endl;
    cout << j << endl;
}
template<typename F, typename ... Args>
void f2(F f, Args... args) {
    f(args...);
    f(args...);
}

int main() {
    SVector<int> s,t;
    s = t;
    s.count = 5;
    s.push_back(42);
    cout << s[0] << endl;
    cout << s[5] << endl;
    cout << s[10] << endl << endl;
    f2(f, 2, 3);
    return 0;
}