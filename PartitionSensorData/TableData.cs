namespace PartitionSensorData
{
    public class TableData
    {
        public string SourceAccount { get; set; }
        public string SourceToken { get; set; }
        public string SourceTable { get; set; }
        public string SourcePartition { get; set; }
        public string DestinationAccount { get; set; }
        public string DestinationToken { get; set; }
    }
}