using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Infrastructure.Services.Notifications;
using PensionMS.Core.Time;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Testing;

namespace PensionMS.IntegrationTests;

public class FinancialEntitlementCreationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task ConcurrentRequests_UseSettlementSalaryAndCreateOnlyOneEntitlement()
    {
        var (id, actor) = await SeedAsync();
        async Task<int> CreateAsync()
        {
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            var service = Service(unit);
            var response = await service.CreateAsync(new CreateFinancialEntitlementDto
                { RetireeID = id, Amount = 1, EffectiveDate = new DateTime(2026, 1, 1) }, actor);
            Assert.True(response.IsSuccess, response.Message);
            Assert.Equal(120000m, response.Data!.Amount);
            return response.Data.EntitlementID;
        }

        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.Equal(results[0], results[1]);
        await using var verify = database.OpenContext();
        var saved = await verify.FinancialEntitlements.Where(e => e.RetireeID == id).OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(90000m, saved[0].Amount);
        Assert.Equal(new DateTime(2020, 1, 1), saved[0].EffectiveDate);
        Assert.Equal(new DateTime(2025, 12, 31), saved[0].EndDate);
        Assert.Equal(120000m, saved[1].Amount);
        Assert.Null(saved[1].EndDate);
        Assert.Contains("تسوية رتبة", saved[1].Notes!);
    }

    [Fact]
    public async Task NestedCreation_RollbackRestoresOldEntitlementAndRemovesNewOne()
    {
        var (id, actor) = await SeedAsync();
        await using (var seed = database.OpenContext())
        {
            seed.Add(new FinancialEntitlement { RetireeID = id, Amount = 90000,
                EffectiveDate = new DateTime(2020, 1, 1) });
            await seed.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
        {
            using var unit = new UnitOfWork(db);
            await unit.BeginTransactionAsync();
            var response = await Service(unit).CreateAsync(new CreateFinancialEntitlementDto
                { RetireeID = id, Amount = 1, EffectiveDate = new DateTime(2026, 1, 1) }, actor);
            Assert.True(response.IsSuccess, response.Message);
            Assert.True(unit.HasActiveTransaction);
            await unit.RollbackAsync();
        }
        await using var verify = database.OpenContext();
        var saved = Assert.Single(await verify.FinancialEntitlements.Where(e => e.RetireeID == id).ToListAsync());
        Assert.Equal(90000m, saved.Amount);
        Assert.Null(saved.EndDate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_UsesFinancialRankAndRetirementDate(bool hasSettlement)
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var settlement = await db.SettlementItems.SingleAsync(s => s.RetireeID == id);
        var rankId = settlement.ProcessedRankID;
        var retiree = await db.Retirees.Include(r => r.ApprovalItem).SingleAsync(r => r.RetireeID == id);
        var approvalRank = new Rank { RankName = Guid.NewGuid().ToString("N"), BaseSalary = 90000 };
        db.Add(approvalRank);
        await db.SaveChangesAsync();
        retiree.ApprovalItem.RankID = approvalRank.RankID;
        if (!hasSettlement) settlement.IsDeleted = true;
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = branch;
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = FileService(unit, cache);
        var response = await service.CreateAsync(new CreateRetirementFileDto
            { RetireeID = id, BranchID = branch.PensionBranchID }, actor);
        Assert.True(response.IsSuccess, response.Message);
        await using var verify = database.OpenContext();
        var file = await verify.RetirementFiles.SingleAsync(f => f.RetireeID == id);
        var entitlements = await verify.FinancialEntitlements.Where(e => e.RetireeID == id)
            .OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(hasSettlement ? 2 : 1, entitlements.Count);
        Assert.Equal(hasSettlement ? rankId : approvalRank.RankID, file.RankID);
        Assert.Equal(90000m, entitlements[0].Amount);
        Assert.Equal(new DateTime(2020, 1, 1), entitlements[0].EffectiveDate);
        if (hasSettlement)
        {
            Assert.Equal(new DateTime(2025, 12, 31), entitlements[0].EndDate);
            Assert.Equal(new DateTime(2026, 1, 1), entitlements[1].EffectiveDate);
            Assert.Equal(120000m, entitlements[1].Amount);
        }
        Assert.Null(entitlements[^1].EndDate);
    }

    [Theory]
    [InlineData("salary")]
    [InlineData("date")]
    [InlineData("unprocessed")]
    [InlineData("conflict")]
    public async Task FileOpening_StrictFinancialFailureRollsBackFileAndProfile(string reason)
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var retiree = await db.Retirees.Include(r => r.ApprovalItem).SingleAsync(r => r.RetireeID == id);
        if (reason == "salary") retiree.ApprovalItem.Salary = null;
        if (reason == "date") retiree.ApprovalItem.RetirementDate = null;
        if (reason == "unprocessed") retiree.ApprovalItem.IsProcessed = false;
        if (reason == "conflict") db.Add(new FinancialEntitlement { RetireeID = id, Amount = 1,
            EffectiveDate = YemenTime.TodayDateTime });
        var oldPhone = retiree.Phone;
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = branch;
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var response = await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto
            { RetireeID = id, BranchID = branch.PensionBranchID, Phone = "777555666" }, actor);
        Assert.False(response.IsSuccess);
        await using var verify = database.OpenContext();
        Assert.False(await verify.RetirementFiles.AnyAsync(f => f.RetireeID == id));
        Assert.Equal(oldPhone, (await verify.Retirees.FindAsync(id))!.Phone);
        Assert.Equal(reason == "conflict" ? 1 : 0, await verify.FinancialEntitlements.CountAsync(e => e.RetireeID == id));
    }

    [Fact]
    public async Task FileOpening_LaterFailureRollsBackAlreadySavedEntitlement()
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = branch;
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var compliance = new Mock<IMissingDocumentService>();
        compliance.Setup(c => c.RefreshComplianceCacheAsync(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("فشل نهائي اختباري"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => FileService(unit, cache, compliance.Object)
            .CreateAsync(new CreateRetirementFileDto { RetireeID = id, BranchID = branch.PensionBranchID }, actor));
        await using var verify = database.OpenContext();
        Assert.False(await verify.RetirementFiles.AnyAsync(f => f.RetireeID == id));
        Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
    }

    [Fact]
    public async Task ConcurrentOpeningAndFinancialCreation_UseSameSourcePeriodsWithoutDuplicates()
    {
        var (id, actor) = await SeedAsync();
        int branchId;
        await using (var db = database.OpenContext())
        {
            var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            db.Add(branch);
            (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = branch;
            await db.SaveChangesAsync();
            branchId = branch.PensionBranchID;
        }
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task OpenAsync()
        {
            await start.Task;
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var response = await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto
                { RetireeID = id, BranchID = branchId }, actor);
            Assert.True(response.IsSuccess, response.Message);
        }
        async Task CreateFinancialAsync()
        {
            await start.Task;
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            var response = await Service(unit).CreateAsync(new CreateFinancialEntitlementDto { RetireeID = id }, actor);
            Assert.True(response.IsSuccess, response.Message);
        }
        var opening = OpenAsync();
        var financial = CreateFinancialAsync();
        start.SetResult(true);
        await Task.WhenAll(opening, financial);
        await using var verify = database.OpenContext();
        Assert.Single(await verify.RetirementFiles.Where(f => f.RetireeID == id).ToListAsync());
        var rows = await verify.FinancialEntitlements.Where(e => e.RetireeID == id)
            .OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(new DateTime(2020, 1, 1), rows[0].EffectiveDate);
        Assert.Equal(new DateTime(2025, 12, 31), rows[0].EndDate);
        var current = Assert.Single(rows.Where(e => !e.EndDate.HasValue));
        Assert.Equal(120000m, current.Amount);
        Assert.Equal(new DateTime(2026, 1, 1), current.EffectiveDate);
        Assert.All(rows.GroupBy(e => new { e.Amount, e.EffectiveDate }), group => Assert.Single(group));
    }

    [Fact]
    public async Task BR01_ConcurrentFileOpenings_CommitExactlyOneFile()
    {
        var (id, actor) = await SeedAsync();
        await using (var db = database.OpenContext())
        {
            (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = new PensionFundBranch
                { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            await db.SaveChangesAsync();
        }
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> OpenAsync()
        {
            await start.Task;
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            return (await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto { RetireeID = id }, actor)).IsSuccess;
        }
        var first = OpenAsync();
        var second = OpenAsync();
        start.SetResult(true);
        var outcomes = await Task.WhenAll(first, second);
        Assert.Single(outcomes.Where(success => success));
        Assert.Single(outcomes.Where(success => !success));
        await using var verify = database.OpenContext();
        Assert.Equal(1, await verify.RetirementFiles.IgnoreQueryFilters().CountAsync(f => f.RetireeID == id));
        Assert.Equal(2, await verify.FinancialEntitlements.CountAsync(e => e.RetireeID == id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BR01_PreviousFileInAnotherBranchIncludingDeleted_RejectsOpening(bool deleted)
    {
        var (id, actor) = await SeedAsync();
        await using (var db = database.OpenContext())
        {
            var own = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            (await db.Users.SingleAsync(u => u.UserID == actor)).Branch = own;
            db.Add(new RetirementFile { RetireeID = id, FileNumber = Guid.NewGuid().ToString("N"),
                PensionBranch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true },
                StatusID = 1, IsDeleted = deleted });
            await db.SaveChangesAsync();
        }
        await using var context = database.OpenContext();
        using var unit = new UnitOfWork(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var response = await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto { RetireeID = id }, actor);
        Assert.False(response.IsSuccess);
        Assert.Equal("يوجد ملف سابق لهذا المتقاعد؛ لا يمكن فتح ملف آخر", response.Message);
        await using var verify = database.OpenContext();
        Assert.Equal(1, await verify.RetirementFiles.IgnoreQueryFilters().CountAsync(f => f.RetireeID == id));
        Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
    }

    [Theory]
    [InlineData("retiree")]
    [InlineData("financial")]
    [InlineData("rank")]
    public async Task WF01And02_FailureAfterSave_RollsBackProfileFileAndEntitlements(string failure)
    {
        var (id, actor) = await SeedAsync();
        await using (var seed = database.OpenContext())
        {
            (await seed.Users.SingleAsync(u => u.UserID == actor)).Branch = new PensionFundBranch
                { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            await seed.SaveChangesAsync();
        }
        await using var db = database.OpenContext();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var oldPhone = (await db.Retirees.FindAsync(id))!.Phone;
        var financial = new Mock<IFinancialEntitlementService>();
        financial.Setup(s => s.CreateForFileAsync(It.IsAny<int>(), actor)).Returns(async (int file, int user) =>
        {
            var saved = await Service(unit).CreateForFileAsync(file, user);
            Assert.True(saved.IsSuccess, saved.Message);
            if (failure == "rank") saved.Data!.RankID = int.MaxValue;
            return failure == "financial" ? ApiResponse<FinancialEntitlementDto>.Fail("فشل بعد الحفظ") : saved;
        });
        var retiree = new Mock<IRetireeService>();
        retiree.Setup(s => s.StageFileOpeningAsync(id, It.IsAny<UpdateRetireeDto>(), true, null, actor))
            .Returns(async () =>
            {
                (await db.Retirees.FindAsync(id))!.Phone = "تغيير لا يؤكد";
                await unit.SaveChangesAsync();
                return ApiResponse<bool>.Fail("فشل تجهيز المتقاعد بعد الحفظ");
            });
        var response = await FileService(unit, cache, financial: financial.Object,
            retireeOverride: failure == "retiree" ? retiree.Object : null).CreateAsync(new CreateRetirementFileDto
                { RetireeID = id, Phone = "تغيير لا يؤكد" }, actor);
        Assert.False(response.IsSuccess);
        await using var verify = database.OpenContext();
        Assert.Equal(oldPhone, (await verify.Retirees.FindAsync(id))!.Phone);
        Assert.False(await verify.RetirementFiles.IgnoreQueryFilters().AnyAsync(f => f.RetireeID == id));
        Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
    }

    [Fact]
    public async Task BR07_BranchLookupIncludesUnfiledRetireeButDoesNotExposeForeignFile()
    {
        var (unfiledId, _) = await SeedAsync();
        var (foreignId, actor) = await SeedAsync();
        int ownBranch;
        await using (var seed = database.OpenContext())
        {
            var own = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            (await seed.Users.SingleAsync(u => u.UserID == actor)).Branch = own;
            seed.Add(new RetirementFile { RetireeID = foreignId, FileNumber = Guid.NewGuid().ToString("N"), StatusID = 1,
                PensionBranch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true } });
            await seed.SaveChangesAsync();
            ownBranch = own.PensionBranchID;
        }
        await using var db = database.OpenContext();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(i => i.GetCurrentBranchId()).Returns(ownBranch);
        var access = TestPensionAccess.From(unit, identity.Object);
        var service = new RetireeService(unit, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IMissingDocumentService>(), NullLogger<RetireeService>.Instance, cache, access);
        var all = await service.GetAllAsync(1, 1000);
        Assert.Contains(all.Data!.Items, r => r.RetireeID == unfiledId);
        var foreign = Assert.Single(all.Data.Items.Where(r => r.RetireeID == foreignId));
        Assert.Null(foreign.FileID);
        Assert.Equal(0, foreign.FileCount);
        Assert.True((await service.GetFullDataAsync(unfiledId)).IsSuccess);
        Assert.True((await service.GetByIdAsync(foreignId)).IsSuccess);
        var scoped = await access.ForCurrentOperationAsync();
        Assert.False(await scoped.Files().AnyAsync(f => f.RetireeID == foreignId));
        Assert.False(await scoped.CanEditRetireeAsync(foreignId));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DocumentUploadFailure_RollsBackCreationOrUpdateAndDeletesEarlierPhysicalUpload(bool update)
    {
        var (id, actor) = await SeedAsync();
        int fileId = 0;
        await using (var seed = database.OpenContext())
        {
            var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            (await seed.Users.SingleAsync(u => u.UserID == actor)).Branch = branch;
            if (update)
            {
                var file = new RetirementFile { RetireeID = id, PensionBranch = branch, StatusID = FileStatusConstants.New,
                    FileNumber = Guid.NewGuid().ToString("N"), Notes = "ملاحظات أصلية" };
                seed.Add(file);
                await seed.SaveChangesAsync();
                fileId = file.FileID;
            }
            else await seed.SaveChangesAsync();
        }
        using var uploads = new IsolatedUploadDirectory();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.WebRootPath).Returns(uploads.Path);
        environment.SetupGet(e => e.ContentRootPath).Returns(uploads.Path);
        var storage = new LocalFileStorageService(environment.Object, new ConfigurationBuilder().Build());
        await using var db = database.OpenContext();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var oldPhone = (await db.Retirees.FindAsync(id))!.Phone;
        var service = FileService(unit, cache, storageOverride: storage);
        var response = update
            ? await service.UpdateAsync(fileId, new UpdateRetirementFileDto { Phone = "لا يؤكد", Notes = "لا تؤكد",
                IdDocumentBack = new UploadProof(), DeathCertificate = new UploadProof(empty: true) }, actor)
            : await service.CreateAsync(new CreateRetirementFileDto { RetireeID = id, Phone = "لا يؤكد",
                IdDocumentBack = new UploadProof(), PrimaryDocument = new UploadProof(empty: true) }, actor);
        Assert.False(response.IsSuccess);
        Assert.Equal("لم يتم اختيار ملف", response.Message);
        Assert.True(Directory.Exists(System.IO.Path.Combine(uploads.Path, "uploads", "documents")),
            "يجب أن يكون المرفق الأول قد وصل إلى التخزين قبل فشل المرفق التالي");
        Assert.Empty(Directory.EnumerateFiles(uploads.Path, "*", SearchOption.AllDirectories));
        await using var verify = database.OpenContext();
        Assert.Equal(oldPhone, (await verify.Retirees.FindAsync(id))!.Phone);
        Assert.False(await verify.Documents.AnyAsync(d => d.RetireeID == id));
        Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
        var savedFiles = await verify.RetirementFiles.IgnoreQueryFilters().Where(f => f.RetireeID == id).ToListAsync();
        if (update) Assert.Equal("ملاحظات أصلية", Assert.Single(savedFiles).Notes);
        else Assert.Empty(savedFiles);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_OutboxAndFileCommitOrRollbackTogether(bool failAfterStaging)
    {
        var (id, actor) = await SeedAsync();
        await using (var seed = database.OpenContext())
        {
            (await seed.Users.SingleAsync(u => u.UserID == actor)).Branch = new PensionFundBranch
                { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            await seed.SaveChangesAsync();
        }
        await using var db = database.OpenContext();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var hub = new Mock<IHubContext<NotificationHub, INotificationClient>>(MockBehavior.Strict);
        var notifications = new NotificationService(unit, hub.Object, Mock.Of<IFileStorageService>(),
            TestPensionAccess.AllowAllPermissions());
        var failing = new Mock<INotificationService>();
        failing.Setup(n => n.EnqueueAsync(It.IsAny<CreateNotificationDto>())).Returns(async (CreateNotificationDto dto) =>
        {
            await notifications.EnqueueAsync(dto);
            await unit.SaveChangesAsync();
            throw new IOException("فشل بعد حفظ Outbox داخل المعاملة");
        });
        var service = FileService(unit, cache, notificationOverride: failAfterStaging ? failing.Object : notifications);
        if (failAfterStaging)
            await Assert.ThrowsAsync<IOException>(() => service.CreateAsync(new CreateRetirementFileDto { RetireeID = id }, actor));
        else
            Assert.True((await service.CreateAsync(new CreateRetirementFileDto { RetireeID = id }, actor)).IsSuccess);
        await using var verify = database.OpenContext();
        var saved = await verify.Notifications.Where(n => n.ActorID == actor && n.Title == "إنشاء ملف تقاعد جديد").ToListAsync();
        var outbox = await verify.Set<NotificationOutbox>().Where(o => o.Notification.ActorID == actor &&
            o.Notification.Title == "إنشاء ملف تقاعد جديد").ToListAsync();
        Assert.Equal(!failAfterStaging, await verify.RetirementFiles.AnyAsync(f => f.RetireeID == id));
        if (failAfterStaging)
        {
            Assert.Empty(saved);
            Assert.Empty(outbox);
            Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
        }
        else
        {
            var notification = Assert.Single(saved);
            var entry = Assert.Single(outbox);
            Assert.Equal(notification.NotificationID, entry.NotificationId);
            Assert.Equal(OutboxStatus.Pending, entry.Status);
        }
        hub.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RetireeDeath_PersistsLifeBeforeIndependentEvaluationRead_AndRollsBackOnFailure(bool failEvaluation)
    {
        var (id, actor) = await SeedAsync();
        int fileId, entitledId;
        await using (var seed = database.OpenContext())
        {
            var person = new EntitledPerson { FullName = "مستحق اختباري", NationalID = Guid.NewGuid().ToString("N")[..16],
                BirthDate = YemenTime.Today.AddYears(-20), RelationshipID = 2,
                StopCode = Core.DTOs.EntitledPersons.EntitledStopCodes.RetireeAlive,
                RetirementFile = new RetirementFile { RetireeID = id, StatusID = FileStatusConstants.New,
                    FileNumber = Guid.NewGuid().ToString("N"),
                    PensionBranch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true } } };
            seed.Add(person);
            await seed.SaveChangesAsync();
            fileId = person.FileID;
            entitledId = person.EntitledID;
        }
        await using var db = database.OpenContext();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var evaluated = false;
        var people = new Mock<IEntitledPersonService>();
        people.Setup(p => p.ReEvaluateEntitlementStatusAsync(entitledId)).Returns(async () =>
        {
            var persisted = await db.Retirees.AsNoTracking().SingleAsync(r => r.RetireeID == id);
            Assert.False(persisted.IsAlive);
            Assert.Equal(YemenTime.Today, persisted.DeathDate);
            evaluated = true;
            return failEvaluation ? ApiResponse<bool>.Fail("تعذر التقييم") : ApiResponse<bool>.Success(true);
        });
        var service = FileService(unit, cache, entitledOverride: people.Object);
        if (failEvaluation)
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.HandleRetireeDeathAsync(fileId, YemenTime.Today, actor));
        else
            Assert.True((await service.HandleRetireeDeathAsync(fileId, YemenTime.Today, actor)).IsSuccess);
        Assert.True(evaluated);
        await using var verify = database.OpenContext();
        var saved = await verify.Retirees.SingleAsync(r => r.RetireeID == id);
        Assert.Equal(failEvaluation, saved.IsAlive);
        Assert.Equal(failEvaluation ? (DateOnly?)null : YemenTime.Today, saved.DeathDate);
    }

    private sealed class UploadProof(bool empty = false) : IRemoteFile
    {
        private readonly byte[] _content = empty ? [] : "%PDF-1.7\nsynthetic upload\n%%EOF"u8.ToArray();
        public string FileName => "identity.pdf";
        public string ContentType => "application/pdf";
        public long Length => _content.Length;
        public Stream OpenReadStream() => new MemoryStream(_content);
    }

    private sealed class IsolatedUploadDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            "PensionMS_DocumentUpload_" + Guid.NewGuid().ToString("N")));

        public IsolatedUploadDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            var temporaryRoot = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath())
                .TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            if (!string.Equals(Directory.GetParent(Path)?.FullName, temporaryRoot, StringComparison.OrdinalIgnoreCase) ||
                !System.IO.Path.GetFileName(Path).StartsWith("PensionMS_DocumentUpload_", StringComparison.Ordinal))
                throw new InvalidOperationException("مسار تنظيف مرفقات الاختبار غير متوقع");
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }

    private static RetirementFileService FileService(IUnitOfWork unit, IMemoryCache cache,
        IMissingDocumentService? compliance = null, IFinancialEntitlementService? financial = null,
        IRetireeService? retireeOverride = null, IFileStorageService? storageOverride = null,
        INotificationService? notificationOverride = null, IEntitledPersonService? entitledOverride = null)
    {
        var access = TestPensionAccess.All(unit);
        if (compliance == null)
        {
            var missing = new Mock<IMissingDocumentService>();
            missing.Setup(m => m.GetRequiredPairsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Core.DTOs.MissingDocuments.AutoDetectedMissingDocumentDto>());
            compliance = missing.Object;
        }
        var review = new Mock<IFileReviewService>();
        review.Setup(r => r.RequestReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(ApiResponse<bool>.Success(true));
        var storage = storageOverride ?? Mock.Of<IFileStorageService>();
        var documents = new DocumentService(unit, storage,
            Mock.Of<IEntitlementEngine>(), review.Object, access,
            Mock.Of<IImmutableAuditService>(), NullLogger<DocumentService>.Instance, compliance);
        var retiree = new RetireeService(unit, storage, documents, compliance,
            NullLogger<RetireeService>.Instance, cache, access);
        return new RetirementFileService(unit, Mock.Of<IImmutableAuditService>(), compliance,
            documents, access, entitledOverride ?? Mock.Of<IEntitledPersonService>(),
            notificationOverride ?? Mock.Of<INotificationService>(), review.Object, NullLogger<RetirementFileService>.Instance,
            retireeOverride ?? retiree, financial ?? Service(unit), new PensionSuspensionService(unit, Mock.Of<INotificationService>(), access,
                new DocumentPledgeService(unit, compliance, access)));
    }

    private static FinancialEntitlementService Service(IUnitOfWork unit) => new(unit,
        Mock.Of<IMissingDocumentService>(), Mock.Of<INotificationService>(),
        Mock.Of<IImmutableAuditService>(), NullLogger<FinancialEntitlementService>.Instance, TestPensionAccess.All(unit));

    private async Task<(int RetireeId, int ActorId)> SeedAsync()
    {
        await using var db = database.OpenContext();
        var suffix = Guid.NewGuid().ToString("N")[..16];
        var actor = new User { Username = suffix, FullName = "اختبار الاستحقاق", PasswordHash = "test" };
        var retiree = new Retiree { FullName = suffix, NationalID = suffix,
            ApprovalItem = new MinistryApprovalItem { FullName = suffix, IsProcessed = true,
                Salary = 90000, EnrollmentDate = new DateOnly(1980, 1, 1), RetirementDate = new DateOnly(2020, 1, 1),
                Batch = new MinistryApprovalBatch { ImportedByUser = actor, Status = "Test" } } };
        var rank = new Rank { RankName = suffix, BaseSalary = 120000 };
        db.Add(new SettlementItem { Retiree = retiree, CurrentRank = rank, ProcessedRank = rank,
            Batch = new SettlementsBatch { CreatedByUser = actor, Status = "Test" },
            Decision = new SettlementDecision { CreatedByUser = actor, DecisionNumber = suffix,
                DecisionDate = new DateTime(2026, 1, 1) } });
        await db.SaveChangesAsync();
        return (retiree.RetireeID, actor.UserID);
    }
}
