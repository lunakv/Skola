cmake_minimum_required(VERSION 3.15)
project(movable_ptr)

set(CMAKE_CXX_STANDARD 17)

add_executable(movable_ptr main.cpp CompactableGraph.hpp CompactableGraph.cpp movable_ptr.hpp)