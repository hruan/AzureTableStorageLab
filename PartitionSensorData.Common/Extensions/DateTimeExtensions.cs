using System;
using System.Globalization;

namespace PartitionSensorData.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ToUnixTime(this DateTime t)
        {
            var epoch = t.Kind == DateTimeKind.Utc ? Epoch : Epoch.ToLocalTime();
            return ((int) (t - epoch).TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public static DateTime FromUnixTimestamp(double seconds)
        {
            return Epoch.AddSeconds(seconds).ToLocalTime();
        }
    }
}