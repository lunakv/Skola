#ifndef LEMMINGS_WORKER_HPP
#define LEMMINGS_WORKER_HPP

#include <mutex>
#include <condition_variable>
#include <vector>
#include <iostream>
template<class T> class circle;

struct rendezvous {
    std::mutex m;
    size_t arr_count = 0;
    std::condition_variable cv;
    size_t leave_count = 0;
};


template<class T, class SF>
class Worker {
    friend class circle<T>;

    rendezvous r_left;
    bool left_first;
    size_t gen;                 // number of generations remaining
    size_t iter;                // number of generations per iteration
    out *o;
    size_t name;
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
    void setO(out *ou) { this->o = ou; }
private:
    void say(std::string_view sv) {
        std::lock_guard lg(o->m);
        o->str << name << ": " << sv << std::endl;
    }

    void arrive(rendezvous &r) {
        std::unique_lock ul(r.m);
        r.leave_count = 0;
        r.arr_count++;
        say("Notifying all threads");
        r.cv.notify_all();
        while (r.arr_count < 2)
            r.cv.wait(ul);
        say("Woke up");
    }

    void leave(rendezvous &r) {
        std::unique_lock ul(r.m);
        r.arr_count = 0;
        r.leave_count++;

        r.cv.notify_all();
        while (r.leave_count < 2)
            r.cv.wait(ul);
    }

    void copy(const Worker &w, size_t my_b, size_t o_b) {
        for (size_t i = 0; i < iter; ++i)
            buffer[current][my_b+i] = w.buffer[current][o_b+i];
    }

    void sync_right() {
        say("Syncing right...");
        arrive(right->r_left);
        copy(*right, buffer[current].size() - iter, iter);
        leave(right->r_left);
        say("Synced right");
    }

    void sync_left() {
        say("Syncing left...");
        arrive(r_left);
        copy(*left, 0, left->buffer[left->current].size() - 2*iter);
        leave(r_left);
        say("Synced left");
    }

    void process_gen(const SF &sf, size_t offset) {
        std::vector<T> &from = buffer[current];
        std::vector<T> &to = buffer[1 - (unsigned)current];
        for (size_t i = offset; i < from.size() - offset; ++i) {
            to[i] = sf(from[i-1], from[i], from[i+1]);
        }
        current = !current;
    }

    void process(const SF &sf, size_t it) {
        for (size_t i = 0; i < it; ++i) {
            process_gen(sf, it+1);
            --gen;
        }
    };
public:
    void run(const SF &sf) {
        say("Starting processing...");
        while (gen > 0) {
            size_t it = std::min(gen, iter);
            process(sf, it);
            if (left_first) {
                sync_left();
                sync_right();
            } else {
                sync_right();
                sync_left();
            }
        }
        say("Done processing");
    }
};

#endif //LEMMINGS_WORKER_HPP
