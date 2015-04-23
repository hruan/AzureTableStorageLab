using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using PartitionSensorData.Common.Extensions;
using Xunit;

namespace PartitionSensorData.Tests
{
    public class TableRangeReader
    {
        private const string AccountName = "";
        private const string AccountKey = "";
        private const string TableName = "Jayway";
        private const string PartitionName = "44";
        private const string QueryPolicyName = "reader";

        [Fact]
        public async Task WhenReadingRangeFromTable_ShouldReturnElementsWithinAFiveMinuteTimespan()
        {
            var account = new CloudStorageAccount(new StorageCredentials(AccountName, AccountKey), useHttps: true);

            var tbl = account.CreateCloudTableClient().GetTableReference(TableName);
            var perm = await tbl.GetPermissionsAsync();
            if (!perm.SharedAccessPolicies.ContainsKey(QueryPolicyName))
            {
                var policy = new SharedAccessTablePolicy
                {
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                    Permissions = SharedAccessTablePermissions.Query
                };
                perm.SharedAccessPolicies.Add(QueryPolicyName, policy);
                await tbl.SetPermissionsAsync(perm);
            }

            var token = tbl.GetSharedAccessSignature(null, QueryPolicyName);
            var sut = new Reader.TableRangeReader(AccountName, token);
            var results = (await sut.ReadRangeAsync(
                TableName,
                PartitionName,
                DateTimeExtensions.FromUnixTimestamp(1429607965).ToUniversalTime()))
                .ToList();

            Assert.NotEmpty(results);
        }
    }
}
