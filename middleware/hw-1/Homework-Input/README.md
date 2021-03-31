Compile the source with `./make`. Then run
```
rmiregistry &
./start-server &
```
to start the server.
The client is run with the following syntax:
`./run-client <n> <m> [ip=<address>] [remote-searcher] [remote-nodes|both-nodes]`

`<n>` and `<m>` is the number of vertices and edges, respectively. 
`ip=<address>` sets the address of the server (defaults to `localhost`)
`remote-searcher` enables an additional benchmark using a server-side searcher
`remote-nodes` specifies a server-side node factory should be used
`both-nodes` will produce two separate benchmarks, one with local node factory
             and one with remote node factory

Benchmark data and other documentation can be found in `documentation.pdf`
The data used in that documentation was generated with `bench.sh`
