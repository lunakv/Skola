using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
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
            var config = new Config();
            ParseArgs(args, config);
            ValidateQuery(config.Query);
			
			if (config.Debug) {
				Console.WriteLine("Config:");
				Log(config);
				Console.WriteLine();
			}

            using TProtocol protocol = MakeProtocol(config);
			Console.WriteLine("Logging in...");
            var loginService = new LoginService(protocol);
            int key = await loginService.LogInAsync(config.Login, 0, token);
            if (config.Debug)
            {
                Console.WriteLine("Login credentials:");
                Console.WriteLine($"Name={config.Login}");
                Console.WriteLine($"Key={key}");
                Console.WriteLine();
            }

			Console.WriteLine("Sending query...");
            var searchService = new SearchService(protocol);
            SearchState initial = await searchService.SearchAsync(config.Query, 10, token);
			Console.WriteLine("Fetching query results...");
            List<Item> results = await searchService.FetchAllAsync(initial, token);
            if (config.Debug)
            {
                Console.WriteLine("Results:");
                Log(results);
                Console.WriteLine();
            }

            Console.WriteLine("Generating report...");
            var reportService = new ReportService(protocol);
            var report = reportService.GenerateReport(results);
            if (config.Debug)
            {
                Console.WriteLine("Report:");
                Log(report);
                Console.WriteLine();
            }
			Console.WriteLine("Sending report...");
            bool reportCorrect = await reportService.SaveReportAsync(report, token);

			Console.WriteLine($"Report correct? {reportCorrect}");
			Console.WriteLine("Logging out...");
            await loginService.LogOutAsync(token);
            Console.WriteLine("Finished.");
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

        private static TProtocol MakeProtocol(Config config)
        {
            var transport = new TSocketTransport(config.Hostname, config.Port, null);
            return new TBinaryProtocol(transport);
        }

        private static void ParseArgs(string[] args, Config config)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--login":
                    case "-l":
                        config.Login = args[++i];
                        break;
                    case "--query":
                    case "-q":
                        config.Query = args[++i];
                        break;
                    case "--host":
                    case "-h":
                        config.Hostname = args[++i];
                        break;
                    case "--port":
                    case "-p":
                        if (int.TryParse(args[++i], out int port))
                        {
                            config.Port = port;
                        }
                        else
                        {
                            throw new ArgumentException($"Port value must be a number. Entered {args[i]}.");
                        }
                        break;
                    case "--debug":
                        config.Debug = true;
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized argument: {args[i]}");
                }
            }
        }

        private static void Log<T>(T value)
        {
            Console.WriteLine(JsonSerializer.Serialize(value,
                new JsonSerializerOptions {WriteIndented = true, IgnoreNullValues = true}));
        }
    }
}
