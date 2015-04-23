using Microsoft.WindowsAzure.Storage.Table;

namespace PartitionSensorData.Reader
{
    public class TableSensorData : TableEntity
    {
        public double Temperature { get; set; }
    }
}