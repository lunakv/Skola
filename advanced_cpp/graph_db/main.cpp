#include <iostream>
#include <queue>
#include <variant>

#include "graph_db.hpp"
#include "tests.hpp"

int main(int argc, char *argv[]) {

    if (argc != 2) {
        std::cerr << "Wrong number of arguments";
        //return -1;
    }
    size_t idx = std::stoi(argv[1]);
    test_bench t;
    if (idx > t.test_count()) {
        std::cerr << "Wrong argument";
        return -2;
    }

    if (idx == 0) {
        t.run_all_tests();
    } else {
        t.run_test(idx - 1);
    }
    return 0;
/**/
}