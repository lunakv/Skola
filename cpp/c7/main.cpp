#include <iostream>
#include <list>
#include <memory>
using namespace std;

/*
struct Base {
    int b;
    virtual void writeB() {
        cout << b << endl;
    }
};

struct P : Base {
    void writeB() override {
        cout << "p.b: " << b << endl;
    }

};

void f(Base* b) {
    b->writeB();
}

int main() {
    P p;
    p.b = 20;
    p.writeB();
    f(&p);
    cout << sizeof(p) << endl;
}*/

class Zvire {
public:
    virtual ~Zvire() = default;
    virtual void zvuk() = 0;
};

class Tulen : public Zvire {
    int kulatost;
public:
    Tulen(int k) : kulatost(k) {}
    ~Tulen() {
        cout << "byyyye - tulen" << endl;
    }
    void zvuk() {
        cout << "Tulen s kulatosti " << kulatost << " rika ahoj" << endl;
    }
};

class Had : public Zvire {
    int delka;
public:
    Had(int d) : delka(d) {}
    ~Had() {
        cout << "sssssssss - had" << endl;
    }
    void zvuk() {
        cout << "Had delky " << delka << " rika SSssss" << endl;
    }
};

class Jedlik : public Zvire {
    unique_ptr<Zvire> jidlo;
public:
    Jedlik(unique_ptr<Zvire> j) {
        jidlo = move(j);
    }
    ~Jedlik() {
        cout << "mnam - jedlik" << endl;
    }
    void zvuk() {
        cout << "Jedlik rika chramst. Co na to potrava? ";
        jidlo->zvuk();
    }
};

class Pavilon : public Zvire {
    list<unique_ptr<Zvire>> zvirata;
public:
    void pridej_zvire(unique_ptr<Zvire> z) {
        zvirata.push_back(move(z));
    }
    void zvuk() {
        cout << "Z pavilonu se ozvalo: " << endl;
        for (auto & i : zvirata)
            i->zvuk();
        cout << "... a vic uz nic" << endl;
    }
};

int main () {
    list<unique_ptr<Zvire>> zoo;
    zoo.push_back(make_unique<Tulen>(1234));
    zoo.push_back(make_unique<Had>(14));
    zoo.push_back(make_unique<Jedlik>(make_unique<Had>(5678)));

    unique_ptr<Pavilon> p = make_unique<Pavilon>();
    p->pridej_zvire(make_unique<Tulen>(42));
    zoo.push_back(move(p));

    for (auto & i : zoo)
        i->zvuk();

    size_t tuleni = 0;
    for (auto & i : zoo) {
        Tulen* t = dynamic_cast<Tulen*>(i.get());
        if (t)
            ++tuleni;
    }
    cout << "Tulenu: " << tuleni << endl;

    return 0;
}


