CLIENT_ROOT=Client
CLIENT_GEN=$(CLIENT_ROOT)/generated

.PHONY: all clean client thrift thriftclient

all: client

thriftclient: Task.thrift
	mkdir -p $(CLIENT_GEN)
	thrift --gen netstd --out $(CLIENT_GEN) -strict Task.thrift

client: thriftclient
	dotnet build -c Release Client
	ln -sf $(CLIENT_ROOT)/bin/Release/net5.0/Client run-client

thrift: thriftclient

clean:
	@echo Removing all generated files...
	-@rm -rf $(CLIENT_GEN) run-client $(CLIENT_ROOT)/bin
