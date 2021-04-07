CLIENT_ROOT=Client
CLIENT_GEN=$(CLIENT_ROOT)/generated
SERVER_ROOT=Server
SERVER_GEN=$(SERVER_ROOT)/generated

all: run-client run-server
.PHONY: all

thriftclient: Task.thrift
	mkdir -p $(CLIENT_GEN)
	thrift --gen netstd --out $(CLIENT_GEN) -strict Task.thrift
.PHONY: thriftclient

thriftserver: Task.thrift
	mkdir -p $(SERVER_GEN)
	thrift --gen cpp --out $(SERVER_GEN) -strict Task.thrift
	rm $(SERVER_GEN)/*.skeleton.cpp
.PHONY: thriftserver

thrift: thriftclient thriftserver
.PHONY: thrift

run-client: thriftclient
	dotnet build -c Release Client
	ln -sf $(CLIENT_ROOT)/bin/Release/net5.0/Client run-client

run-server: thriftserver
	g++ -o run-server -I $(SERVER_GEN) $(SERVER_ROOT)/*.cpp $(SERVER_GEN)/*.cpp -lthrift

clean:
	@echo Removing all generated files...
	-@rm -rf $(CLIENT_GEN) run-client $(CLIENT_ROOT)/bin $(SERVER_GEN) run-server
.PHONY: clean
