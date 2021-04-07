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
