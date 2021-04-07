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
            if (args.Contains("--help") || args.Length == 0) {
                PrintUsage();
                return;
            }

            try
            {
                using var source = new CancellationTokenSource();
                RunClientAsync(args, source.Token).Wait(source.Token);
            }
            catch (AggregateException e)
            {
                Console.WriteLine("ERROR:");
                foreach (Exception inner in e.InnerExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(e.Message);
            }
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
            int key = await loginService.LogInAsync(config.Login, config.Key, token);
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
            if (query is null)
            {
                throw new ArgumentException("Query not specified.");
            }
            
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
                    case "--key":
                    case "-k":
                        if (int.TryParse(args[++i], out int key))
                        {
                            config.Key = key;
                        }
                        else
                        {
                            throw new ArgumentException($"Key must be a number. Entered {args[i]}.");
                        }
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
                    case "-d":
                        config.Debug = true;
                        break;
                    case {} when i == args.Length - 1:
                        config.Query = args[i];
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized option: {args[i]}");
                }
            }
        }

        private static void Log<T>(T value)
        {
            Console.WriteLine(JsonSerializer.Serialize(value,
                new JsonSerializerOptions {WriteIndented = true, IgnoreNullValues = true}));
        }

        private static void PrintUsage() {
            Console.WriteLine(
@"Usage: run-client [options] {query}
QUERY SPECIFICATION:
  {query} is a comma-separated list of item types.
  Types in query can repeat.
  Supported item types:
    ItemA
    ItemB
    ItemC

OPTIONS:
  -l, --login <name>     specify login name (default: lunakv-mw2-client)
  -k, --key <pwd>        specify login key (default: 0)
  -h, --host <hostname>  hostname to connect to (deafult: localhost)
  -p, --port <port>      port to connect to (default: 5000)
  -d, --debug            print additional debugging info
  --help                 display this help and exit");
        }
    }
}
