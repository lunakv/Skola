#ifndef LEMMINGS_STENCIL1D_HPP
#define LEMMINGS_STENCIL1D_HPP

#include <thread>
#include <future>
#include <mutex>
#include <condition_variable>
#include <vector>
template<class T> class circle;

// struct synchronizing one pair of workers
struct rendezvous {
    std::mutex m;
    size_t arr_count = 0;       // arrival counter
    size_t leave_count = 0;     // exit counter
    std::condition_variable cv;
};


template<class T, class SF>
class Worker {
    friend class circle<T>;

    rendezvous r_left;          // barrier of this and left neighbor
    bool left_first;            // workers alternate on which way to lock first to avoid dependency chains
    size_t gen;                 // number of generations remaining
    size_t iter;                // number of generations per iteration

    std::array<std::vector<T>, 2> buffer;
    bool current;

    Worker *left, *right;
public:
    explicit Worker(std::vector<T> &&buffer, size_t gen, size_t iter, bool left_first) :
            left_first(left_first), gen(gen), iter(iter), current(false)
    {
        this->buffer[1].resize(buffer.size());
        this->buffer[0] = std::move(buffer);

    }
private:
    /**
     * @brief Synchronize arrival of this and neighbour (before mutual copy)
     * @param r Rendezvous indicating which neighbour I'm synchronizing with
     */
    void arrive(rendezvous &r) {
        std::unique_lock ul(r.m);   // unlocks once we leave the function
        r.arr_count++;

        r.cv.notify_all();
        while (r.arr_count < 2) {
            r.cv.wait(ul);
            if (r.arr_count == 2) {     // the one waking up from sleep is resetting the counter
                r.arr_count = 0;        //   that way, we know that the other is past this sync and won't be affected
                break;                  //   the other couldn't get to this spot again - there's a second sync
            }
        }
    }

    /**
     * @brief Synchronize leave of this and neighbour (assures both copies are done)
     * @param r Rendezvous indicating which neighbour I'm synchronizing with
     * @note Pretty much identical to 'arrive()', but didn't seem worth it to parametrize
     */
    void leave(rendezvous &r) {
        std::unique_lock ul(r.m);
        r.leave_count++;

        r.cv.notify_all();
        while (r.leave_count < 2) {
            r.cv.wait(ul);
            if (r.leave_count == 2) {
                r.leave_count = 0;
                break;
            }
        }
    }

    /**
     * @brief Refresh outmost 'iter' blocks from neighbor
     * @param w The Worker we're copying from
     * @param my_b Index of first block on my end
     * @param o_b Index of first block on their end
     */
    void copy(const Worker &w, size_t my_b, size_t o_b) {
        for (size_t i = 0; i < iter; ++i)
            buffer[current][my_b+i] = w.buffer[current][o_b+i];
    }

    /**
     * @brief Share edge results between this and right neighbour
     */
    void sync_right() {
        arrive(right->r_left);
        copy(*right, buffer[current].size() - iter, iter);
        leave(right->r_left);
    }

    /**
     * @brief Share edge results between this and left neighbour
     */
    void sync_left() {
        arrive(r_left);
        copy(*left, 0, left->buffer[left->current].size() - 2*iter);
        leave(r_left);
    }

    /**
     * @brief Apply one generation of stencil function to the buffer
     * @param sf Stencil function
     * @note Edges slowly fill with garbage values, but those are overwritten on synchronization
     */
    void process_gen(const SF &sf) {
        std::vector<T> &from = buffer[current];
        std::vector<T> &to = buffer[1 - (unsigned)current];
        // first and last index can never be calculated
        for (size_t i = 1; i < from.size() - 1; ++i) {
            to[i] = sf(from[i-1], from[i], from[i+1]);
        }
        current = !current;
    }

