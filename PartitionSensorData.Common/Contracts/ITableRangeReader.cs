using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PartitionSensorData.Common.Shared;

namespace PartitionSensorData.Common.Contracts
{
    public interface ITableRangeReader
    {
        Task<IEnumerable<SensorData>> ReadRangeAsync(string table, string partition, DateTime from, TimeSpan stan);
    }
}