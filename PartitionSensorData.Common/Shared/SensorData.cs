using System;

namespace PartitionSensorData.Common.Shared
{
    public class SensorData
    {
        public DateTime At { get; set; }
        public double Temperature { get; set; }
        public string Site { get; set; }
        public string SensorId { get; set; }
    }
}