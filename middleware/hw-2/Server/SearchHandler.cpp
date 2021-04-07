#include "SearchHandler.h"

using namespace std;

// helper function
vector<string> splitQuery(const string& query) {
    vector<string> ret;
    istringstream iss(query);
    string item;
    while (getline(iss, item, ',')) {
        if (item == "ItemA" || item == "ItemB" || item == "ItemC") {
            ret.push_back(item);
        } else {
            auto ex = ProtocolException();
            ex.message = "Invalid type in query: " + item;
            throw ex;
        }
    }
    return ret;
}

void SearchHandler::generateResults(int32_t count) {
    for (size_t i = 0; i < count; ++i) {
        std::string &type = queryTypes.at(i % queryTypes.size());
        Item item = Item();
        if (type == "ItemA") {
            item.__set_itemA(AddItemA());
        } else if (type == "ItemB") {
            item.__set_itemB(AddItemB());
        } else if (type == "ItemC") {
            item.__set_itemC(AddItemC());
        }
        queryResults.emplace_back(item);
    }
}

ItemA SearchHandler::AddItemA() {
    ItemA itemA;
    itemA.__set_fieldA(rand.getRandom<int16_t>());
    std::vector<int16_t> fieldB;
    int16_t bSize = rand.getRandom(0, 20);
    for (size_t i = 0; i < bSize; ++i) {
        itemA.fieldB.push_back(rand.getRandom<int16_t>());
    }
    itemA.__set_fieldB(fieldB);
    itemA.__set_fieldC(rand.getRandom<int32_t>());
    return itemA;
}

ItemB SearchHandler::AddItemB() {
    ItemB itemB;
    itemB.__set_fieldA("itemBfieldAstring");
    std::set<std::string> fieldB;
    fieldB.insert("itemBfieldBfirstString");
    fieldB.insert("itemBfieldBsecondString");
    itemB.__set_fieldB(fieldB);
    if (rand.getRandom(0, 1)) {
        std::vector<std::string> fieldC;
        itemB.fieldC.emplace_back("itemBfieldCstring");
        itemB.fieldC.emplace_back("itemBfieldCstring2");
        itemB.fieldC.emplace_back("itemBfieldCstring3");
        itemB.__set_fieldC(fieldC);
    }
    return itemB;
}

ItemC SearchHandler::AddItemC() {
    ItemC itemC;
    itemC.__set_fieldA(static_cast<bool>(rand.getRandom(0, 1)));
    return itemC;
}

void SearchHandler::search(SearchState& _return, const std::string& query, const int32_t limit) {
    loginHandler->loginGuard();
    queryTypes = splitQuery(query);
    int32_t count = std::min(limit, 20);
    generateResults(count);

    _return.__set_countEstimate(count);
    _return.__set_fetchedItems(0);
    searchInProgress = true;
}

void SearchHandler::fetch(FetchResult& _return, const SearchState& state) {
    loginHandler->loginGuard();
    if (!searchInProgress) {
        ProtocolException ex;
        ex.__set_message("Cannot call fetch() before search().");
        throw ex;
    }

    // simulate long computation
    if (!rand.getRandom(0, 30)) {
        _return.__set_state(FetchState::PENDING);
        _return.__set_nextSearchState(state);
        return;
    }

    if (state.fetchedItems >= queryResults.size()) {
        _return.__set_state(FetchState::ENDED);
        _return.__set_nextSearchState(state);
        searchInProgress = false;
        return;
    }

    int32_t i = state.fetchedItems;
    _return.__set_state(FetchState::ITEMS);
    _return.__set_item(queryResults[i]);
    SearchState newState(state);
    newState.__set_fetchedItems(i+1);
    _return.__set_nextSearchState(newState);
}