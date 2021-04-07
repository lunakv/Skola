// Standard library headers
#include <memory>
#include <iostream>
#include <string>
#include <sstream>
#include <mutex>

// Thrift headers
#include <thrift/protocol/TProtocol.h>
#include <thrift/protocol/TBinaryProtocol.h>
#include <thrift/protocol/TMultiplexedProtocol.h>
#include <thrift/transport/TSocket.h>
#include <thrift/transport/TTransportUtils.h>
#include <thrift/server/TServer.h>
#include <thrift/server/TThreadedServer.h>
#include <thrift/processor/TMultiplexedProcessor.h>
#include <thrift/TProcessor.h>
#include <thrift/Thrift.h>
#include <random>
#include <utility>

#include "LoginHandler.h"
#include "SearchHandler.h"
#include "ReportsHandler.h"

using namespace apache::thrift;
using namespace apache::thrift::transport;
using namespace apache::thrift::protocol;
using namespace apache::thrift::server;
using namespace std;

// This factory creates a new handler for each conection
class PerConnectionExampleProcessorFactory: public TProcessorFactory{
    // We assign each handler an id
    unsigned connectionIdCounter;
    mutex lock;

public:
    PerConnectionExampleProcessorFactory(): connectionIdCounter(0) {}

    // The counter is incremented for each connection
    unsigned assignId() {
        lock_guard<mutex> counterGuard(lock);
        return ++connectionIdCounter;
    }

    // This metod is called for each connection
    std::shared_ptr<TProcessor> getProcessor(const TConnectionInfo& connInfo) override {
        unsigned connectionId = assignId();
        shared_ptr<LoginHandler> loginHandler(new LoginHandler(connectionId));
        shared_ptr<TProcessor> loginProcessor(new LoginProcessor(loginHandler));
        shared_ptr<SearchHandler> searchHandler(new SearchHandler(loginHandler));
        shared_ptr<TProcessor> searchProcessor(new SearchProcessor(searchHandler));
        shared_ptr<ReportsHandler> reportsHandler(new ReportsHandler(loginHandler, searchHandler));
        shared_ptr<TProcessor> reportsProcessor(new ReportsProcessor(reportsHandler));

        shared_ptr<TMultiplexedProcessor> muxProcessor(new TMultiplexedProcessor());
        muxProcessor->registerProcessor("Login", loginProcessor);
        muxProcessor->registerProcessor("Search", searchProcessor);
        muxProcessor->registerProcessor("Reports", reportsProcessor);
        return muxProcessor;
    }
};

int main(int argc, char *argv[]){
	int port = 5000;
	if (argv[1]) {
		port = stoi(argv[1]);
	}

    try{
        // Accept connections on a TCP socket
        shared_ptr<TServerTransport> serverTransport(new TServerSocket(port));
        // Use buffering
        shared_ptr<TTransportFactory> transportFactory(new TBufferedTransportFactory());
        // Use a binary protocol to serialize data
        shared_ptr<TProtocolFactory> protocolFactory(new TBinaryProtocolFactory());
        // Use a processor factory to create a processor per connection
        shared_ptr<TProcessorFactory> processorFactory(new PerConnectionExampleProcessorFactory());

        // Start the server
        TThreadedServer server(processorFactory, serverTransport, transportFactory, protocolFactory);
        server.serve();
    }
    catch (TException& tx) {
        cout << "ERROR: " << tx.what() << endl;
    }
}
