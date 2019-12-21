#include <iostream>
#include <queue>
#include <array>
#include <set>
#include <map>
#include <algorithm>

using State = std::array<int, 9>;
using namespace std;
int main_BROKEN() {
    queue<State> open;
    set<State> visited;
    map<State, State> previous;

    State s;
    for (char i = 0; i < 9; ++i)
        cin >> s[i];

    open.push(s);
    while (!open.empty()) {
        State curr = open.front();
        open.pop();
        bool end = true;
        for (char i = 0; i < 9; ++i)
            if (curr[i] != (i == 8 ? 0 : i+1)) {
                end = false;
                break;
            }

        if (end)
        {
            cout << "Hotovo!" << endl;
            for (State i = curr; previous.count(i); i = previous[i])
            {
                for (auto j : i)
                    cout << j << ' ';
                cout << endl;
            }
            return 0;
        }

        if (visited.count(curr)) continue;
        visited.insert(curr);

        int npos = 0;
        for (size_t i = 0; i < 9; ++i)
            if (curr[i] == 0) {
                npos = i;
                break;
            }

        // space goes up, block goes down
        if (npos > 2) {
            State down = curr;
            swap(down[npos-3], down[npos]);
            if (!previous.count(down));
                previous[down] = curr;
            open.push(down);
        }
        // space goes down, block goes up
        if (npos < 6) {
            State up = curr;
            std::swap(up[npos+3], up[npos]);
            if (!previous.count(up))
                previous[up] = curr;
            open.push(up);
        }

        if (npos % 3) {
            State right = curr;
            swap(right[npos-1], right[npos]);
            if (!previous.count(right))
                previous[right] = curr;
            open.push(right);
        }

        if (npos % 3 != 2) {
            State left = curr;
            swap(left[npos+1], left[npos]);
            if (!previous.count(left))
                previous[left] = curr;
            open.push(left);
        }

    }
    cout << "Tudy cesta nevede" << endl;
    return 0;
}

///////////////////////////////////////
int main() {
    map<int, int> groups;
    for (;;)
    {
        int a, b;
        cin >> a;
        if (!a) break;
        cin >> b;
        
        if (groups.count(a))
            a = groups[a];
        else
            groups[a] = a;
        if (groups.count(b))
            b = groups[b];
        else
            groups[b] = b;

        for(auto & i : groups)
            if (i.second == b)
                i.second = a;
    }

    set<int> c;
    for_each(groups.begin(), groups.end(), [&](auto par){c.insert(par.second);});
    for (auto & i : groups) {
        cout << i.first << ": " << i.second << endl;
    }

    cout << "# of groups: " << c.size() << endl;

}
