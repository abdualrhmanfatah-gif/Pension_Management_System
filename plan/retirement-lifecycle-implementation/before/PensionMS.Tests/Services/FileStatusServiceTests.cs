using PensionMS.Infrastructure.Services.Pension.Files;
using Moq;
using System.Linq.Expressions;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using Xunit;

namespace PensionMS.Tests.Services;

public class FileStatusServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISoDGuard> _sodGuardMock = new();
    private readonly Mock<IImmutableAuditService> _auditMock = new();
    private readonly FileStatusService _service;

    public FileStatusServiceTests()
    {
        _service = new FileStatusService(
            _unitOfWorkMock.Object,
            _sodGuardMock.Object,
            Mock.Of<IWorkflowEngine>(),
            Mock.Of<IRiskScoringService>(),
            _auditMock.Object,
            Mock.Of<ISnapshotService>(),
            Mock.Of<INotificationService>());
    }

    [Theory]
    [InlineData(FileStatusConstants.Active)]
    [InlineData(FileStatusConstants.Suspended)]
    [InlineData(FileStatusConstants.Closed)]
    public async Task ChangeStatusAsync_PendingReview_RejectsChangeBeforeSaving(int newStatusId)
    {
        var file = new RetirementFile
        {
            FileID = 7,
            StatusID = FileStatusConstants.UnderReview,
            ReviewStatus = FileReviewStatus.PendingReview
        };
        var repository = new Mock<IRepository<RetirementFile>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(new[] { file }));
        _unitOfWorkMock.Setup(u => u.RetirementFiles).Returns(repository.Object);

        var result = await _service.ChangeStatusAsync(file.FileID, newStatusId, "تغيير الحالة", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(FileStatusConstants.UnderReview, file.StatusID);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        _sodGuardMock.Verify(s => s.ValidateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SoDAction>()), Times.Never);
    }

    [Fact]
    public async Task GetAllowedStatusTransitionsAsync_FileNotFound_ReturnsFail()
    {
        var repository = new Mock<IRepository<RetirementFile>>();
        repository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RetirementFile?)null);
        _unitOfWorkMock.Setup(u => u.RetirementFiles).Returns(repository.Object);

        var result = await _service.GetAllowedStatusTransitionsAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("الملف غير موجود", result.Message);
    }

    [Fact]
    public async Task GetAllowedStatusTransitionsAsync_ExcludesCurrentStatus()
    {
        var file = new RetirementFile { FileID = 1, StatusID = FileStatusConstants.UnderReview };
        var fileRepository = new Mock<IRepository<RetirementFile>>();
        fileRepository.Setup(r => r.GetByIdAsync(file.FileID)).ReturnsAsync(file);
        _unitOfWorkMock.Setup(u => u.RetirementFiles).Returns(fileRepository.Object);
        var statuses = new[]
        {
            new FileStatus { StatusID = FileStatusConstants.UnderReview, StatusName = "تحت المراجعة" },
            new FileStatus { StatusID = FileStatusConstants.Active, StatusName = "نشط" },
            new FileStatus { StatusID = FileStatusConstants.Pending, StatusName = "معلق" }
        };
        var statusRepository = new Mock<IRepository<FileStatus>>();
        statusRepository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<FileStatus>(statuses));
        _unitOfWorkMock.Setup(u => u.FileStatuses).Returns(statusRepository.Object);

        var result = await _service.GetAllowedStatusTransitionsAsync(file.FileID);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.DoesNotContain(result.Data, s => s.StatusID == FileStatusConstants.UnderReview);
        Assert.Contains(result.Data, s => s.StatusID == FileStatusConstants.Active);
        Assert.Contains(result.Data, s => s.StatusID == FileStatusConstants.Pending);
    }

    public static IEnumerable<object[]> TransitionCases => new[]
    {
        new object[] { FileStatusConstants.New, new[] { FileStatusConstants.UnderReview, FileStatusConstants.Pending, FileStatusConstants.Active, FileStatusConstants.Incomplete, FileStatusConstants.Rejected } },
        new object[] { FileStatusConstants.UnderReview, new[] { FileStatusConstants.Active, FileStatusConstants.Pending, FileStatusConstants.Incomplete, FileStatusConstants.Rejected, FileStatusConstants.Suspended } },
        new object[] { FileStatusConstants.Pending, new[] { FileStatusConstants.Active, FileStatusConstants.UnderReview, FileStatusConstants.Incomplete, FileStatusConstants.Rejected } },
        new object[] { FileStatusConstants.Incomplete, new[] { FileStatusConstants.UnderReview, FileStatusConstants.Active, FileStatusConstants.Pending, FileStatusConstants.Rejected } },
        new object[] { FileStatusConstants.Active, new[] { FileStatusConstants.Suspended, FileStatusConstants.Closed, FileStatusConstants.Incomplete } },
        new object[] { FileStatusConstants.Suspended, new[] { FileStatusConstants.Active, FileStatusConstants.Closed, FileStatusConstants.Incomplete } },
        new object[] { FileStatusConstants.Rejected, new[] { FileStatusConstants.UnderReview, FileStatusConstants.Closed } },
        new object[] { FileStatusConstants.Closed, Array.Empty<int>() }
    };

    [Theory]
    [MemberData(nameof(TransitionCases))]
    public async Task GetAllowedStatusTransitionsAsync_RespectsLifecycle(int currentStatus, int[] expectedTransitions)
    {
        var file = SetupFile(currentStatus);
        SetupStatuses();

        var result = await _service.GetAllowedStatusTransitionsAsync(file.FileID);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedTransitions.Order(), result.Data.Select(s => s.StatusID).Order());
    }

    [Theory]
    [MemberData(nameof(TransitionCases))]
    public async Task ChangeStatusAsync_InvalidLifecycleTransitionDoesNotSaveOrInvokeSoD(int currentStatus, int[] allowedTransitions)
    {
        var file = SetupFile(currentStatus);

        foreach (var nextStatus in Enumerable.Range(1, 8).Except(allowedTransitions))
        {
            var result = await _service.ChangeStatusAsync(file.FileID, nextStatus, "اختبار انتقال", 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(currentStatus, file.StatusID);
        }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        _sodGuardMock.Verify(s => s.ValidateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SoDAction>()), Times.Never);
    }

    [Fact]
    public async Task GetAllowedStatusTransitionsAsync_PendingReviewOffersOnlyUnderReview()
    {
        var file = SetupFile(FileStatusConstants.New);
        file.ReviewStatus = FileReviewStatus.PendingReview;
        SetupStatuses();

        var result = await _service.GetAllowedStatusTransitionsAsync(file.FileID);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(FileStatusConstants.UnderReview, Assert.Single(result.Data).StatusID);
    }

    [Theory]
    [InlineData(0, "استحقاق مالي")]
    [InlineData(1, "دفتر صرف")]
    [InlineData(2, "مستلم المعاش")]
    public async Task ChangeStatusAsync_ActivationRequiresFinancialAndPaymentPrerequisites(int missingPrerequisite, string messagePart)
    {
        var file = SetupActivation(missingPrerequisite != 0, missingPrerequisite != 1, missingPrerequisite != 2);

        var result = await _service.ChangeStatusAsync(file.FileID, FileStatusConstants.Active, "تفعيل", 1);

        Assert.False(result.IsSuccess);
        Assert.Contains(messagePart, result.Message);
        Assert.Equal(FileStatusConstants.UnderReview, file.StatusID);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangeStatusAsync_SeparationOfDutiesRejectsActivationBeforeSaving()
    {
        var file = SetupActivation();
        _sodGuardMock.Setup(s => s.ValidateAsync(file.FileID, 1, SoDAction.Activate))
            .ReturnsAsync(SoDResult.Fail("تعارض فصل المهام"));

        var result = await _service.ChangeStatusAsync(file.FileID, FileStatusConstants.Active, "تفعيل", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعارض فصل المهام", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangeStatusAsync_MissingDocumentsRequireUnexpiredCoveringPledge(bool expired)
    {
        var file = SetupActivation();
        var pledge = new DocumentPledge
        {
            PledgeID = 1, FileID = file.FileID, Status = GeneralStatusConstants.Active,
            Deadline = DateTime.UtcNow.AddDays(expired ? -1 : 1)
        };
        file.DocumentPledges.Add(pledge);
        file.MissingDocuments.Add(new MissingDocument
        {
            MissingDocumentID = 5, DocumentTypeID = 1, PledgeID = pledge.PledgeID,
            Status = MissingDocumentStatusConstants.Pending
        });
        SetupRepository(new[] { pledge }, u => u.DocumentPledges);

        var result = await _service.ChangeStatusAsync(file.FileID, FileStatusConstants.Active, "تفعيل", 1);

        Assert.Equal(!expired, result.IsSuccess);
        Assert.Equal(expired ? FileStatusConstants.UnderReview : FileStatusConstants.Active, file.StatusID);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), expired ? Times.Never() : Times.Once());
        if (!expired)
        {
            _unitOfWorkMock.Verify(u => u.StatusTransitionLogs.AddAsync(It.Is<StatusTransitionLog>(log =>
                log.OldStatusID == FileStatusConstants.UnderReview && log.NewStatusID == FileStatusConstants.Active)), Times.Once);
            _auditMock.Verify(a => a.LogAsync(1, "", "ChangeStatus", "RetirementFile", file.FileID,
                It.IsAny<string?>(), FileStatusConstants.Active.ToString(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Once);
        }
    }

    private RetirementFile SetupFile(int statusId)
    {
        var file = new RetirementFile { FileID = 7, RetireeID = 42, StatusID = statusId };
        var repository = new Mock<IRepository<RetirementFile>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(new[] { file }));
        repository.Setup(r => r.GetByIdAsync(file.FileID)).ReturnsAsync(file);
        _unitOfWorkMock.Setup(u => u.RetirementFiles).Returns(repository.Object);
        return file;
    }

    private void SetupStatuses() => SetupRepository(Enumerable.Range(1, 8)
        .Select(id => new FileStatus { StatusID = id, StatusName = $"الحالة {id}" }), u => u.FileStatuses);

    private RetirementFile SetupActivation(bool financial = true, bool book = true, bool recipient = true)
    {
        var file = SetupFile(FileStatusConstants.UnderReview);
        SetupRepository(financial ? new[] { new FinancialEntitlement { RetireeID = file.RetireeID } } : Array.Empty<FinancialEntitlement>(), u => u.FinancialEntitlements);
        SetupRepository(book ? new[] { new PaymentBook { FileID = file.FileID, Status = LookupConstants.ActiveAr } } : Array.Empty<PaymentBook>(), u => u.PaymentBooks);
        SetupRepository(recipient ? new[] { new SalaryRecipient { FileID = file.FileID, IsActive = true } } : Array.Empty<SalaryRecipient>(), u => u.SalaryRecipients);
        SetupRepository(Array.Empty<DocumentType>(), u => u.DocumentTypes);
        SetupRepository(Array.Empty<Agent>(), u => u.Agents);
        SetupRepository(Array.Empty<DocumentPledge>(), u => u.DocumentPledges);
        SetupRepository(Array.Empty<AuditLog>(), u => u.AuditLogs);
        SetupRepository(Array.Empty<StatusTransitionLog>(), u => u.StatusTransitionLogs);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _sodGuardMock.Setup(s => s.ValidateAsync(file.FileID, 1, SoDAction.Activate)).ReturnsAsync(SoDResult.Ok());
        return file;
    }

    private void SetupRepository<T>(IEnumerable<T> rows, Expression<Func<IUnitOfWork, IRepository<T>>> selector) where T : class
    {
        var repository = new Mock<IRepository<T>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<T>(rows));
        _unitOfWorkMock.Setup(selector).Returns(repository.Object);
    }
}
