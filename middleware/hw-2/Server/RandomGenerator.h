#ifndef RANDOM_GENERATOR_H
#define RANDOM_GENERATOR_H

#include <random>

class RandomGenerator {
    std::default_random_engine rand;

public:
    RandomGenerator() : rand(static_cast<size_t>(time(nullptr))) {}
    
    template<class T>
    T getRandom(T min, T max) {
        std::uniform_int_distribution<T> dist(min, max);
        return dist(rand);
    }

    template<class T>
    T getRandom() {
        std::uniform_int_distribution<T> dist;
        return dist(rand);
    }
};

#endif