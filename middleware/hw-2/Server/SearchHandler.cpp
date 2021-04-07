#include "SearchHandler.h"

using namespace std;

// helper function
vector<string> splitQuery(const string& query) {
    vector<string> ret;
    istringstream iss(query);
    string item;
    while (getline(iss, item, ',')) {
        // UPDATE: allow ItemD as a query type
        if (item == "ItemA" || item == "ItemB" || item == "ItemC" || item == "ItemD") {
            ret.push_back(item);
        } else {
            ProtocolException ex;
            ex.__set_message("Unsupported query item: " + item);
            throw ex;
        }
    }
    return ret;
}

void SearchHandler::generateResults(int32_t count, const vector<string>& queryTypes) {
    for (size_t i = 0; i < count; ++i) {
        const std::string &type = queryTypes.at(i % queryTypes.size());
        Item item = Item();
        if (type == "ItemA") {
            item.__set_itemA(AddItemA());
        } else if (type == "ItemB") {
            item.__set_itemB(AddItemB());
        } else if (type == "ItemC") {
            item.__set_itemC(AddItemC());
        } else if (type == "ItemD") {
            // UPDATE: add ItemD result generation
            item.__set_itemD(AddItemD());
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
    itemB.__set_fieldA("itemBfieldAstring_"+ std::to_string(rand.getRandom<short>()));
    std::set<std::string> fieldB;
    size_t count = rand.getRandom(1, 10);
    for (size_t i = 0; i < count; ++i)
        fieldB.insert("itemBfieldBstring_"+std::to_string(rand.getRandom<short>()));
    itemB.__set_fieldB(fieldB);
    if (rand.getRandom(0, 1)) {
        std::vector<std::string> fieldC;
        count = rand.getRandom(1, 10);
        for (size_t i = 0; i < count; ++i)
            itemB.fieldC.emplace_back("itemBfieldCstring_"+std::to_string(rand.getRandom<short>()));
        itemB.__set_fieldC(fieldC);
    }
    return itemB;
}

ItemC SearchHandler::AddItemC() {
    ItemC itemC;
    itemC.__set_fieldA(static_cast<bool>(rand.getRandom(0, 1)));
    return itemC;
}

ItemD SearchHandler::AddItemD() {
    ItemD itemD;
    itemD.__set_fieldA(rand.getRandom<int32_t>());
    if (rand.getRandom(0, 2)) {
        vector<bool> fieldB;
        size_t count = rand.getRandom(1, 10);
        for (size_t i = 0; i < count; ++i)
            fieldB.emplace_back(static_cast<bool>(rand.getRandom(0,1)));
        itemD.__set_fieldB(fieldB);
    }
    itemD.__set_fieldC("itemDFieldC_" + to_string(rand.getRandom<short>()));
    return itemD;
}

void SearchHandler::search(SearchState& _return, const std::string& query, const int32_t limit) {
    loginHandler->loginGuard();
    if (limit <= 0) {
        ProtocolException ex;
        ex.__set_message("Limit must be positive.");
        throw ex;
    }
    vector<string> queryTypes = splitQuery(query);
    int32_t count = std::min(limit, 50);
    generateResults(count, queryTypes);

    _return.__set_countEstimate(count);
    _return.__set_fetchedItems(0);
    // UPDATE: notify client about support for ITEMLIST
    _return.__set_itemListSupported(true);
    searchIndex = 0;
}

void SearchHandler::fetch(FetchResult& _return, const SearchState& state) {
    loginHandler->loginGuard();
    if (searchIndex == -1) {
        ProtocolException ex;
        ex.__set_message("Cannot call fetch() before search().");
        throw ex;
    }
    if (searchIndex != state.fetchedItems) {
        ProtocolException ex;
        ex.__set_message("Sent state is different from expected value.");:
        throw ex;
    }

    // simulate long computation
    if (!rand.getRandom(0, 10)) {
        _return.__set_state(FetchState::PENDING);
        _return.__set_nextSearchState(state);
        return;
    }

    // end fetch cycle if all items were returned
    if (state.fetchedItems >= queryResults.size()) {
        _return.__set_state(FetchState::ENDED);
        _return.__set_nextSearchState(state);
        return;
    }


    int32_t i = state.fetchedItems;
    // UPDATE: sometimes send ITEMLIST if supported by client
    if (state.__isset.itemListSupported && state.itemListSupported && !rand.getRandom(0, 5)) {
        vector<Item> itemList;
        size_t end = i + rand.getRandom<size_t>(1, queryResults.size() - i);
        for (; i < end; ++i)
            itemList.push_back(queryResults[i]);
        _return.__set_itemList(itemList);
        _return.__set_state(FetchState::ITEMLIST);
    } else {
        _return.__set_item(queryResults[i++]);
        _return.__set_state(FetchState::ITEMS);
    }

    SearchState newState(state);
    newState.__set_fetchedItems(i);
    _return.__set_nextSearchState(newState);
    searchIndex = i;
}