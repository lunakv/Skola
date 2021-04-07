using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace Client
{
    public class SearchService
    {
        private readonly Search.Client _client;
        private readonly Config _config;

        public SearchService(TProtocol protocol, Config config)
        {
            _client = new Search.Client(new TMultiplexedProtocol(protocol, nameof(Search)));
            _config = config;
        }

        public async Task<SearchState> SearchAsync(string query, int limit, CancellationToken cancellationToken)
        {
            if (_config.Debug)
                Console.WriteLine("Running Search...");
            return await _client.searchAsync(query, limit, cancellationToken);
        }

        public async Task<FetchResult> FetchAsync(SearchState state, CancellationToken cancellationToken)
        {
            if (_config.Debug)
                Console.WriteLine("Running Fetch...");
            FetchResult result = await _client.fetchAsync(state, cancellationToken);
            if (_config.Debug)
                Console.WriteLine($"Fetch completed with state {result.State}.");
            return result;
        }

        public async Task<List<Item>> FetchAllAsync(SearchState initialState, CancellationToken cancellationToken)
        {
            var ret = new List<Item>();
            // UPDATE: if server sends 'itemListSupported', we pass it in the SearchState
            FetchResult result = await FetchAsync(initialState, cancellationToken);
            while (result.State != FetchState.ENDED)
            {
                switch (result.State)
                {
                    case FetchState.ITEMS:
                        ret.Add(result.Item);
                        break;
                    case FetchState.PENDING:
                        await Task.Delay(100, cancellationToken);
                        break;
                    // UPDATE: handle ITEMLIST
                    case FetchState.ITEMLIST:
                        ret.AddRange(result.ItemList);
                        break;
                    case FetchState.ENDED:
                        break;
                    default:
                        Console.WriteLine($"Unsupported FetchState value encountered: {result.State.ToString()}");
                        Console.WriteLine("Ending fetch cycle early.");
                        return ret;
                }

                result = await FetchAsync(result.NextSearchState, cancellationToken);
            }

            return ret;
        }
    }
}