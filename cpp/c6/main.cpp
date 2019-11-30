//
// Created by vaasa on 11/6/19.
//
#include <iostream>
#include <map>
#include <fstream>
#include <string>

using namespace std;

int main() {
    ifstream in("slovnik.txt");
    map<string, string> dict;
    for (;;) {
        string a,b;
        in >> a >> b;
        if (!in) break;
        dict.insert(make_pair(a,b));
    }

    //for (auto& i : dict)
    //    cout << i.first << ' ' << i.second << endl;

    for (string s; cin >> s;) {
        auto i = --dict.upper_bound(s);
        if (i->first == s.substr(0, i->first.length()))
            cout << i->second << s.substr(i->first.length()) << ' ';
        else
            cout << s << ' ';
    }
    return 0;
}
