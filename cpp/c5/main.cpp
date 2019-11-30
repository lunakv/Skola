#include <iostream>
#include <map>
#include <string>
#include <sstream>
#include <list>
#include <set>
//              index   rand.add    iterator    modifikace
// string       1       n                       front,back,push_,pop_
// vector       1       n                       emplace_
// list         x       1
// deque                A(log)
// (multi)map   k(logn) log         pair        insert(k,v), erase(k)
// (multi)set   k(log)  log
//
// pair<a,b> ... first,second
using namespace std;

int main() {
    multiset<int> s;
    for (int i; cin >> i; s.insert(i));
    for (auto i : s)
        cout << i << endl;
    return 0;
}


template <typename T>
bool atLeast2(const T& cnt) {
    auto i =  cnt.begin();
    if (i == cnt.end()) return false;
    return ++i != cnt.end();
}

template <typename T>
list<T> merge(list<T>&& a, list<T>&& b) {
    list<T> c;
    while (!a.empty() && !b.empty())
        if (a.front() < b.front()) {
            c.push_back(move(a.front()));
            a.pop_front();
        } else {
            c.push_back(move(b.front()));
            b.pop_front();
        }

    while (!a.empty()) {
        c.push_back(move(a.front()));
        a.pop_front();
    }
    while (!b.empty()) {
        c.push_back(move(b.front()));
        b.pop_front();
    }

    return c;
}

template <typename T>
void mergesort(list<T>& seznam) {
    if (seznam.empty()) return;

    list<list<T>> lst;
    for (auto &t : seznam) {
        list<T> item;
        item.push_back(move(t));
        lst.push_back(move(item));
    }

    while (atLeast2(lst)) {
        list<T> a = move(lst.front());
        lst.pop_front();
        list<T> b = move(lst.front());
        lst.pop_front();
        lst.push_back(merge(move(a), move(b)));
    }

    seznam = move(lst.front());
}

int main1() {
    list<string> lst;
    for (string str; getline(cin, str); ) {
        lst.emplace_back(move(str));
    }
    mergesort(lst);
    for (auto & s : lst) {
        cout << s << endl;
    }
    return 0;
}

int main0() {
    map<string, int> hist;

    for (string inp; getline(cin, inp);)
    {
        stringstream sstream(inp);
        for(string word; sstream >> word; )
            ++hist[word];
    }

    for(auto & i : hist)
    {
        cout << i.first << ' ';
        for(int j = 0; j < i.second; ++j)
            cout << '*';
        cout << endl;
    }
    return 0;
}

