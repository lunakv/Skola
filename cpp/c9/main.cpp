#include <iostream>
#include <utility>
#include <vector>
#include <memory>
#include <map>

using namespace std;
enum class token_type {
    integer,
    plus,
    times,
    paren_l,
    paren_r,
    ident,
    in,
    out,
    assign,
    semicolon
};

struct Token {
    token_type type;
    int value;
    string name;
    Token(token_type type, int value = 0) : type(type), value(value), name("") {}
    Token(string&& name) : type(token_type::ident), value(0), name(move(name)) {}
    string format() {
        switch (type) {
            case token_type::integer:
                return "integer: " + to_string(value);
            case token_type::plus:
                return "+";
            case token_type::times:
                return "*";
            case token_type::paren_l:
                return "(";
            case token_type::paren_r:
                return ")";
            case token_type::ident:
                return name;
            case token_type::in:
                return ">";
            case token_type::out:
                return "<";
            case token_type::assign:
                return "=";
            case token_type::semicolon:
                return ";";
        }
    }
};



vector<Token> tokenize(istream& in) {
    char c;
    vector<Token> res;
    while (in) {
        c = in.peek();
        if (c < 0) break;
        if (isspace(c)) {
            in.get();
            continue;
        }
        if (isdigit(c)) {
            int n;
            in >> n;
            res.emplace_back(token_type::integer, n);
            continue;
        }
        if (islower(c)) {
            string s;
            while (islower(in.peek()))
                s += in.get();
            res.emplace_back(move(s));
            continue;
        }
        switch (c) {
            case '+':
                res.emplace_back(token_type::plus);
                break;
            case '*':
                res.emplace_back(token_type::times);
                break;
            case '(':
                res.emplace_back(token_type::paren_l);
                break;
            case ')':
                res.emplace_back(token_type::paren_r);
                break;
            case '>':
                res.emplace_back(token_type::in);
                break;
            case '<':
                res.emplace_back(token_type::out);
                break;
            case '=':
                res.emplace_back(token_type::assign);
                break;
            case ';':
                res.emplace_back(token_type::semicolon);
                break;
            default:
                cerr << "Unexpected character: " << int(c) << endl;
        }
        in.get();
    }
    return res;
}

using Scope = map<string, int>;

class Expr {
public:
    virtual int eval(const Scope& s) const = 0;
    virtual string format() const = 0;
};

class Var : public Expr {
    string name;
public:
    Var(string  name) : name(move(name)) {}
    string format() const override {
        return name;
    }
    int eval(const Scope& s) const override {
        auto v = s.find(name);
        if (v == s.end()) {
            cerr << "Neznama promenna: " << name << endl;
            return 0;
        }
        return v->second;
    }
};

class Num : public Expr {
    int value;
public:
    Num(int n) : value(n) {}
    int eval(const Scope& s) const override {
        return value;
    }
    string format() const override {
        return to_string(value);
    }
};

class Add : public Expr {
    unique_ptr<Expr> l,r;
public:
    Add(unique_ptr<Expr>&& l, unique_ptr<Expr>&& r) : l(move(l)), r(move(r)) {}
    int eval(const Scope& s) const override {
        return l->eval(s) + r->eval(s);
    }
    string format() const override {
        return "(" + l->format() + " + " + r->format() + ")";
    }
};

class Mul : public Expr {
    unique_ptr<Expr> l,r;
public:
    Mul(unique_ptr<Expr>&& l, unique_ptr<Expr>&& r) : l(move(l)), r(move(r)) {}
    int eval(const Scope& s) const override {
        return l->eval(s) * r->eval(s);
    }
    string format() const override {
        return l->format() + " * " + r->format();
    }
};

class Prog {
public:
    virtual void exec(Scope& s) const = 0;
};

class Seq : public Prog {
    unique_ptr<Prog> l, r;
public:
    Seq(unique_ptr<Prog>&& l, unique_ptr<Prog>&& r) : l(move(l)), r(move(r)) {}
    void exec(Scope& s) const override {
        l->exec(s);
        r->exec(s);
    }
};

