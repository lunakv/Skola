#ifndef GRAPH_DB_ITERATOR_HPP
#define GRAPH_DB_ITERATOR_HPP

#include "graph_db.hpp"
template<class T> class graph_db;

// Collection of iterators for graph_db<T>
// Simple objects that just hold a position value and create a proxy when de-referenced

template<class T>
class iterator {
protected:
    size_t i;        // current index of the iterator
    const graph_db<T> &db;
    explicit iterator(const graph_db<T> &db, size_t i)
        : i(i), db(db)
    { }
public:
    auto &operator++() {
        ++i;
        return *this;
    }
    auto &operator++(int) {
        ++i;
        return *this;
    }
    bool operator==(const iterator<T> &other) const {
        return i == other.i && &db == &other.db;
    }
    bool operator!=(const iterator<T> &other) const {
        return !(*this == other);
    }
};

template<class T>
class vert_iterator : public iterator<T> {
public:
    explicit vert_iterator(const graph_db<T> &db, size_t i)
        : iterator<T>(db, i)
    { }
    auto operator*() const {
        return typename graph_db<T>::vertex_t(iterator<T>::i, const_cast<graph_db<T>&>(iterator<T>::db));
    }
};

template<class T>
class edge_iterator : public iterator<T> {
public:
    explicit edge_iterator(const graph_db<T> &db, size_t i)
        : iterator<T>(db, i)
    { }
    auto operator*() const {
        return typename graph_db<T>::edge_t(iterator<T>::i, const_cast<graph_db<T>&>(iterator<T>::db));
    }
};

template<class T>
class neighbor_iterator : public iterator<T> {
private:
    size_t vertex;
public:
    neighbor_iterator(const graph_db<T> &db, size_t vertex, size_t i)
        : iterator<T>(db, i), vertex(vertex)
    { }
    auto operator*() const {
        size_t at = iterator<T>::db.neighbors.at(vertex).at(iterator<T>::i);
        return typename graph_db<T>::edge_t(at, const_cast<graph_db<T>&>(iterator<T>::db));
    };
    bool operator==(const neighbor_iterator<T> &other) const {
        return iterator<T>::i == other.i && vertex == other.vertex && &(iterator<T>::db) == &other.db;
    }
    bool operator!=(const neighbor_iterator<T> &other) const  {
        return !(*this == other);
    }
};

#endif //GRAPH_DB_ITERATOR_HPP
