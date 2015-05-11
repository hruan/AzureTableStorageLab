using System.Collections.Generic;
using System.Threading.Tasks;
using PartitionSensorData.Common.Shared;

namespace PartitionSensorData.Common.Contracts
{
    public interface ITableWriter
    {
        Task WriteAsync(object data);
    }
}