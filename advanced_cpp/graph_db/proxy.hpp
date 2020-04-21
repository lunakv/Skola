#ifndef GRAPH_DB_PROXY_HPP
#define GRAPH_DB_PROXY_HPP

#include "graph_db.hpp"
template<class T> class graph_db; // forward declaration to satisfy cyclic dependency

// vertex and edge are simple proxy objects that just remember the position of the referenced object in the db
// the function calls just defer to the database itself
template<class T>
class vertex {
    size_t position;
    graph_db<T> &db;
    friend class graph_db<T>;
public:
    explicit vertex(size_t pos, graph_db<T> &db)
        : position(pos), db(db)
    { }

    /**
     * @brief Returns the immutable user id of the element.
     */
    auto id() const {
        return db.v_ids.at(position);
    }

    /**
     * @brief Returns all immutable properties of the element in tuple.
     * @note The return type is GraphSchema::vertex_property_t for vertexes and GraphSchema::edge_property_t for edges.
     */
    auto get_properties() const {
        return db.get_props(position, db.v_props, db.v_prop_seq);
    }

    /**
     *
     * @brief Returns a single immutable property of the I-th element.
     * @tparam I An index of the property.
     * @return The value of the property.
     * @note The first property is on index 0.
     */
    template<size_t I>
    decltype(auto) get_property() const {
        return std::get<I>(db.v_props).at(position);
    }

    /**
     * @brief Sets the values of properties of the element.
     * @tparam PropsType Types of the properties.
     * @param props The value of each individual property.
     * @note Should not compile if not provided with all properties.
     */
    template<typename ...PropsType>
    void set_properties(PropsType &&...props) {
        db.set_props(position, db.v_props, std::forward_as_tuple(props...), db.v_prop_seq);
    }

    /**
     * @brief Set a value of the given property of the I-th element
     * @tparam I An index of the property.
     * @tparam PropType The type of the property.
     * @param prop The new value of the property.
     * @note The first property is on index 0.
     */
    template<size_t I, typename PropType>
    void set_property(const PropType &prop) {
        std::get<I>(db.v_props)[position] = prop;
    }

    /**
     * @see graph_db::neighbor_it_t
     */
    using neighbor_it_t = typename graph_db<T>::neighbor_it_t;

    /**
     * @brief Returns begin() and end() iterators to all forward edges from the vertex
     * @return A pair<begin(), end()> of a neighbor iterators.
     * @see graph_db::neighbor_it_t
     */
    std::pair<neighbor_it_t, neighbor_it_t> edges() const {
        return std::make_pair(neighbor_it_t(db, position, 0), neighbor_it_t(db, position, db.neighbors.at(position).size()));
    }
};

template<class T>
class edge {
private:
    size_t position;
    graph_db<T> &db;
public:
    edge(size_t position, graph_db<T> &db) : position(position), db(db)
    { }

    /**
     * @brief Returns the immutable user id of the element.
     */
    auto id() const {
        return db.e_ids.at(position);
    };

    /**
     * @brief Returns all immutable properties of the element in tuple.
     * @note The return type is GraphSchema::vertex_property_t for vertexes and GraphSchema::edge_property_t for edges.
     */
    auto get_properties() const {
        return db.get_props(position, db.e_props, db.e_prop_seq);
    };

    /**
     *
     * @brief Returns a single immutable property of the I-th element.
     * @tparam I An index of the property.
     * @return The value of the property.
     * @note The first property is on index 0.
     */
    template<size_t I>
    decltype(auto) get_property() const {
        return std::get<I>(db.e_props).at(position);
    }

    /**
     * @brief Sets the values of properties of the element.
     * @tparam PropsType Types of the properties.
     * @param props The value of each individual property.
     * @note Should not compile if not provided with all properties.
     */
    template<typename ...PropsType>
    void set_properties(PropsType &&...props) {
        db.set_props(position, db.e_props, std::forward_as_tuple(props...), db.e_prop_seq);
    }

    /**
     * @brief Set a value of the given property of the I-th element
     * @tparam I An index of the property.
     * @tparam PropType The type of the property.
     * @param prop The new value of the property.
     * @note The first property is on index 0.
     */
    template<size_t I, typename PropType>
    void set_property(const PropType &prop) {
        std::get<I>(db.e_props)[position] = prop;
    }

    /**
     * @brief Returns the source vertex of the edge.
     * @return The vertex.
     */
    auto src() const {
        size_t p = db.edges.at(position).first;
        return vertex(p, db);
    }

    /**
     * @brief Returns the destination vertex of the edge.
     * @return The vertex.
     */
    auto dst() const {
        size_t p = db.edges.at(position).second;
        return vertex(p, db);
    }
};
#endif //GRAPH_DB_PROXY_HPP
