#ifndef SEARCH_HANDLER_H
#define SEARCH_HANDLER_H

#include <sstream>

#include "generated/Search.h"
#include "LoginHandler.h"
#include "RandomGenerator.h"

class SearchHandler : public SearchIf {
    std::shared_ptr<LoginHandler> loginHandler;
    std::vector<Item> queryResults;
    RandomGenerator rand;
    int searchIndex; // -1 if no search was performed

    void generateResults(int32_t count, const std::vector<std::string>& queryTypes);
    ItemA AddItemA();
    ItemB AddItemB();
    ItemC AddItemC();
    ItemD AddItemD();

public:
    explicit SearchHandler(std::shared_ptr<LoginHandler> loginHandler):
        loginHandler(std::move(loginHandler)), searchIndex(-1)
    {}

    void search(SearchState& _return, const std::string& query, const int32_t limit) override;
    void fetch(FetchResult& _return, const SearchState& state) override;
    std::vector<Item> getQueryResults() const { return queryResults; }
    bool startedSearch() const { return searchIndex != -1; }
    bool finishedSearch() const { return searchIndex >= queryResults.size(); }
};

#endif