## Installation
### Prerequisites
- `dotnet` compiler with support for .NET 5.0
- `g++` compiler
- Apache Thrift library for C++ (.NET version installed as a NuGet package)
- `thrift` compiler able to translate for C++ and .NET Standard

### Compilation
To compile binaries for both client and server, run `make`. 
To compile only the client binary or only the server binary, run `make run-client` or `make run-server` respectively. 
To run only the thrift generator without building binaries (useful for development), run `make thrift`.

## Usage
The client binary is created in `./run-client` and the server binary in `./run-server`.
To find out about options on client usage, see `./run-client --help`.
The server is run as `./run-server [port]`, where port is an optional number setting the port to bind to (default 5000)

To remove all generated files, run `make clean`

## Updates to Interface
The thrift interface was extended in the following ways:
- A new type of item, `ItemD`, was added to the `Item` union
- The `FetchState` enum was extended with the value `ITEMLIST`
- The `FetchResult` struct introduced a new optional field `itemList`
- The `SearchState` struct was extended with an optional bool `itemListSupported`

If the server supports returning multiple items in an `itemList`, it will set the `itemListSupported` value of the initial `SearchState`. If the client also supports it, it will pass that value on during calls to `fetch`. When the server receives a `fetch` call with a set `itemListSupported` value, it can safely set the `FetchState` to `ITEMLIST` and return a list of results in the `itemList` field.

Changes that are related to the interface update are marked in code with comments containing the word "UPDATE".
