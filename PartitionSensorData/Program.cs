using PartitionSensorData.Common.Contracts;
using PartitionSensorData.Reader;
using System;
using System.Threading;
using System.Threading.Tasks;
using PartitionSensorData.Common.Shared;
using System.Collections.Generic;
using System.Linq;

namespace PartitionSensorData
{
    class Program
    {
        static void Main(string[] args)
        {
            var td = new TableData
            {
                SourceAccount = "",
                SourceToken = "",
                SourceTable = "",
                SourcePartition = "",
                DestinationAccount = "",
                DestinationToken = "",
            };

            using (var cts = new CancellationTokenSource())
            {
                var t = Loop(td, cts.Token);

                Console.ReadLine();
                cts.Cancel();

                t.Wait();
            }
        }

        private static async Task Loop(TableData data, CancellationToken token)
        {
            ITableWriter writer = null;
            var r = new AzureStorageTableRangeReader(data.SourceAccount, data.SourceToken);

            while (!token.IsCancellationRequested)
            {
                var rows = await r.ReadRangeAsync(data.SourceTable, data.SourcePartition, DateTime.UtcNow);
                var result = await ProcessRowsAsync(rows);
                await writer.WriteAsync(rows);
            }
        }

        private static Task<double> ProcessRowsAsync(IEnumerable<SensorData> rows)
        {
            return Task.FromResult(rows.Average(r => r.Temperature));
        }
    }
}
