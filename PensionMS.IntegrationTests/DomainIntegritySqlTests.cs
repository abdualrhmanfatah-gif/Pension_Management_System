using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using CreateLeaseDto = PensionMS.Core.DTOs.Investment.CreateLeaseDto;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Assets;
using PensionMS.Infrastructure.Services.Audit;
using PensionMS.Infrastructure.Services.HR;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class DomainIntegritySqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task AttendanceHistory_UsesSavedIdentity_AndFailureRollsBackBothRows()
    {
        int employeeId;
        await using (var db = database.OpenContext())
        {
            var employee = new Employee { FullName = "موظف اختبار", EmployeeNumber = "test-attendance", NationalID = "test-attendance",
                Department = new Department { Code = "test", Name = "اختبار" },
                JobTitle = new JobTitle { Code = "test", Name = "اختبار" },
                EmploymentStatus = new EmploymentStatus { Code = "test", Name = "اختبار" } };
            db.Add(employee);
            await db.SaveChangesAsync();
            employeeId = employee.EmployeeID;
        }
        var dto = new CreateAttendanceEventDto { EmployeeID = employeeId, EventDate = new DateTime(2026, 10, 9), EventType = AttendanceConstants.CheckIn, EventTime = "08:00" };
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var result = await new HrAttendanceService(uow).RecordAttendanceAsync(dto, 1);
            Assert.True(result.IsSuccess, result.Message);
            var history = await db.Set<ServiceHistoryEvent>().SingleAsync(h => h.EmployeeID == employeeId);
            Assert.Equal(result.Data!.AttendanceEventID, history.SourceEntityId);
            Assert.True(history.SourceEntityId > 0);
        }
        await using (var db = database.OpenContext(new FailOnSave(2)))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => new HrAttendanceService(uow).RecordAttendanceAsync(dto, 1));
        await using (var db = database.OpenContext())
        {
            Assert.Equal(1, await db.Set<AttendanceEvent>().CountAsync(a => a.EmployeeID == employeeId));
            Assert.Equal(1, await db.Set<ServiceHistoryEvent>().CountAsync(a => a.EmployeeID == employeeId));
        }
    }

    [Fact]
    public async Task LeaseAndSchedules_CommitTogether_OrRollbackAfterSecondSave()
    {
        int unitId, tenantId;
        await using (var db = database.OpenContext())
        {
            var unit = new PropertyUnit { UnitNumber = "test-lease", Area = 100, UnitType = new UnitType { TypeName = "اختبار" },
                Property = new InvestmentProperty { PropertyName = "اختبار", TotalArea = 100, InitialValue = 1000, PropertyType = new PropertyType { TypeName = "اختبار" },
                    PensionBranch = new PensionFundBranch { BranchName = "اختبار عقود" } } };
            var tenant = new Tenant { FullName = "اختبار عقد", NationalIDOrCR = "test-lease" };
            db.AddRange(unit, tenant);
            await db.SaveChangesAsync();
            (unitId, tenantId) = (unit.UnitID, tenant.TenantID);
        }
        var dto = new CreateLeaseDto { UnitID = unitId, TenantID = tenantId, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2027, 1, 1),
            TotalContractValue = 1000, GenerateSchedules = true, PaymentFrequency = PaymentFrequencyConstants.Monthly };
        await using (var db = database.OpenContext(new FailOnSave(2)))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => new InvestmentLeasingService(uow, PensionMS.Testing.TestPensionAccess.All(uow)).CreateLeaseContractAsync(dto, 1));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.False(await db.Set<LeaseContract>().AnyAsync(c => c.UnitID == unitId));
            var result = await new InvestmentLeasingService(uow, PensionMS.Testing.TestPensionAccess.All(uow)).CreateLeaseContractAsync(dto, 1);
            Assert.True(result.IsSuccess, result.Message);
            var schedules = await db.Set<PaymentSchedule>().Where(s => s.ContractID == result.Data).ToListAsync();
            Assert.Equal(12, schedules.Count);
            Assert.Equal(1000, schedules.Sum(s => s.ExpectedAmount));
        }
    }

    [Fact]
    public async Task AssetJournalFailure_RollsBackAssetAndAudit_AndDepreciationStartsInServiceMonth()
    {
        int userId, accountId, categoryId, typeId;
        await using (var db = database.OpenContext())
        {
            var user = new User { Username = "test-asset", FullName = "اختبار", PasswordHash = "test" };
            var account = await db.Set<ChartOfAccount>().FirstOrDefaultAsync(a => a.AccountCode == "1811")
                ?? new ChartOfAccount { AccountCode = "1811", AccountName = "اختبار", AccountNature = "Debit", IsPostable = true };
            var type = new FixedAssetType { TypeCode = "TST", TypeName = "اختبار" };
            var category = new FixedAssetCategory { CategoryName = "اختبار", AssetType = type };
            db.AddRange(user, category);
            if (account.AccountID == 0) db.Add(account);
            await db.SaveChangesAsync();
            (userId, accountId, categoryId, typeId) = (user.UserID, account.AccountID, category.CategoryID, type.AssetTypeID);
        }
        var dto = new CreateFixedAssetDto { AssetName = "test-asset", AssetTypeID = typeId, CategoryID = categoryId,
            AssetAccountID = accountId, AccumDeprAccountID = accountId, DeprExpenseAccountID = accountId,
            UnitPrice = 1200, Quantity = 1, PurchaseDate = new DateTime(2026, 7, 16), InServiceDate = new DateTime(2026, 7, 16), DepreciationRate = 12 };
        var journal = new Mock<IJournalEntryService>();
        journal.Setup(j => j.GetPeriodIdForDateAsync(It.IsAny<DateTime>())).ReturnsAsync(1);
        journal.Setup(j => j.CreateJournalEntryAsync(It.IsAny<CreateJournalEntryDto>(), userId))
            .ReturnsAsync(ApiResponse<JournalEntryDto>.Fail("فشل اختباري"));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
            Assert.False((await new FixedAssetService(uow, new ImmutableAuditService(db), journal.Object, db).CreateAsync(dto, userId)).IsSuccess);
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.False(await db.FixedAssets.IgnoreQueryFilters().AnyAsync(a => a.AssetName == "test-asset"));
            Assert.False(await db.ImmutableAuditLogs.AnyAsync(a => a.ActorId == userId));
            dto.SkipJournalEntry = true;
            var service = new FixedAssetService(uow, new ImmutableAuditService(db), journal.Object, db);
            var result = await service.CreateAsync(dto, userId);
            Assert.True(result.IsSuccess, result.Message);
            Assert.True((await service.RunYearlyDepreciationAsync(2026, userId)).IsSuccess);
            Assert.True((await service.RunYearlyDepreciationAsync(2026, userId)).IsSuccess);
            var schedules = await db.DepreciationSchedules.AsNoTracking().Where(s => s.AssetID == result.Data!.AssetID).OrderBy(s => s.PeriodMonth).ToListAsync();
            Assert.Equal(6, schedules.Count);
            Assert.Equal(7, schedules[0].PeriodMonth);
            Assert.Equal(Math.Round(12m * 16 / 31, 2), schedules[0].DepreciationAmount);
            Assert.False((await service.ResetCategoriesAsync()).IsSuccess);
        }
    }

    [Fact]
    public async Task DeleteUser_PreservesHistory_AndRevokesCredentials()
    {
        int userId, actorId;
        await using (var db = database.OpenContext())
        {
            var actor = new User { Username = "delete-actor", FullName = "منفذ", PasswordHash = "test" };
            var user = new User { Username = "delete-target", FullName = "اختبار", PasswordHash = "test", RefreshToken = "legacy" };
            db.AddRange(actor, user);
            db.Add(new RefreshToken { User = user, Token = "test-refresh", ExpiresAt = DateTime.UtcNow.AddDays(1) });
            db.Add(new DeviceRegistration { User = user, DeviceID = "test-device" });
            db.Add(new LoginHistory { User = user });
            await db.SaveChangesAsync();
            (userId, actorId) = (user.UserID, actor.UserID);
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
            Assert.True(await new UserService(uow, Mock.Of<INotificationService>(), PensionMS.Testing.TestPensionAccess.All(uow), Mock.Of<IImmutableAuditService>(), new ScopedPermissionResolver(db)).DeleteUserAsync(userId, actorId));
        await using (var db = database.OpenContext())
        {
            Assert.False(await db.Set<User>().AnyAsync(u => u.UserID == userId));
            var user = await db.Set<User>().IgnoreQueryFilters().SingleAsync(u => u.UserID == userId);
            Assert.False(user.IsActive);
            Assert.Equal(actorId, user.DeletedByUserID);
            Assert.Null(user.RefreshToken);
            Assert.True((await db.Set<RefreshToken>().SingleAsync(t => t.UserId == userId)).IsRevoked);
            Assert.False((await db.Set<DeviceRegistration>().SingleAsync(d => d.UserID == userId)).IsActive);
            Assert.True(await db.Set<LoginHistory>().AnyAsync(h => h.UserID == userId));
        }
    }

    private sealed class FailOnSave(int ordinal) : SaveChangesInterceptor
    {
        private int _saves;
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) => ++_saves == ordinal
                ? throw new InvalidOperationException("فشل اختباري بعد الحفظ") : new ValueTask<int>(result);
    }
}