class Out : public Prog {
    string name;
public:
    Out(string  name) : name(move(name)) {}
    void exec(Scope& s) const override {
        auto v = s.find(name);
        if (v == s.end())
            cout << "???" << endl;
        else
            cout << v->second << endl;
    }
};

class In : public Prog {
    string name;
public:
    In(string  name) : name(move(name)) {}
    void exec(Scope& s) const override {
        cin >> s[name];
    }
};

class Assign : public Prog {
    string name;
    unique_ptr<Expr> val;
public:
    Assign(string l, unique_ptr<Expr>&& r) : name(move(l)), val(move(r)) {}
    void exec(Scope& s) const override {
        s[name] = val->eval(s);
    }
};

using Pos = vector<Token>::iterator;

unique_ptr<Expr> parse_expr(Pos& begin, Pos end);

unique_ptr<Expr> parse_simple_expr(Pos& begin, Pos end) {
    if (begin == end) return nullptr;
    if (begin->type == token_type::integer) {
        auto ret = make_unique<Num>(begin->value);
        begin++;
        return ret;
    }
    if (begin->type == token_type::ident) {
        auto ret = make_unique<Var>(begin->name);
        begin++;
        return ret;
    }
    if (begin->type == token_type::paren_l) {
        auto orig_begin = begin;
        ++begin;

        unique_ptr<Expr> e = parse_expr(begin, end);
        if (!e || begin == end || begin->type != token_type::paren_r) {
            begin = orig_begin;
            return nullptr;
        }

        ++begin;
        return e;
    }
    return nullptr;
}
unique_ptr<Expr> parse_mul_expr(Pos& begin, Pos end) {
    auto l = parse_simple_expr(begin, end);
    if (!l || begin == end) return l;
    if (begin->type != token_type::times) return l;

    begin++;
    auto r = parse_mul_expr(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<Mul>(move(l), move(r));
}
unique_ptr<Expr> parse_add_expr(Pos& begin, Pos end) {
    auto l = parse_mul_expr(begin, end);
    if (!l || begin == end) return l;
    if (begin->type != token_type::plus) return l;

    begin++;
    auto r = parse_add_expr(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<Add>(move(l), move(r));
}
unique_ptr<Expr> parse_expr(Pos& begin, Pos end) {
    return parse_add_expr(begin, end);
}

unique_ptr<Prog> parse_simple_prog(Pos& begin, Pos end) {
    if (begin == end) return nullptr;
    if (begin->type == token_type::out) {
        begin++;
        if (begin == end || begin->type != token_type::ident) {
            --begin;
            return nullptr;
        }
        return make_unique<Out>((begin++)->name);
    }
    if (begin->type == token_type::in) {
        begin++;
        if (begin == end || begin->type != token_type::ident) {
            --begin;
            return nullptr;
        }
        return make_unique<In>((begin++)->name);
    }
    if (begin->type == token_type::assign) {
        begin++;
        if (begin == end || begin->type != token_type::ident) {
            --begin;
            return nullptr;
        }
        string n = (begin++)->name;
        unique_ptr<Expr> e = parse_expr(begin, end);
        if (!e) {
            --begin; --begin;
            return nullptr;
        }
        return make_unique<Assign>(n, move(e));
    }
    return nullptr;
}

unique_ptr<Prog> parse_prog(Pos& begin, Pos end) {
    auto l = parse_simple_prog(begin, end);
    if (!l || begin == end) return l;
    if (begin->type != token_type::semicolon) return l;

    begin++;
    auto r = parse_prog(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<Seq>(move(l), move(r));
}

int main() {
    vector<Token> v = tokenize(cin);
    auto begin = v.begin();
    auto end = v.end();
    auto e = parse_prog(begin, end);
    Scope s;
    s["pi"] = 3;
    if (e && begin == end) {
        e->exec(s);
    }
    else
        cout << "Chyba vstupu" << endl;

    for (auto & i : s)
        cout << i.first << ": " << i.second << endl;
    return 0;
}