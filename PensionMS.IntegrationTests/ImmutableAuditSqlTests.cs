using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PensionMS.Infrastructure.Services.Audit;

namespace PensionMS.IntegrationTests;

public class ImmutableAuditSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task AuditLockTimeout_DoesNotAppendOutsideTheChain()
    {
        await using var holder = database.OpenContext();
        await using var transaction = await holder.Database.BeginTransactionAsync();
        await holder.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = N'PensionMS.ImmutableAudit.Append',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 0;
            IF @result < 0 THROW 51002, N'تعذر تجهيز قفل الاختبار', 1;
            """);
        var before = await holder.ImmutableAuditLogs.CountAsync();
        await using var blocked = database.OpenContext();
        var error = await Assert.ThrowsAsync<SqlException>(() => new ImmutableAuditService(blocked)
            .LogAsync(1, "اختبار", "Blocked", "Test", 1, null, "{}"));
        Assert.Equal(51001, error.Number);
        Assert.Null(blocked.Database.CurrentTransaction);
        Assert.Equal(before, await holder.ImmutableAuditLogs.CountAsync());
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ConcurrentAppends_AndRolledBackAppend_KeepHashesValidAfterReload()
    {
        await Task.WhenAll(Enumerable.Range(1, 12).Select(async id =>
        {
            await using var db = database.OpenContext();
            await new ImmutableAuditService(db).LogAsync(id, "اختبار", "Create", "Test", id, null, "{}");
        }));
        await using (var db = database.OpenContext())
        {
            Assert.Equal(0, await new ImmutableAuditService(db).VerifyChainIntegrityAsync());
            await using var transaction = await db.Database.BeginTransactionAsync();
            await new ImmutableAuditService(db).LogAsync(1, "اختبار", "Rollback", "Test", 99, null, "{}");
            Assert.NotNull(db.Database.CurrentTransaction);
            await transaction.RollbackAsync();
        }
        await using (var db = database.OpenContext())
        {
            await new ImmutableAuditService(db).LogAsync(1, "اختبار", "AfterRollback", "Test", 100, null, "{}");
        }
        await using (var db = database.OpenContext())
        {
            Assert.Equal(13, await db.ImmutableAuditLogs.CountAsync());
            Assert.Equal(0, await new ImmutableAuditService(db).VerifyChainIntegrityAsync());
            var entry = await db.ImmutableAuditLogs.FirstAsync();
            entry.NewValues = "tampered";
            await db.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
            Assert.Equal(1, await new ImmutableAuditService(db).VerifyChainIntegrityAsync());
    }
}
