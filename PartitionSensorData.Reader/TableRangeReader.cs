using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using PartitionSensorData.Common.Contracts;
using PartitionSensorData.Common.Extensions;
using PartitionSensorData.Common.Shared;

namespace PartitionSensorData.Reader
{
    public class TableRangeReader : ITableRangeReader
    {
        private static readonly TimeSpan DefaultRange = TimeSpan.FromMinutes(5);
        private readonly CloudTableClient _client;

        public TableRangeReader(string storageAccount, string token)
        {
            var credentials = new StorageCredentials(token);
            _client = new CloudTableClient(new Uri(String.Format("https://{0}.table.core.windows.net/", storageAccount)), credentials);
        }

        public Task<IEnumerable<SensorData>> ReadRangeAsync(string table, string partition, DateTime @from)
        {
            var tbl = _client.GetTableReference(table);
            return Task.Run(() => tbl.CreateQuery<TableSensorData>()
                .Where(s => s.PartitionKey == partition
                            && String.Compare(s.RowKey, @from.ToUnixTime(), StringComparison.Ordinal) >= 0
                            && String.Compare(s.RowKey, @from.Add(DefaultRange).ToUnixTime(), StringComparison.Ordinal) < 0)
                .Select(t => new SensorData
                {
                    At = DateTimeExtensions.FromUnixTimestamp(double.Parse(t.RowKey)).ToUniversalTime(),
                    SensorId = partition,
                    Site = table,
                    Temperature = t.Temperature
                })
                .AsEnumerable());
        }
    }
}
