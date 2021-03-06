// Interface for a client-server application
// Do not modify, except when required by comments

// Thrown when an invalid key is used for login
exception InvalidKeyException{
    // The invalid key that was used
    1: i32 invalidKey
    // The expected key that will succeed
    2: i32 expectedKey
}

// Thrown when the client calls remote methods with wrong arguments or in wrong order
exception ProtocolException{
    1: string message
}
// Service handling user identification
service Login{
    // Log
    // throws InvalidKeyError if the key is not correct
    // throws ProtocolException if already logged in
    void logIn(1: string userName, 2: i32 key) throws (1: InvalidKeyException invalidKeyException, 2: ProtocolException protocolException)

    // throws ProtocolException if already logged in
    oneway void logOut();
}

// 3 types of items that the application works with
// UPDATE: 4* types of items
struct ItemA{
  1: required i16 fieldA
  2: required list<i16> fieldB
  3: required i32 fieldC
}

struct ItemB{
  1: string fieldA
  2: set<string> fieldB
  3: optional list<string> fieldC
}

struct ItemC{
  1: bool fieldA
}

// UPDATE: Added 4th item type
struct ItemD{
  1: i32 fieldA
  2: optional list<bool> fieldB
  3: string fieldC
}
  

// A type which can contain any of the item types
union Item{
  1: ItemA itemA
  2: ItemB itemB
  3: ItemC itemC
  4: ItemD itemD
}

// State of fetching results of a search
enum FetchState{
    // The search is still running and may return more items later
    // Call fetchResult again after waiting for a while
    PENDING = 1
    // The item field of FetchResult contains an item
    ITEMS = 2
    // All items were fetched
    ENDED = 3
	// UPDATE: multiple items at once possible
	ITEMLIST = 4
}

// Represents a state of a search
struct SearchState{
    // Estimated number of items in the search result
    1: i32 countEstimate
    // Number of items fetched so far
    2: i32 fetchedItems
	// UPDATE: notify of ITEMLIST capability
	3: optional bool itemListSupported
}

// Result of a call to fetch
struct FetchResult{
    // Tells whether an item is fetched, the search is still running or all items were fetched
    1: FetchState state
    // If state is ITEMS, contains an item
    2: optional Item item
    // SearchState that should be passed to next call of fetch
    3: SearchState nextSearchState
	// UPDATE: If state is ITEMLIST, contains multiple items sent at once
	4: optional list<Item> itemList
}

// Type of a report
typedef map<string, set<string>> Report

// Service handling item search
service Search{
    // Searches for items of a specified type.
    // The result of a search is a list of items.
    // Those items are not returned immediately, but can be fetched using fetchResult
    // query is a comma-separated string specifies which kinds of items may be present in the result
    // for example "ItemA,ItemB"
    // limit is maximum number of items that can be searched for
    // Throws ProtocolException if not logged in, or if query or limit is invalid
    SearchState search(1: string query, 2: i32 limit = 100) throws (1: ProtocolException protocolException)

    // Fetches a part of the search result.
    // state is the result returned from search, or from the previous call to fetch
    // Throws ProtocolException if no search was performed, or if the index does not match the number 
    FetchResult fetch(1: required SearchState state) throws (1: ProtocolException protocolException);
}

// Service for receiving a report
service Reports{
    // Sends the report to the server
    // throws ProtocolException if not logged in, or no search was performed
    // returns true if the contents of the report match the last search
    // returns false otherwise
    bool saveReport(1: Report report) throws (1: ProtocolException protocolException);
}
