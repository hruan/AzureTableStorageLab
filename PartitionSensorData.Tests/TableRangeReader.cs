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
        private const string InsertPolicyName = "insert";
        private const double StartUnixTimeStamp = 1429607965;

        private static readonly DateTime StartTime = DateTimeExtensions.FromUnixTimestamp(StartUnixTimeStamp).ToUniversalTime();

        private readonly CloudTable _table;

        public TableRangeReader()
        {
            var account = new CloudStorageAccount(new StorageCredentials(AccountName, AccountKey), useHttps: true);
            _table = account.CreateCloudTableClient().GetTableReference(TableName);

            var perm = _table.GetPermissions();
            if (!perm.SharedAccessPolicies.ContainsKey(QueryPolicyName))
            {
                perm.SharedAccessPolicies.Add(QueryPolicyName, new SharedAccessTablePolicy
                {
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                    Permissions = SharedAccessTablePermissions.Query
                });
                _table.SetPermissions(perm);
            }

            if (!perm.SharedAccessPolicies.ContainsKey(InsertPolicyName))
            {
                perm.SharedAccessPolicies.Add(InsertPolicyName, new SharedAccessTablePolicy
                {
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                    Permissions = SharedAccessTablePermissions.Add
                });
                _table.SetPermissions(perm);
            }
        }

        [Fact]
        public async Task WhenReadingRangeFromTable_ReturnElementsWithinAFiveMinuteTimespan()
        {
            var token = _table.GetSharedAccessSignature(null, QueryPolicyName);
            var sut = new Reader.TableRangeReader(AccountName, token);
            var actual = (await sut.ReadRangeAsync(TableName, PartitionName, StartTime).ConfigureAwait(false));

            Assert.All(actual, s => Assert.True(s.At <= StartTime.AddMinutes(5)));
        }

        [Fact]
        public async Task WhenReadingRangeFromTableWithPartialAccess_ReturnOnlyQueryableRows()
        {
            var endTime = StartTime.AddMinutes(5);
            var startTime = endTime.Subtract(TimeSpan.FromSeconds(30));
            var token = _table.GetSharedAccessSignature(
                null,
                QueryPolicyName,
                PartitionName,
                startTime.ToUnixTime(),
                PartitionName,
                endTime.ToUnixTime());
            var sut = new Reader.TableRangeReader(AccountName, token);
            var actual = await sut.ReadRangeAsync(TableName, PartitionName, StartTime).ConfigureAwait(false);

            Assert.All(actual, s => Assert.True(s.At >= startTime && s.At <= endTime));
        }

        [Fact]
        public async Task WhenReadingRangeFromTableWithoutAccess_ReturnsEmptyEnumerable()
        {
            var startTime = StartTime.AddMinutes(10);
            var token = _table.GetSharedAccessSignature(
                null,
                QueryPolicyName,
                PartitionName,
                startTime.ToUnixTime(),
                PartitionName,
                startTime.AddMinutes(5).ToUnixTime());
            var sut = new Reader.TableRangeReader(AccountName, token);
            var actual = await sut.ReadRangeAsync(TableName, PartitionName, StartTime).ConfigureAwait(false);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task WhenReadingRangeFromTableWithoutAccess_ThrowsAnException()
        {
            var token = _table.GetSharedAccessSignature(null, InsertPolicyName);
            var sut = new Reader.TableRangeReader(AccountName, token);
            await Assert.ThrowsAsync<StorageException>(
                () => sut.ReadRangeAsync(TableName, PartitionName, StartTime)
                    .ContinueWith(t => t.Result.ToList()))
                .ConfigureAwait(false);
        }
    }
}
