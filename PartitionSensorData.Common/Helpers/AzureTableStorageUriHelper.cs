using System;

namespace PartitionSensorData.Common.Helpers
{
    public static class AzureTableStorageUriHelper
    {
        private const string Url = "https://{0}.table.core.windows.net/";

        public static Uri BaseUriFor(string accountName)
        {
            return new Uri(String.Format(Url, accountName));
        }
    }
}