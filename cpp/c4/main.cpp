#include <iostream>
#include <ostream>

using namespace std;

class Str {
    char* s;
public:
    Str() : s(nullptr) {}
    Str(const Str& val) : Str() {
        assign(val.s);
    }
    Str(Str&& val) : Str() {
        s = val.s;
        val.s = nullptr;
        cout << "move" << endl;
    }
    Str(const Str& val, size_t offset, size_t len) {
        size_t length = val.length() - offset;
    }

    ~Str() {
        clear();
    }

    Str& operator=(const char* val) {
        assign(val);
        return *this;
    }

    Str& operator=(const Str& val) {
        assign(val.s);
        return *this;
    }

    Str& operator=(Str&& val) {
        swap(s, val.s);
        return *this;
    }

    Str operator+(const Str& val) const {
        char* ns = new char[length() + val.length() + 1];
        char* i = ns;
        for(char* si = s; *si; ++i,++si) *i = *si;
        for(char* vi = val.s; *vi; ++i,++vi) *i = *vi;
        *i = 0;
        Str ret;
        ret.s = ns;
        return ret;
    }

    const char& operator[](size_t ind) const {
        return s[ind];
    }

    char& operator[](size_t ind) {
        return s[ind];
    }

    void assign(const char* val) {
        cout << "copy" << endl;
        clear();
        size_t len;
        for(len = 0; val[len]; ++len);
        s = new char[len+1];
        for(size_t i = 0; i <= len; ++i)
            s[i] = val[i];
    }
    void clear() {
        delete s;
    }
    size_t length() const {
        size_t len;
        for (len = 0; s[len]; ++len);
        return len;
    }

    const char* c_str() const {
        return s;
    }


};

ostream& operator <<(ostream& o, const Str& val)  {
    return o << val.c_str();
}

int main() {
    Str a,b;
    a = b = "ahoj";
    a = a+b;
    cout << a+b << endl;
    cout << a+b << endl;
    cout << (a+b)[5] << endl;
    return 0;
}