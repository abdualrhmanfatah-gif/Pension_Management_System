using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentPledges;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Suspensions;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

/// <summary>
/// اختبارات عزل الوثائق عن خدمة الإيقاف (F2):
/// مالك التعهدات يرفض النواقص غير الصالحة قبل أي كتابة، والمنسق يفوض المالك ويرفض ويعلن بلا أصفار صامتة.
/// مسارات القوائم (باستخدام Include) تغطيها اختبارات التكامل فقط.
/// </summary>
public class PensionSuspensionDelegationTests
{
    private static (Mock<IUnitOfWork> Unit, Mock<IRepository<RetirementFile>> Files,
        Mock<IRepository<DocumentPledge>> Pledges, Mock<IRepository<MissingDocument>> Missing)
        NewUnit(List<RetirementFile>? fileRows = null, List<DocumentPledge>? pledgeRows = null,
            List<MissingDocument>? missingRows = null)
    {
        var unit = new Mock<IUnitOfWork>();

        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(fileRows ?? []));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);

        var pledges = new Mock<IRepository<DocumentPledge>>();
        pledges.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<DocumentPledge>(pledgeRows ?? []));
        unit.Setup(u => u.DocumentPledges).Returns(pledges.Object);

        var missing = new Mock<IRepository<MissingDocument>>();
        missing.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<MissingDocument>(missingRows ?? []));
        unit.Setup(u => u.MissingDocuments).Returns(missing.Object);

        return (unit, files, pledges, missing);
    }

    private static RetirementFile NewFile(int id = 1, int? assignedEmployeeID = null) => new()
    { FileID = id, FileNumber = $"F-{id}", PensionBranchID = 1, AssignedEmployeeID = assignedEmployeeID };

    private static DocumentPledge MakePledge(int id, int missingCount, bool deleted = false)
    {
        var pledge = new DocumentPledge { PledgeID = id, FileID = 1, IsDeleted = deleted };
        for (var i = 0; i < missingCount; i++)
            pledge.MissingDocuments.Add(new MissingDocument { MissingDocumentID = i + 1, FileID = 1 });
        return pledge;
    }

    private static DocumentPledgeService NewRealOwner(Mock<IUnitOfWork> unit) =>
        new(unit.Object, Mock.Of<IMissingDocumentService>(), TestPensionAccess.All(unit.Object));

    // ── المالك الحقيقي: رفض قبل أي كتابة ──

    [Theory]
    [InlineData(2, MissingDocumentStatusConstants.Pending)]   // تابع لملف آخر ضمن النطاق
    [InlineData(999, MissingDocumentStatusConstants.Pending)] // غير موجود
    [InlineData(5, MissingDocumentStatusConstants.Pledged)]   // ليست بحالة Pending
    public async Task PledgeOwner_RejectsForeignOrInvalidMissingDocs_BeforeAnyWrite(int missingId, string status)
    {
        var rows = new List<MissingDocument>
        {
            new() { MissingDocumentID = 5, FileID = 1, DocumentTypeID = 1, Status = status }
        };
        var (unit, _, pledges, missing) = NewUnit(fileRows: [NewFile()], missingRows: rows);
        var owner = NewRealOwner(unit);

        var result = await owner.CreateAsync(new CreateDocumentPledgeDto
        {
            FileID = 1,
            MissingDocumentIDs = [missingId],
            Deadline = DateTime.UtcNow.AddDays(30)
        }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal("بعض الوثائق الناقصة غير موجودة أو ليست بحالة Pending", result.Message);
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        pledges.Verify(r => r.AddAsync(It.IsAny<DocumentPledge>()), Times.Never);
        missing.Verify(r => r.AddAsync(It.IsAny<MissingDocument>()), Times.Never);
    }

    // ── المالك الحقيقي: العضو الدفعي لأعداد النواقص ──

    [Fact]
    public async Task PledgeOwner_MissingDocumentCounts_ReturnsCountsAndOmitsUnknownOrDeletedIds()
    {
        List<DocumentPledge> rows =
        [
            MakePledge(77, 3),
            MakePledge(81, 1),
            MakePledge(92, 4),                // غير مطلوب
            MakePledge(93, 2, deleted: true), // محذوف
        ];
        var (unit, _, _, _) = NewUnit(pledgeRows: rows);
        var owner = NewRealOwner(unit);

        var result = await owner.GetMissingDocumentCountsAsync([77, 81, 999, 93]);

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count);
        Assert.Equal(3, result.Data[77]);
        Assert.Equal(1, result.Data[81]);
        Assert.False(result.Data.ContainsKey(999));
        Assert.False(result.Data.ContainsKey(93));
    }

    [Fact]
    public async Task PledgeOwner_MissingDocumentCounts_EmptyInputSkipsQuery()
    {
        var (unit, _, pledges, _) = NewUnit();
        var owner = NewRealOwner(unit);

        var result = await owner.GetMissingDocumentCountsAsync([]);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Empty(result.Data!);
        pledges.Verify(r => r.Query(), Times.Never);
    }

    // ── المنسق مع مالك وهمي ──

    [Fact]
    public async Task Coordinator_ForwardsOwnerRejection_BeforeAnyWrite()
    {
        var (unit, _, _, _) = NewUnit(fileRows: [NewFile()]);
        var suspensions = SuspensionRepo();
        unit.Setup(u => u.PensionSuspensions).Returns(suspensions.Object);
        var notifications = new Mock<INotificationService>();
        var owner = new Mock<IDocumentPledgeService>();
        owner.Setup(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), It.IsAny<int>()))
            .ReturnsAsync(ApiResponse<DocumentPledgeDto>.Fail("بعض الوثائق الناقصة غير موجودة أو ليست بحالة Pending"));
        var service = NewCoordinator(unit, notifications, owner);

        var result = await service.CreateWithPledgeAsync(new CreateSuspensionDto
        {
            FileID = 1, SuspensionReason = "سبب إداري", SuspensionSource = "إيقاف إداري",
            MissingDocumentIDs = [5]
        }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal("بعض الوثائق الناقصة غير موجودة أو ليست بحالة Pending", result.Message);
        suspensions.Verify(r => r.AddAsync(It.IsAny<PensionSuspension>()), Times.Never);
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        notifications.Verify(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Never);
        owner.Verify(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), 9), Times.Once);
    }

    [Fact]
    public async Task Coordinator_Success_DelegatesPledgeLinksSuspensionNotifiesAndMapsResponse()
    {
        var file = NewFile(assignedEmployeeID: 5);
        var (unit, files, pledges, _) = NewUnit(fileRows: [file]);
        files.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(file);

        PensionSuspension? added = null;
        var suspensions = SuspensionRepo();
        suspensions.Setup(r => r.AddAsync(It.IsAny<PensionSuspension>()))
            .Callback<PensionSuspension>(s => { s.SuspensionID = 42; added = s; })
            .Returns(Task.CompletedTask);
        unit.Setup(u => u.PensionSuspensions).Returns(suspensions.Object);

        var pledgeEntity = new DocumentPledge { PledgeID = 77, FileID = 1 };
        pledges.Setup(r => r.GetByIdAsync(77)).ReturnsAsync(pledgeEntity);

        var notifications = new Mock<INotificationService>();
        CreateDocumentPledgeDto? captured = null;
        var owner = new Mock<IDocumentPledgeService>();
        owner.Setup(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), It.IsAny<int>()))
            .Callback<CreateDocumentPledgeDto, int>((dto, _) => captured = dto)
            .ReturnsAsync(ApiResponse<DocumentPledgeDto>.Success(new DocumentPledgeDto
            {
                PledgeID = 77,
                FileID = 1,
                PledgeDate = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Status = "Active",
                PledgedByType = "متقاعد"
            }, "تم إنشاء التعهد"));
        var service = NewCoordinator(unit, notifications, owner);

        var result = await service.CreateWithPledgeAsync(new CreateSuspensionDto
        {
            FileID = 1, SuspensionReason = "تعثر المستندات", SuspensionSource = "إيقاف إداري",
            MissingDocumentIDs = [1, 2, 3], DeadlineDays = 15, PledgedByName = "المتقاعد نفسه"
        }, 9);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("تم إيقاف المعاش بنجاح", result.Message);

        Assert.NotNull(added);
        Assert.Equal(SuspensionStatusConstants.Active, added!.Status);
        Assert.Equal(1, added.FileID);
        Assert.Equal(42, added.SuspensionID);

        Assert.NotNull(captured);
        Assert.Equal(1, captured!.FileID);
        Assert.Equal([1, 2, 3], captured.MissingDocumentIDs);
        Assert.Equal("متقاعد", captured.PledgedByType);
        Assert.Null(captured.SuspensionID); // الربط يجري على التعهد بعد حفظ الإيقاف، لا عبر مدخل الإنشاء

        Assert.Equal(42, pledgeEntity.SuspensionID);
        pledges.Verify(r => r.Update(It.Is<DocumentPledge>(p => p.SuspensionID == 42)), Times.Once);

        var linked = result.Data!.LinkedPledge!;
        Assert.Equal(77, linked.PledgeID);
        Assert.Equal(3, linked.MissingDocumentCount);
        Assert.Equal("متقاعد", linked.PledgedByType);
        Assert.Equal("Active", linked.Status);

        unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
        notifications.Verify(n => n.SendNotificationAsync(It.Is<CreateNotificationDto>(d =>
            d.UserID == 5 && d.ActorID == 9 && d.Title == "إيقاف معاش")), Times.Once);
    }

    [Fact]
    public async Task Coordinator_LinkFailure_FailsExplicitly_WithoutNotification()
    {
        var file = NewFile(assignedEmployeeID: 5);
        var (unit, files, pledges, _) = NewUnit(fileRows: [file]);
        pledges.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((DocumentPledge?)null);
        var suspensions = SuspensionRepo();
        unit.Setup(u => u.PensionSuspensions).Returns(suspensions.Object);
        var notifications = new Mock<INotificationService>();
        var owner = new Mock<IDocumentPledgeService>();
        owner.Setup(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), It.IsAny<int>()))
            .ReturnsAsync(ApiResponse<DocumentPledgeDto>.Success(new DocumentPledgeDto
            { PledgeID = 77, PledgeDate = DateTime.UtcNow, Deadline = DateTime.UtcNow.AddDays(30), Status = "Active" }));
        var service = NewCoordinator(unit, notifications, owner);

        var result = await service.CreateWithPledgeAsync(new CreateSuspensionDto
        {
            FileID = 1, SuspensionReason = "سبب إداري", SuspensionSource = "إيقاف إداري",
            MissingDocumentIDs = [1]
        }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر ربط التعهد بالإيقاف", result.Message);
        unit.Verify(u => u.SaveChangesAsync(), Times.Once); // الإيقاف حُفظ فقط؛ الربط فشل (يبقى حتى F3)
        suspensions.Verify(r => r.AddAsync(It.IsAny<PensionSuspension>()), Times.Once);
        notifications.Verify(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Never);
    }

    [Fact]
    public async Task Coordinator_WithoutMissingDocs_DoesNotTouchPledgeOwner()
    {
        var file = NewFile(assignedEmployeeID: 5);
        var (unit, files, _, _) = NewUnit(fileRows: [file]);
        files.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(file);
        var suspensions = SuspensionRepo();
        unit.Setup(u => u.PensionSuspensions).Returns(suspensions.Object);
        var notifications = new Mock<INotificationService>();
        var owner = new Mock<IDocumentPledgeService>();
        var service = NewCoordinator(unit, notifications, owner);

        var result = await service.CreateWithPledgeAsync(new CreateSuspensionDto
        {
            FileID = 1, SuspensionReason = "سبب إداري", SuspensionSource = "إيقاف إداري"
        }, 9);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Null(result.Data!.LinkedPledge);
        owner.Verify(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), It.IsAny<int>()), Times.Never);
        unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        notifications.Verify(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task Coordinator_ReviewSourceGuard_RejectsBeforeOwnerAndAnyWrite()
    {
        var (unit, _, _, _) = NewUnit(fileRows: [NewFile()]);
        var suspensions = SuspensionRepo();
        unit.Setup(u => u.PensionSuspensions).Returns(suspensions.Object);
        var notifications = new Mock<INotificationService>();
        var owner = new Mock<IDocumentPledgeService>();
        var service = NewCoordinator(unit, notifications, owner);

        var result = await service.CreateWithPledgeAsync(new CreateSuspensionDto
        {
            FileID = 1, SuspensionReason = "سبب", SuspensionSource = RetirementReviewConstants.SuspensionSource,
            MissingDocumentIDs = [5]
        }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal("إيقاف المراجعة يديره مسار المراجعة تلقائياً", result.Message);
        owner.Verify(o => o.CreateAsync(It.IsAny<CreateDocumentPledgeDto>(), It.IsAny<int>()), Times.Never);
        suspensions.Verify(r => r.AddAsync(It.IsAny<PensionSuspension>()), Times.Never);
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── العقد: لا عضو إعفاء ──

    [Fact]
    public void SuspensionContract_HasNoExemptionMembers()
    {
        var members = typeof(IPensionSuspensionService).GetMethods()
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToList();

        Assert.Equal(6, members.Count);
        Assert.DoesNotContain("CreateExemptionAsync", members);
        Assert.DoesNotContain("GetExemptionsByFileIdAsync", members);
    }

    private static Mock<IRepository<PensionSuspension>> SuspensionRepo()
    {
        var repo = new Mock<IRepository<PensionSuspension>>();
        repo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PensionSuspension>([]));
        return repo;
    }

    private static PensionSuspensionService NewCoordinator(Mock<IUnitOfWork> unit,
        Mock<INotificationService> notifications, Mock<IDocumentPledgeService> owner) =>
        new(unit.Object, notifications.Object, TestPensionAccess.All(unit.Object), owner.Object);
}
