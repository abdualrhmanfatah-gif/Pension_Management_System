using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace PensionMS.IntegrationTests;

public class AuditIndexSqlTests(SqlTestDatabase database, ITestOutputHelper output) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task IndexScripts_AreRepeatableAndReversible_AndReduceReadsOnSyntheticAuditData()
    {
        await using var db = database.OpenContext();
        await db.Database.ExecuteSqlRawAsync("""
            DROP INDEX IF EXISTS IX_AuditLogs_Record_CreatedAt ON dbo.AuditLogs;
            DROP INDEX IF EXISTS IX_AuditLogs_Recent ON dbo.AuditLogs;
            DROP INDEX IF EXISTS IX_EntitledPersons_FileID ON dbo.EntitledPersons;
            INSERT dbo.AuditLogs (TableID, RecordID, Action, CreatedAt, OldValues, NewValues)
            SELECT 2, n % 1000, N'Update', DATEADD(second, n, CONVERT(datetime2, '20260101')),
                   REPLICATE(N'x', 400), REPLICATE(N'y', 400)
            FROM (SELECT TOP (100000) CONVERT(int, ROW_NUMBER() OVER (ORDER BY (SELECT NULL))) n
                  FROM sys.all_objects a CROSS JOIN sys.all_objects b) source;
            """);
        const string recent = "SELECT TOP (10) LogID, TableID, UserID, Action, EntityDisplayName, CreatedAt FROM dbo.AuditLogs ORDER BY CreatedAt DESC, LogID DESC";
        const string record = "SELECT LogID, CreatedAt, OldValues, NewValues FROM dbo.AuditLogs WHERE TableID = 2 AND RecordID = 17 AND CreatedAt >= '20260101' ORDER BY CreatedAt DESC";
        var recentBefore = await Measure(recent);
        var recordBefore = await Measure(record);
        var apply = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "scripts", "db-access-indexes-apply.sql"));
        await db.Database.ExecuteSqlRawAsync(apply);
        await db.Database.ExecuteSqlRawAsync(apply);
        var recentAfter = await Measure(recent);
        var recordAfter = await Measure(record);
        Assert.Equal(recentBefore.Ids, recentAfter.Ids);
        Assert.Equal(recordBefore.Ids, recordAfter.Ids);
        output.WriteLine($"Synthetic 100000 rows: recent logical reads {recentBefore.Reads} -> {recentAfter.Reads}; record logical reads {recordBefore.Reads} -> {recordAfter.Reads}");
        Assert.True(recentAfter.Reads < recentBefore.Reads);
        Assert.True(recordAfter.Reads < recordBefore.Reads);
        var rollback = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "scripts", "db-access-indexes-rollback.sql"));
        await db.Database.ExecuteSqlRawAsync(rollback);
        await db.Database.ExecuteSqlRawAsync(rollback);
        var recentRestored = await Measure(recent);
        Assert.Equal(recentBefore.Ids, recentRestored.Ids);
        Assert.True(recentRestored.Reads > recentAfter.Reads);

        async Task<(List<long> Ids, long Reads)> Measure(string query)
        {
            await db.Database.OpenConnectionAsync();
            var connection = (SqlConnection)db.Database.GetDbConnection();
            long reads = 0;
            void OnInfo(object sender, SqlInfoMessageEventArgs args)
            {
                foreach (Match match in Regex.Matches(args.Message, @"logical reads (\d+)"))
                    reads += long.Parse(match.Groups[1].Value);
            }
            connection.InfoMessage += OnInfo;
            try
            {
                await using var command = new SqlCommand("SET STATISTICS IO ON; " + query + "; SET STATISTICS IO OFF;", connection) { CommandTimeout = 120 };
                var ids = new List<long>();
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    do { while (await reader.ReadAsync()) ids.Add(reader.GetInt64(0)); }
                    while (await reader.NextResultAsync());
                }
                return (ids, reads);
            }
            finally { connection.InfoMessage -= OnInfo; }
        }
    }
}
