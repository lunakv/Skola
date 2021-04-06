using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Collections;
using Thrift.Protocol;

namespace Client
{
    public class ReportService
    {
        private readonly Reports.Client _client;

        public ReportService(TProtocol protocol)
        {
            _client = new Reports.Client(new TMultiplexedProtocol(protocol, nameof(Reports)));
        }

        public async Task<bool> SaveReportAsync(Dictionary<string, THashSet<string>> report,
            CancellationToken cancellationToken)
        {
            return await _client.saveReportAsync(report, cancellationToken);
        }

        public Dictionary<string, THashSet<string>> GenerateReport(List<Item> results)
        {
            var report = new Dictionary<string, THashSet<string>>();
            foreach (Item result in results)
            {
                AddToReport(result, report);
            }

            return report;
        }

        private void AddToReport(Item result, Dictionary<string, THashSet<string>> report)
        {
            if (result.__isset.itemA)
            {
                AddItemA(result.ItemA, report);
            }

            if (result.__isset.itemB)
            {
                AddItemB(result.ItemB, report);
            }

            if (result.__isset.itemC)
            {
                AddItemC(result.ItemC, report);
            }
        }

        private void AddItemA(ItemA result, Dictionary<string, THashSet<string>> report)
        {
            AddField("fieldA", result.FieldA.ToString(), report);
            AddField("fieldB", result.FieldB?.Select(x => x.ToString()), report);
            AddField("fieldC", result.FieldC.ToString(), report);
        }

        private void AddItemB(ItemB result, Dictionary<string, THashSet<string>> report)
        {
            if (result.__isset.fieldA)
            {
                AddField("fieldA", result.FieldA, report);
            }
            if (result.__isset.fieldB)
            {
                AddField("fieldB", result.FieldB, report);
            }
            if (result.__isset.fieldC)
            {
                AddField("fieldC", result.FieldC, report);
            }
        }

        private void AddItemC(ItemC result, Dictionary<string, THashSet<string>> report)
        {
            if (result.__isset.fieldA)
            {
                AddField("fieldA", result.FieldA.ToString(), report);
            }
        }

        private void AddField(string key, string value, Dictionary<string, THashSet<string>> report)
        {
            if (!report.ContainsKey(key))
            {
                report[key] = new THashSet<string>();
            }

            report[key].Add(value);
        }

        private void AddField(string key, IEnumerable<string> values, Dictionary<string, THashSet<string>> report) =>
            AddField(key, string.Join(",", values ?? Array.Empty<string>()), report);
    }
}