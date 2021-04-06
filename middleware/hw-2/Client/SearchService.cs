using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace Client
{
    public class SearchService
    {
        private readonly Search.Client _client;

        public SearchService(TProtocol protocol)
        {
            _client = new Search.Client(new TMultiplexedProtocol(protocol, nameof(Search)));
        }
        
        public async Task<SearchState> SearchAsync(string query, int limit, CancellationToken cancellationToken)
        {
            return await _client.searchAsync(query, limit, cancellationToken);
        }

        public async Task<FetchResult> FetchAsync(SearchState state, CancellationToken cancellationToken)
        {
            return await _client.fetchAsync(state, cancellationToken);
        }

        public async Task<List<Item>> FetchAllAsync(SearchState initialState, CancellationToken cancellationToken)
        {
            var ret = new List<Item>();
            FetchResult result = await FetchAsync(initialState, cancellationToken);
            while (result.State != FetchState.ENDED)
            {
                if (result.State == FetchState.ITEMS)
                {
                    ret.Add(result.Item);
                } 
                else if (result.State == FetchState.PENDING)
                {
                    await Task.Delay(100, cancellationToken);
                }

                result = await FetchAsync(result.NextSearchState, cancellationToken);
            }

            return ret;
        }
    }
}