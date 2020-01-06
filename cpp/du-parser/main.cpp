
#include <iostream>
#include <memory>
#include <string>
#include <utility>
#include <vector>
using namespace std;

/*
 * tokenizer
 */

enum class token_type
{
    integer,
    identifier,
    single_char
};

struct Token
{
    token_type type;
    int value;
    string name;
    explicit Token(int value = 0)
            : type(token_type::integer)
            , value(value)
    {}
    explicit Token(char token_char)
            : type(token_type::single_char)
            , value(0)
            , name(1, token_char)
    {}
    explicit Token(string &&name)
            : type(token_type::identifier)
            , value(0)
            , name(name)
    {}

    bool isIdent()
    {
        return this->type == token_type::identifier;
    }
    bool isInt()
    {
        return this->type == token_type::integer;
    }
    bool is(char op)
    {
        return this->type == token_type::single_char && this->name[0] == op;
    }

    string format()
    {
        switch (type) {
            case token_type::integer:
                return to_string(value);
            case token_type::identifier:
            case token_type::single_char:
                return name;
            default:
                return "<unknown_token>";
        }
    }
};

vector<Token>
tokenize(istream &in)
{
    vector<Token> res;
    while (in) {
        char c = in.peek();
        if (c < 0) {
            continue;
        }
        if (isspace(c)) {
            in.get();
            continue;
        }
        if (isdigit(c)) {
            int n;
            in >> n;
            res.emplace_back(n);
            continue;
        }
        if (islower(c)) {
            string s;
            while (islower(in.peek())) {
                s += char(in.get());
            }
            res.emplace_back(move(s));
            continue;
        }

        switch (c) {
            // list of recognized characters
            // Expr
            case '+':
            case '-':
            case '*':
            case '(':
            case ')':
            // Prog
            case '<':
            case '>':
            case ';':
            case '=':
            case '?':
            case '!':
            case '@':
            case '{':
            case '}':
                res.emplace_back(c);
                break;
            default:
                // returns empty vector on unrecognized character
                return vector<Token>();
        }
        in.get();
    }
    return res;
}

/*
 * interfaces for AST
 * (very abstract now)
 */

// Conversion to stack-assembly returns a vector of strings
// Each element represents one instruction
class Expr
{
public:
    virtual vector<string> compile() = 0;
};

class Prog
{
public:
    virtual vector<string> compile() = 0;
};

/*
 * program implementations
 */
class Seq : public Prog
{
    unique_ptr<Prog> left, right;

public:
    Seq(unique_ptr<Prog> &&left, unique_ptr<Prog> &&right)
            : left(move(left))
            , right(move(right))
    {}

    vector<string> compile() override
    {
        vector<string> res;
        vector<string> tmp = left->compile();
        res.insert(
                res.end(),
                make_move_iterator(tmp.begin()),
                make_move_iterator(tmp.end())
            );
        tmp = right->compile();
        res.insert(
                res.end(),
                make_move_iterator(tmp.begin()),
                make_move_iterator(tmp.end())
            );
        return res;
    }
};

class Out : public Prog
{
    string name;

public:
    explicit Out(string name)
            : name(move(name))
    {}

    vector<string> compile() override
    {
        vector<string> res;
        res.push_back("LOADVAR " + name);
        res.emplace_back("WRITE");
        return res;
    }
};

class In : public Prog
{
    string name;

public:
    explicit In(string name)
            : name(move(name))
    {}

    vector<string> compile() override
    {
        vector<string> res;
        res.emplace_back("READ");
        res.push_back("STOREVAR " + name);
        return res;
    }
};

class Assign : public Prog
{

    string name;
    unique_ptr<Expr> right;

public:
    Assign(string name, unique_ptr<Expr> &&right)
            : name(move(name))
            , right(move(right))
    {}

    vector<string> compile() override
    {
        vector<string> res(move(right->compile()));
        res.push_back("STOREVAR " + name);
        return res;
    }
};

// Conditional - if/!if/while
class Cond : public Prog
{
    char type; // 'T' for if, 'N' for !if, 'W' for while
    string name;
    unique_ptr<Prog> code;

public:
    Cond(char type, string name, unique_ptr<Prog> &&code)
            : type(type)
            , name(move(name))
            , code(move(code))
    {}

    vector<string> compile() override
    {
        vector<string> res;
        res.emplace_back("LOADVAR " + name);
        auto tmp = code->compile();
        // 'while' needs one more instruction for the jump back
        string offset = to_string(tmp.size() + (type == 'W'?2:1));
        string jmp = (type == 'N' ? "JMPT " : "JMPF ") + offset;
        res.emplace_back(jmp);
        res.insert(
                res.end(),
                make_move_iterator(tmp.begin()),
                make_move_iterator(tmp.end())
            );

        if (type == 'W')
            res.emplace_back("JMP -" + offset);

        return res;
    }
};
/*
 * expressions
 */

class Num : public Expr
{
    int val;

public:
    explicit Num(int v)
            : val(v)
    {}

    vector<string> compile() override
    {
        return vector<string>(1, "INT " + to_string(val));
    }
};

class Var : public Expr
{
    string name;

public:
    explicit Var(string v)
            : name(move(v)) {}

    vector<string> compile() override
    {
        return vector<string>(1, "LOADVAR " + name);
    }
};

