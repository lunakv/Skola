cmake_minimum_required(VERSION 3.16)
project(lemmings)

set(CMAKE_CXX_STANDARD 17)
find_package(Threads REQUIRED)
add_executable(lemmings lemmings.cpp stencil1d.hpp worker.hpp)
target_link_libraries(lemmings PRIVATE Threads::Threads)
