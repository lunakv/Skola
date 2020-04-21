#ifndef GRAPH_DB_HPP
#define GRAPH_DB_HPP

#include <tuple>
#include "iterator.hpp"
#include "proxy.hpp"

/**
 * @brief A graph database that takes its schema (types and number of vertex/edge properties, user id types) from a given trait
 * @tparam GraphSchema A trait which specifies the schema of the graph database.
 * @see graph_schema
 */
template<class GraphSchema>
class graph_db {
    friend class vertex<GraphSchema>;
    friend class edge<GraphSchema>;
    friend class neighbor_iterator<GraphSchema>;    // unlike other iterators, neighbor_it needs access to the neighbors vector
private:
    // vertices/edges are internally identified by a unique numeric index.
    // the following vectors store the values for a vertex/edge at the position of the index identifying this vertex/edge
    std::vector<typename GraphSchema::vertex_user_id_t> v_ids;  // list of vertex IDs
    std::vector<typename GraphSchema::edge_user_id_t> e_ids;    // list of edge IDs
    std::vector<std::pair<size_t, size_t>> edges;               // list of src and dest vertices for each edge
    std::vector<std::vector<size_t>> neighbors;                 // list of lists of outgoing edges for each vertex

    // Helper template that exposes the inner structure of a passed tuple
    // used to a) convert a tuple into a tuple of vectors, for storing properties
    //         b) generate an index sequence for that tuple (see helper functions below)
    template<class> struct unpacker;
    template<class ...Ts> struct unpacker<std::tuple<Ts...>> {
        using prop_t = std::tuple<std::vector<Ts>...>;
        using index = std::index_sequence_for<Ts...>;
    };
    template<class T> using prop_t = typename unpacker<T>::prop_t;
    template<class T> using index = typename unpacker<T>::index;

    // tuple<vector<Properties>...> -- columnar storage for vertex/edge properties
    prop_t<typename GraphSchema::vertex_property_t> v_props;
    prop_t<typename GraphSchema::edge_property_t> e_props;

public:
    /**
     * @brief A type representing a vertex.
     * @see vertex
     */
    using vertex_t = vertex<GraphSchema>;
    /**
     * @brief A type representing an edge.
     * @see edge
     */
    using edge_t = edge<GraphSchema>;

    /**
     * @brief A type representing a vertex iterator. Must be at least of output iterator. Returned value_type is a vertex.
     * @note Iterate in insertion order.
     */
    using vertex_it_t = vert_iterator<GraphSchema>;

    /**
     * @brief A type representing a edge iterator. Must be at least an output iterator. Returned value_type is an edge.
     * @note Iterate in insertion order.
     */
    using edge_it_t = edge_iterator<GraphSchema>;

    /**
     * @brief A type representing a neighbor iterator. Must be at least an output iterator. Returned value_type is an edge.
     * @note Iterate in insertion order.
     */
    using neighbor_it_t = neighbor_iterator<GraphSchema>;

private:
    /**
     * @brief Creates a new vertex in the database (doesn't handle properties)
     * @param vuid User ID of the new vertex
     * @return Internal ID of the created vertex
     */
    size_t make_vertex(typename GraphSchema::vertex_user_id_t &&vuid) {
        size_t i = v_ids.size();
        v_ids.push_back(std::move(vuid));
        neighbors.emplace_back(); // expand neighbor list to include new vertex
        return i;
    }
    size_t make_vertex(const typename GraphSchema::vertex_user_id_t &vuid) {
        size_t i = v_ids.size();
        v_ids.push_back(vuid);
        neighbors.emplace_back();
        return i;
    }

    /**
     * @brief Creates a new edge in the database (doesn't handle properties);
     * @param euid User ID of the new edge
     * @param v1 Source vertex
     * @param v2 Destination vertex
     * @return Internal ID of the created edge
     */
    size_t make_edge(typename GraphSchema::edge_user_id_t &&euid, const vertex_t &v1, const vertex_t &v2) {
        size_t i = e_ids.size();
        e_ids.push_back(std::move(euid));
        edges.emplace_back(v1.position, v2.position);
        neighbors[v1.position].push_back(i);    // always in bounds if v1 is valid (see make_vertex)
        return i;
    }
    size_t make_edge(const typename GraphSchema::edge_user_id_t &euid, const vertex_t &v1, const vertex_t &v2) {
        size_t i = e_ids.size();
        e_ids.push_back(euid);
        edges.emplace_back(v1.position, v2.position);
        neighbors[v1.position].push_back(i);
        return i;
    }

    // Index sequence for vertex/edge properties. Used to easily set/extract properties with fold expressions (see below)
    constexpr static auto v_prop_seq = typename unpacker<typename GraphSchema::vertex_property_t>::index{};
    constexpr static auto e_prop_seq = typename unpacker<typename GraphSchema::edge_property_t>::index{};

    /**
     * @brief Appends passed properties to the specified list in the database
     * @param props List of properties to add the new values to (in tuple<vector<...>> form)
     * @param add Tuple of properties to add
     * @tparam Ints Index sequence for props/add
     * @note If props and add aren't compatible, the fold expression fails to compile
     */
    template <class T, class ...P, size_t ...Ints>
    void add_props(T &props, std::tuple<P...> &&add, std::index_sequence<Ints...>) {
        (std::get<Ints>(props).push_back(std::move(std::get<Ints>(add))), ...);
    }

