### Configuring, building and running
The only configuration required is modifying `setenv.sh` to contain the path to your Hazelcast installation.

To build all components, run `./make.sh`. To clean up all generated files, run `./clean.sh`.

You can start a new member of the cluster by running `./run-member.sh`. To start a client, run `./run-client.sh <name>`.

## Documentation
### Data layout
There are five maps available through the Hazelcast cluster. First is the `Documents` map, which serves as a cache for created documents. Then there's `AccessCounts`, storing the number of times each document was accessed, and `Comments`, storing a `List<>` of comments for each Document. We also have `SelectedDocuments`, listing for each user the name of the document they have currently selected, and `FavoriteLists`, saving for each user a `List<>` of their favorite documents.

### Data updates
Updating the stored data is done through `map.executeOnKey` using an appropriate `EntryProcessor`. This method was chosen as a simple way to ensure all the processing is done on the cluster and to avoid race conditions. Because an `EntryProcessor` called this way locks the key it's accessing, it can modify items without worry of another process accessing the same values.

### Cluster Configuration
The `Documents` map is special in its function. Unlike the other maps, it isn't meant as a reliable data store, but only as a cache for accessed documents. As such, it has additional configuration associated with it.

First, it is configured not to use any backups. For a cache, data persistence is a secondary concern, while speed is paramount, so replicating each entry on multiple partitions isn't desirable in this case. Second, as it could go large quickly, it is configured with an eviction policy that will start to remove items once a node reaches 20% or less of available heap memory.

The other Maps are using the default configuration. As they're meant to be a more general-purpose data storage, having some level of redundancy is beneficial. By the task assignment, we assume that these tables won't grow large enough to require an eviction policy, so we do not specify one.

### Execution offloading
For time-consuming computations, the EntryProcessors are relegated to a separate execution thread, releasing the partition thread to serve other requests. For our purposes, time-consuming computations are document generation and any operation that requires iterating through a List. On the other hand, simple operations, such as incrementing a counter or appending an item to a List are fast enough that relegating them to another thread seemed unnecessary. Those are therefore run directly on the partition threads.

### Processor return types
When the execution of an EntryProcessor is finished, its return value must be transferred from the cluster to the client. Since the client mostly ignores these results, in some cases they were adjusted to minimize the amount of data that has to be sent. In particular, for processors on Maps of Lists, the execution doesn't return an instance of the resulting List as one might expect, but simply `null`. 

### Possible race conditions
While the usage of EntryProcessors and their inherent locking mechanisms eliminates most race conditions that could occur, there are still some not handled. The one most prominent would be in the `n` command, where a user's "favorites" list might change between the time the currently selected document name is fetched and the time the next item is searched for. In such a case, the returned document might not be what the client anticipated. That being said, this is mainly an issue of perception, and not one of data integrity; the command will still find the "next" item in the list as it exists when that search occurs. This was therefore deemed not worthy of change, as any attempt to resolve such an issue would dramatically increase complexity.

We could also mention the inherent inconsistencies that could occur when a collection changes between the time it's fetched from the cluster and the time it's actually displayed to the user, but such scenarios are both extremely rare and in some sense unavoidable, making them not an issue worth spending time on.


