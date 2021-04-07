#ifndef SEARCH_HANDLER_H
#define SEARCH_HANDLER_H

#include <sstream>

#include "generated/Search.h"
#include "LoginHandler.h"
#include "RandomGenerator.h"

class SearchHandler : public SearchIf {
    std::shared_ptr<LoginHandler> loginHandler;
    std::vector<Item> queryResults;
    std::vector<std::string> queryTypes;
    RandomGenerator rand;
    bool searchInProgress;

    void generateResults(int32_t count);
    ItemA AddItemA();
    ItemB AddItemB();
    ItemC AddItemC();

public:
    explicit SearchHandler(std::shared_ptr<LoginHandler> loginHandler):
        loginHandler(std::move(loginHandler))
    {}

    void search(SearchState& _return, const std::string& query, const int32_t limit) override;
    void fetch(FetchResult& _return, const SearchState& state) override;
    std::vector<Item> getQueryResults() const { return queryResults; }
};

#endif