    /**
     * @brief Appends the default values for all properties to the specified list
     * @note Used when no properties are specified
     */
    template <class T, size_t ...Ints>
    void add_props(T &props, std::index_sequence<Ints...>) {
        (std::get<Ints>(props).emplace_back(), ...);
    }

    /**
     * @brief Sets the properties at a given index according to the specified tuple
     */
    template <class T, class ...P, size_t ...Ints>
    void set_props(size_t pos, T &props, std::tuple<P...> &&add, std::index_sequence<Ints...>) {
        ((std::get<Ints>(props)[pos] = std::move(std::get<Ints>(add))), ...);
    }

    /**
     * @brief Gets a slice of the properties at a given index
     * @return A tuple of references to the specified properties
     */
    template <class T, size_t ...Ints>
    auto get_props(size_t pos, T &props, std::index_sequence<Ints...>) const {
        return std::make_tuple(get_r(std::get<Ints>(props), pos)...);
    }

    // Dirty hack to get around the vector<bool> specialization when creating tuple of properties
    // Has the consequence of not returning bools by reference, but the properties should be immutable anyway
    template <class T>
    T & get_r(std::vector<T> &v, size_t pos) const {
        return v[pos];
    }
    bool get_r(std::vector<bool> &v, size_t pos) const {
        return (bool) v[pos];
    }

public:
    /**
     * @brief Insert a vertex into the database.
     * @param vuid A user id of the newly created vertex.
     * @return The newly created vertex.
     * @note The vertex's properties have default values.
     */
    vertex_t add_vertex(typename GraphSchema::vertex_user_id_t &&vuid) {
        size_t i = make_vertex(std::move(vuid));
        add_props(v_props, v_prop_seq);
        return vertex_t(i, *this);
    }
    vertex_t add_vertex(const typename GraphSchema::vertex_user_id_t &vuid) {
        size_t i = make_vertex(vuid);
        add_props(v_props, v_prop_seq);
        return vertex_t(i, *this);
    }

    /**
     * @brief Insert a vertex into the database with given values of the vertex's properties.
     * @tparam Props All types of properties.
     * @param vuid A user id of the newly created vertex.
     * @param props Properties of the new vertex.
     * @return The newly created vertex.
     * @note Should not compile if not provided with all properties.
     */
    template<typename ...Props>
    vertex_t add_vertex(typename GraphSchema::vertex_user_id_t &&vuid, Props &&...props) {
        size_t i = make_vertex(std::move(vuid));
        add_props(v_props, std::forward_as_tuple(props...), v_prop_seq);
        return vertex_t(i, *this);
    }
    template<typename ...Props>
    vertex_t add_vertex(const typename GraphSchema::vertex_user_id_t &vuid, Props &&...props) {
        size_t i = make_vertex(vuid);
        add_props(v_props, std::forward_as_tuple(props...), v_prop_seq);
        return vertex_t(i, *this);
    }

    /**
     * @brief Returns begin() and end() iterators to all vertexes in the database.
     * @return A pair<begin(), end()> of vertex iterators.
     * @note The iterator can iterate in any order.
     */
    std::pair<vertex_it_t, vertex_it_t> get_vertexes() const {
        return std::make_pair(vertex_it_t(*this, 0), vertex_it_t(*this, v_ids.size()));
    }

    /**
     * @brief Insert a directed edge between v1 and v2 with a given user id.
     * @param euid An user id of the edge.
     * @param v1 A source vertex of the edge.
     * @param v2 A destination vertex of the edge.
     * @return The newly create edge.
     * @note The edge's properties have default values.
     */
    edge_t add_edge(typename GraphSchema::edge_user_id_t &&euid, const vertex_t &v1, const vertex_t &v2) {
        size_t i = make_edge(std::move(euid), v1, v2);
        add_props(e_props, e_prop_seq);
        return edge_t(i, *this);
    };
    edge_t add_edge(const typename GraphSchema::edge_user_id_t &euid, const vertex_t &v1, const vertex_t &v2) {
        size_t i = make_edge(euid, v1, v2);
        add_props(e_props, e_prop_seq);
        return edge_t(i, *this);
    }

    /**
     * @brief Insert a directed edge between v1 and v2 with a given user id and given properties.
     * @tparam Props Types of properties
     * @param euid An user id of the edge.
     * @param v1 A source vertex of the edge.
     * @param v2 A destination vertex of the edge.
     * @param props All properties of the edge.
     * @return The newly create edge.
     * @note Should not compile if not provided with all properties.
     */
    template<typename ...Props>
    edge_t add_edge(typename GraphSchema::edge_user_id_t &&euid, const vertex_t &v1, const vertex_t &v2, Props &&...props) {
        size_t i = make_edge(std::move(euid), v1, v2);
        add_props(e_props, std::forward_as_tuple(props...), e_prop_seq);
        return edge_t(i, *this);
    }
    template<typename ...Props>
    edge_t add_edge(const typename GraphSchema::edge_user_id_t &euid, const vertex_t &v1, const vertex_t &v2, Props &&...props) {
        size_t i = make_edge(euid, v1, v2);
        add_props(e_props, std::forward_as_tuple(props...), e_prop_seq);
        return edge_t(i, *this);
    }

    /**
     * @brief Returns begin() and end() iterators to all edges in the database.
     * @return A pair<begin(), end()> of edge iterators.
     * @note The iterator can iterate in any order.
     */
    std::pair<edge_it_t, edge_it_t> get_edges() const {
        return std::make_pair(edge_it_t(*this, 0), edge_it_t(*this, e_ids.size()));
    }
};

#endif //GRAPH_DB_HPP
