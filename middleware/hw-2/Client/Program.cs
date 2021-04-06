using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;
using Thrift.Transport.Client;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using var source = new CancellationTokenSource();
            RunClientAsync(args, source.Token).Wait(source.Token);
        }
        
        private static async Task RunClientAsync(string[] args, CancellationToken token)
        {
            string login = "test";
            string query = "ItemA,ItemB";
            ValidateQuery(query);
            
            using TProtocol protocol = MakeProtocol();
            var loginService = new LoginService(protocol);
            await loginService.LogInAsync(login, 0, token);

            var searchService = new SearchService(protocol);
            SearchState initial = await searchService.SearchAsync(query, 10, token);
            List<Item> results = await searchService.FetchAllAsync(initial, token);

            var reportService = new ReportService(protocol);
            var report = reportService.GenerateReport(results);
            bool reportCorrect = await reportService.SaveReportAsync(report, token);

            await loginService.LogOutAsync(token);
        }
        
        private static void ValidateQuery(string query)
        {
            string[] validTypes = {nameof(ItemA), nameof(ItemB), nameof(ItemC)};
            foreach (string type in query.Split(','))
            {
                if (validTypes.All(x => x != type))
                {
                    throw new ArgumentOutOfRangeException($"Type \"{type}\" is not valid in search query.");
                }
            }
        }

        private static TProtocol MakeProtocol()
        {
            var transport = new TSocketTransport("lab.d3s.mff.cuni.cz", 5001, null);
            return new TBinaryProtocol(transport);
        }
    }
}