    /**
     * @brief Apply multiple generations of stencil function to the buffer
     * @param sf Stencil function
     * @param it Number of generations to run through
     */
    void process(const SF &sf, size_t it) {
        for (size_t i = 0; i < it; ++i) {
            process_gen(sf);
            --gen;
        }
    };
public:
    /**
     * @brief Applies the stencil function 'gen' times to the buffer in synchronized iterations
     * @param sf Stencil function
     */
    void run(const SF &sf) {
        while (gen > 0) {
            size_t it = std::min(gen, iter);
            process(sf, it);
            // If all started the same way, we'd get deadlock through circular waiting chains:
            //  0 -> 1-> 2 -> 3 -> 0 (already sleeping) ==> deadlock
            if (left_first) {
                sync_left();
                sync_right();
            } else {
                sync_right();
                sync_left();
            }
        }
    }
};

/**
 * Dynamically allocated circular container of fixed size
 * @tparam T Contained element
 */
template<class T>
class circle {
    std::vector<T> data;
    size_t n;               // size of 'data' - can't change, so we can save it

    /**
     * @brief Translates indices from circular notation <-n; 2n-1> to regular notation <0; n-1>
     * @param i Circular index
     * @return Normalized value of index usable to access 'data'
     * @note Technically supports all values of ptrdiff_t.
     */
    size_t normalize(ptrdiff_t i) const {
        while (i < 0) i += n;
        return i % n;
    }
public:
    explicit circle(size_t size) : n(size), data(size) {}

    [[nodiscard]] size_t size() const { return n; }

    decltype(auto) get(ptrdiff_t i) const {
        return data[normalize(i)];
    }

    void set(ptrdiff_t i, const T &val) {
        data[normalize(i)] = val;
    }

private:
    /**
     * @brief Fills a vector with 'data' elements in the specified range
     * @param buffer Vector to be filled
     * @param start First index to be copied
     * @param end Index after the last copied
     */
    void fill_buffer(std::vector<T> &buffer, ptrdiff_t start, ptrdiff_t end) {
        ptrdiff_t i = 0;
        // even though always positive, end must be ptrdiff_t, otherwise we break comparison with j
        for (ptrdiff_t j = start; j < end; ++j)
            buffer[i++] = data[normalize(j)];
    }

public:
    /**
     * @brief Runs a stencil function on the circle using multiple threads
     * @param sf Stencil function
     * @param g Total number of generations to run
     * @param thrs Number of background threads to spawn
     */
    template<class SF>
    void run(const SF &sf, size_t g, size_t thrs = std::thread::hardware_concurrency()) {
        std::vector<Worker<T,SF>*> workers; // Worker isn't movable (contains a mutex), so we have to store pointers
        size_t width = size()/thrs;         // width of relevant data
        size_t g_p_it = width/8 + 1;        // generations between syncs (+1 so that we avoid 0 gen. pathological cases)

        // allocate all workers
        for (size_t i = 0; i < thrs; ++i) {
            // buffers must reach 'g_p_it' blocks further than relevant data width on each side
            ptrdiff_t start = i*width - g_p_it;
            // last block may be larger if 'size()' isn't divisible by 'thrs'
            ptrdiff_t end = (i == thrs-1) ? size() + g_p_it : (i+1)*width + g_p_it;
            std::vector<T> buffer(end - start);
            fill_buffer(buffer, start, end);
            workers.push_back(new Worker<T,SF>(std::move(buffer), g, g_p_it, i%2));
        }

        // set neighbors for each worker
        for (size_t i = 0; i < thrs; ++i) {
            workers[i]->right = workers[(i+1)%thrs];
            workers[i]->left = (i == 0) ? workers[thrs-1] : workers[i-1];
        }

        // start all worker threads
        std::vector<std::future<void>> threads;
        for (size_t i = 0; i < thrs; ++i) {
            auto w = workers[i];
            threads.emplace_back(std::async(&Worker<T,SF>::run, w, std::ref(sf)));
        }

        // get results from all workers once they finish
        for (size_t i = 0; i < thrs; ++i) {
            threads[i].wait();
            auto w = workers[i];

            // we don't need to wait for all threads to finish before doing this - we're just reading the data
            size_t begin = i * width;
            for (size_t j = 0; j < w->buffer[0].size() - 2 * g_p_it; ++j) {
                data[begin + j] = w->buffer[(unsigned)w->current][g_p_it + j];
            }
        }

        // deallocate the workers once all results have been retrieved
        for (size_t i = 0; i < thrs; ++i)
            delete workers[i];
    }
};

#endif //LEMMINGS_STENCIL1D_HPP
