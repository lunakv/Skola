#include <iostream>
#include <vector>
#include <memory>

using namespace std;
enum class token_type {
    integer,
    plus,
    times,
    paren_l,
    paren_r,
};

struct Token {
    token_type type;
    int value;
    Token(token_type type, int value = 0) : type(type), value(value) {}
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
            default:
                cerr << "Unexpected character: " << int(c) << endl;
        }
        in.get();
    }
    return res;
}

class Expr {
public:
    virtual int eval() const = 0;
    virtual string format() const = 0;
};

class Num : public Expr {
    int value;
public:
    Num(int n) : value(n) {}
    int eval() const override {
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
    int eval() const override {
        return l->eval() + r->eval();
    }
    string format() const override {
        return "(" + l->format() + " + " + r->format() + ")";
    }
};

class Mul : public Expr {
    unique_ptr<Expr> l,r;
public:
    Mul(unique_ptr<Expr>&& l, unique_ptr<Expr>&& r) : l(move(l)), r(move(r)) {}
    int eval() const override {
        return l->eval() * r->eval();
    }
    string format() const override {
        return l->format() + " * " + r->format()ě;
    }
};

using Pos = vector<Token>::iterator;
unique_ptr<Expr> parse_simple_expr(Pos& begin, Pos end) {
    if (begin == end) return nullptr;
    if (begin->type == token_type::integer) {
        auto ret = make_unique<Num>(begin->value);
        begin++;
        return move(ret);
    }
    if (begin->type == token_type::paren_l) {

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

int main() {
    vector<Token> v = tokenize(cin);
    auto begin = v.begin();
    auto end = v.end();
    auto e = parse_expr(begin, end);
    cout << e->format() << endl;
    cout << e->eval() << endl;
    return 0;
}