class BinOp : public Expr
{
    unique_ptr<Expr> left, right;
    string op;

public:
    BinOp(unique_ptr<Expr> &&left, unique_ptr<Expr> &&right, string op)
            : left(move(left))
            , right(move(right))
            , op(move(op))
    {}

    vector<string> compile() override
    {
        vector<string> res(move(left->compile()));
        auto tmp = right->compile();
        res.insert(
                res.end(),
                make_move_iterator(tmp.begin()),
                make_move_iterator(tmp.end())
        );
        res.push_back(op);
        return res;
    }
};

/*
 * Parsing
 */

using Pos = vector<Token>::iterator;

unique_ptr<Expr>
ParseExpr(Pos &begin, Pos end);

unique_ptr<Expr>
ParseSimpleExpr(Pos &begin, Pos end)
{
    if (begin == end)
        return nullptr;
    if (begin->isInt()) {
        unique_ptr<Expr> val = make_unique<Num>(begin->value);
        begin++;
        return val;
    }

    if (begin->isIdent()) {
        unique_ptr<Expr> val = make_unique<Var>(begin->name);
        begin++;
        return val;
    }

    if (begin->is('(')) {
        auto original_begin = begin;
        begin++;

        unique_ptr<Expr> e = ParseExpr(begin, end);
        if (!e || begin == end ||
            !begin->is(')')) {
            begin = original_begin;
            return nullptr;
        }

        begin++;
        return e;
    }

    return nullptr;
}

unique_ptr<Expr>
ParseMulExpr(Pos &begin, Pos end)
{
    unique_ptr<Expr> l = ParseSimpleExpr(begin, end);
    if (!l)
        return l;
    if (begin == end)
        return l;
    if (!begin->is('*'))
        return l;
    begin++;
    unique_ptr<Expr> r = ParseMulExpr(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<BinOp>(move(l), move(r), "MULT");
}

unique_ptr<Expr>
ParseAddExpr(Pos &begin, Pos end)
{
    unique_ptr<Expr> l = ParseMulExpr(begin, end);
    if (!l)
        return l;
    if (begin == end)
        return l;

    string op;
    if (begin->is('+'))
        op = "ADD";
    else if (begin->is('-'))
        op = "SUB";
    else
        return l;

    begin++;
    unique_ptr<Expr> r = ParseAddExpr(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<BinOp>(move(l), move(r), move(op));
}

unique_ptr<Expr>
ParseExpr(Pos &begin, Pos end)
{
    return ParseAddExpr(begin, end);
}

unique_ptr<Prog>
ParseProg(Pos &begin, Pos end);

unique_ptr<Prog>
ParseSimpleProg(Pos &begin, Pos end)
{
    if (begin == end)
        return nullptr;
    if (begin->is('<')) {
        begin++;
        if (begin == end || !begin->isIdent()) {
            begin--;
            return nullptr;
        }

        return make_unique<Out>((begin++)->name);
    }
    if (begin->is('>')) {
        begin++;
        if (begin == end || !begin->isIdent()) {
            begin--;
            return nullptr;
        }

        return make_unique<In>((begin++)->name);
    }
    if (begin->is('=')) {
        begin++;
        if (begin == end || !begin->isIdent()) {
            begin--;
            return nullptr;
        }
        string n = (begin++)->name;
        unique_ptr<Expr> e = ParseExpr(begin, end);
        if (!e) {
            begin -= 2;
            return nullptr;
        }

        return make_unique<Assign>(n, move(e));
    }
    // parsing '{' is almost identical to parsing '('
    if (begin->is('{')) {
        auto original_begin = begin;  // if inner program parses, but '}' doesn't follow, begin needs to be put back
        begin++;

        unique_ptr<Prog> p = ParseProg(begin, end);
        if (!p || begin == end || !begin->is('}')) {
            begin = original_begin;
            return nullptr;
        }
        begin++;
        return p;
    }

    // parsing conditionals
    char cond = '\0';
    if (begin->is('?')) cond = 'Y';
    else if (begin->is('!')) cond = 'N';
    else if (begin->is('@')) cond = 'W';
    if (cond != '\0') {
        begin++;
        if (begin == end || !begin->isIdent()) {
            begin--;
            return nullptr;
        }
        string n = (begin++)->name;
        unique_ptr<Prog> p = ParseSimpleProg(begin, end);
        if (!p) {
            begin -= 2;
            return nullptr;
        }

        return make_unique<Cond>(cond, n, move(p));
    }

    return nullptr;
}

unique_ptr<Prog>
ParseProg(Pos &begin, Pos end)
{
    unique_ptr<Prog> l = ParseSimpleProg(begin, end);
    if (!l)
        return l;
    if (begin == end)
        return l;
    if (!begin->is(';'))
        return l;

    begin++;
    unique_ptr<Prog> r = ParseProg(begin, end);
    if (!r) {
        begin--;
        return l;
    }
    return make_unique<Seq>(move(l), move(r));
}

/*
 * the main program
 */

int
main()
{
    auto tokens = tokenize(cin);
    if (tokens.empty()) {
        cout << "FAIL" << endl;
        return 0;
    }
    auto b = tokens.begin();
    auto e = ParseProg(b, tokens.end());
    if (b != tokens.end()) {
        cout << "FAIL" << endl;
        return 0;
    }

    auto cmp = e->compile();
    for (auto & i : cmp) {
        cout << i << endl;
    }
    cout << "QUIT" << endl;
    return 0;
}
