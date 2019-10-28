#include <stdlib.h>
#include <iostream>
#include <fstream>

template <typename T>
struct Comp {
    T re;
    T im;

    Comp(T re, T im) : re(re), im(im) {}

    Comp operator+(const Comp &b) const {
        return Comp(re+b.re, im+b.im);
    }


    Comp operator*(T b) const {
        return Comp(re*b, im*b);
    }

    Comp& operator=(const Comp &b) {
        re = b.re;
        im = b.im;
        return *this;
    }

    Comp& operator+=(const Comp &b) {
        return *this = *this + b;
    }

    Comp operator*(const Comp &b) const {
        return Comp(re * b.re - im * b.im, re * b.im + im * b.re);
    }
};

template <typename T, typename I>
Comp<T> operator*(I i, const Comp<T> &c) {
    return c * T(i);
}

template <typename T>
std::ostream& operator<<(std::ostream& os, const Comp<T> &c) {
    return os << c.re << " + i*" << c.im;
}

template <typename T>
std::istream& operator >>(std::istream& is, Comp<T> &c) {
    return is >> c.re >> c.im;
}


template <typename T>
void swap(T &a, T &b) {
    T t = a;
    a = b;
    b = t;
}

using CF = Comp<float>;

int strlen(const char* str) {
    int i = 0;
    while (*str != 0) {
        i++;
        str++;
    }
    return i;
}

int main() {
    std::cout << strlen("abcd");
    CF a(0,0);
    std::cin >> a;
    a = a+a;
    a = 5.0f * a;
    a += CF(0,1);
    std::cout << a << std::endl;

    CF b = CF(0,1) * CF(0,1);
    std::cout << b << std::endl;

    std::ofstream f("out.txt");
    f << a << std::endl;
    f.close();

#if 0
    int a, b;
    float c, d;
    std::cin >> a >> b;
    swap(a, b);
    std:: cout << a << ' ' << b << std::endl;
    std::cin >> c >> d;
    swap(c, d);
    std::cout << c << ' ' << d << std::endl;
#endif
    return 0;
}