cmake_minimum_required(VERSION 3.15)
project(graph_db)

set(CMAKE_CXX_STANDARD 17)
add_compile_options(-Wall -Wextra -pedantic -Werror)
add_executable(graph_db
        graph_db.hpp
        main.cpp
        tests.hpp
        iterator.hpp proxy.hpp)
