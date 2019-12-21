#include <iostream>
#include <vector>
#include <algorithm>
#include <stack>
#include <sstream>

using namespace std;

template<typename T, typename P>
void heapsort(vector<T> &vec, P cmp)
{
    make_heap(vec.begin(), vec.end(), cmp);
    for (size_t i = 0; i < vec.size(); ++i)
        pop_heap(vec.begin(), vec.begin() + vec.size() - i, cmp);
}

template<typename T>
void heapsort(vector<T> &vec)
{
    heapsort(vec, less<T>());
}

bool str_rcmp(const string &a, const string &b)
{
    return lexicographical_compare(a.rbegin(), a.rend(), b.rbegin(), b.rend());
}

int main2() {
    vector<string> v;
    string x;
    for (size_t i = 0; i < 10; ++i)
    {
        cin >> x;
        v.push_back(x);
    }
    cout << endl << endl;

    heapsort(v, str_rcmp);
    for (const auto &i : v)
        cout << i << endl;
    return 0;
}

//////////////////////////////////////////////////////////////////

void print_poly(const vector<int> &p) {
    for (size_t i = 0; i < p.size(); ++i)
        cout << p[i] << "x^" << i << ' ';
    cout << endl;
}


int main() {
    stack<vector<int>> s;
    string str;
    while (cin >> str)
    {
        if (str == "p") {
            if (!s.empty()) {
                print_poly(s.top());
                s.pop();
            } else {
                cout << "!" << endl;
            }
        }
        else if (str == "+") {
            if (s.size() >= 2) {
                auto a = s.top(); s.pop();
                auto b = s.top(); s.pop();
                a.resize(max(a.size(), b.size()));
                for (size_t i = 0; i < b.size(); ++i)
                    a[i] += b[i];
                s.push(a);
            } else {
                cout << "!" << endl;
            }
        }
        else if (str == "*") {
            if (s.size() >= 2) {
                auto a = s.top(); s.pop();
                auto b = s.top(); s.pop();
                vector<int> c(a.size() + b.size());
                for (size_t i = 0; i < a.size(); ++i)
                    for (size_t j = 0; j < b.size(); ++j)
                        c[i+j] += a[i] * b[j];
                s.push(c);
            } else {
                cout << "!" << endl;
            }
        }
        else if (str == "d")
        {
            if (!s.empty()) {
                auto a = s.top();
                s.push(a);
            } else {
                cout << "!" << endl;
            }
        }
        else if (str == "x") {
            vector<int> a;
            a.push_back(0);
            a.push_back(1);
            s.push(a);
        }
        else if (!str.empty() && all_of(str.begin(), str.end(), ::isdigit)) {
            stringstream sstream(str);
            vector<int> v;
            int n;
            sstream >> n;
            v.push_back(n);
            s.push(v);
        }
        else {
            cout << "?" << endl;
        }
    }
    return 0;
}
