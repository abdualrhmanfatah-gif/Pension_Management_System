using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services;
using Xunit;

namespace PensionMS.Tests.Services;

public class RetirementFileServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ISoDGuard> _sodGuardMock;
    private readonly Mock<IWorkflowEngine> _workflowMock;
    private readonly Mock<IRiskScoringService> _riskMock;
    private readonly Mock<IImmutableAuditService> _auditMock;
    private readonly Mock<ISnapshotService> _snapshotMock;
    private readonly RetirementFileService _service;

    public RetirementFileServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _envMock = new Mock<IWebHostEnvironment>();
        _sodGuardMock = new Mock<ISoDGuard>();
        _workflowMock = new Mock<IWorkflowEngine>();
        _riskMock = new Mock<IRiskScoringService>();
        _auditMock = new Mock<IImmutableAuditService>();
        _snapshotMock = new Mock<ISnapshotService>();

        _service = new RetirementFileService(
            _unitOfWorkMock.Object, 
            _envMock.Object, 
            _sodGuardMock.Object,
            _workflowMock.Object,
            _riskMock.Object,
            _auditMock.Object,
            _snapshotMock.Object,
            Mock.Of<IComplianceEngine>(),
            Mock.Of<IMissingDocumentService>(),
            Mock.Of<IDocumentService>(),
            Mock.Of<IFileStorageService>(),
            Mock.Of<IBranchScopeProvider>(),
            Mock.Of<IEntitledPersonService>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IFileReviewService>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RetirementFileService>.Instance);
    }

    [Fact]
    public async Task UpdateAsync_RejectsLifeStatusChangeBeforeSaving()
    {
        var retiree = new Retiree { RetireeID = 42, IsAlive = false, DeathDate = new DateOnly(2024, 3, 4) };
        var file = new RetirementFile { FileID = 7, RetireeID = 42, Retiree = retiree, StatusID = FileStatusConstants.Active };
        SetupQuery(new[] { file }, u => u.RetirementFiles);

        var result = await _service.UpdateAsync(file.FileID, new UpdateRetirementFileDto { IsAlive = true }, 1);

        Assert.False(result.IsSuccess);
        Assert.False(retiree.IsAlive);
        Assert.Equal(new DateOnly(2024, 3, 4), retiree.DeathDate);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangeStatusAsync_CannotActivateBeforeLifeCorrectionReviewIsApproved()
    {
        var file = new RetirementFile { FileID = 7, StatusID = FileStatusConstants.UnderReview,
            ReviewStatus = FileReviewStatus.PendingReview };
        SetupQuery(new[] { file }, u => u.RetirementFiles);

        var result = await _service.ChangeStatusAsync(7, FileStatusConstants.Active, "تفعيل", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(FileStatusConstants.UnderReview, file.StatusID);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CorrectRetireeLifeStatusAsync_StopsPaymentsAndClearsDeathData(bool initiallyAlive)
    {
        var retiree = new Retiree { RetireeID = 42, IsAlive = initiallyAlive, DeathDate = new DateOnly(2024, 3, 4), HealthStatusID = HealthStatusConstants.Deceased };
        var file = new RetirementFile { FileID = 7, RetireeID = 42, Retiree = retiree, PensionBranchID = 2,
            StatusID = initiallyAlive ? FileStatusConstants.Active : FileStatusConstants.Suspended };
        var entitled = new EntitledPerson { EntitledID = 8, FileID = 7, IsActive = true };
        var recipient = new SalaryRecipient { RecipientID = 9, FileID = 7, EntitledID = 8, IsActive = true };
        var agent = new Agent { AgentID = 10, FileID = 7, EntitledID = 8, IsActive = true };
        var book = new PaymentBook { PaymentBookID = 11, FileID = 7, Status = PaymentBookStatus.Active };
        var missing = new MissingDocument { FileID = 7, DocumentTypeID = DocumentTypeConstants.DeathCertificate, Status = MissingDocumentStatusConstants.Pending, Notes = "شهادة وفاة المتقاعد" };
        SetupQuery(new[] { file }, u => u.RetirementFiles);
        SetupQuery(new[] { entitled }, u => u.EntitledPersons);
        SetupQuery(new[] { recipient }, u => u.SalaryRecipients);
        SetupQuery(new[] { agent }, u => u.Agents);
        SetupQuery(new[] { book }, u => u.PaymentBooks);
        SetupQuery(new[] { missing }, u => u.MissingDocuments);
        _unitOfWorkMock.Setup(u => u.Retirees).Returns(Mock.Of<IRepository<Retiree>>());
        _unitOfWorkMock.Setup(u => u.StatusTransitionLogs).Returns(Mock.Of<IRepository<StatusTransitionLog>>());
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
        _sodGuardMock.Setup(s => s.ValidateAsync(7, 1, SoDAction.Override)).ReturnsAsync(SoDResult.Ok());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        var review = new Mock<IFileReviewService>();
        review.Setup(r => r.RequestReviewAsync(7, 1, It.IsAny<DateTime?>()))
            .ReturnsAsync(PensionMS.Core.DTOs.Common.ApiResponse<bool>.Success(true));
        var scoped = new Mock<IBranchScopeProvider>();
        scoped.Setup(s => s.IsHQ()).Returns(true);
        var service = new RetirementFileService(_unitOfWorkMock.Object, _envMock.Object, _sodGuardMock.Object,
            _workflowMock.Object, _riskMock.Object, _auditMock.Object, _snapshotMock.Object,
            Mock.Of<IComplianceEngine>(), Mock.Of<IMissingDocumentService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IFileStorageService>(), scoped.Object, Mock.Of<IEntitledPersonService>(),
            Mock.Of<INotificationService>(), review.Object,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RetirementFileService>.Instance);

        var result = await service.CorrectRetireeLifeStatusAsync(7,
            new CorrectRetireeLifeStatusDto { Reason = "تصحيح إداري موثق", HealthStatusID = HealthStatusConstants.Healthy }, 1);

        Assert.True(result.IsSuccess);
        Assert.True(retiree.IsAlive);
        Assert.Null(retiree.DeathDate);
        Assert.Equal(HealthStatusConstants.Healthy, retiree.HealthStatusID);
        Assert.Equal(FileStatusConstants.UnderReview, file.StatusID);
        Assert.False(entitled.IsActive);
        Assert.Equal(EntitledStopCodes.RetireeAlive, entitled.StopCode);
        Assert.False(recipient.IsActive);
        Assert.False(agent.IsActive);
        Assert.Equal(PaymentBookStatus.Cancelled, book.Status);
        Assert.True(missing.IsDeleted);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        _auditMock.Verify(a => a.LogAsync(1, It.IsAny<string>(), "CorrectRetireeLifeStatus", "Retiree", 42,
            It.IsAny<string>(), It.IsAny<string>(), 2, It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task HandleRetireeDeathAsync_SuspendsFileAndStopsRetireePayment()
    {
        var retiree = new Retiree { RetireeID = 42, IsAlive = true, BirthDate = new DateOnly(1960, 1, 1), HealthStatusID = HealthStatusConstants.Healthy };
        var file = new RetirementFile { FileID = 7, RetireeID = 42, Retiree = retiree, PensionBranchID = 2,
            StatusID = FileStatusConstants.Active, AssignedEmployeeID = 1 };
        var recipient = new SalaryRecipient { RecipientID = 9, FileID = 7, RetireeID = 42, IsActive = true };
        var book = new PaymentBook { PaymentBookID = 11, FileID = 7, Status = PaymentBookStatus.Active };
        SetupQuery(new[] { file }, u => u.RetirementFiles);
        SetupQuery(new[] { recipient }, u => u.SalaryRecipients);
        SetupQuery(new[] { book }, u => u.PaymentBooks);
        SetupQuery(Array.Empty<EntitledPerson>(), u => u.EntitledPersons);
        SetupQuery(Array.Empty<Document>(), u => u.Documents);
        _unitOfWorkMock.Setup(u => u.Retirees).Returns(Mock.Of<IRepository<Retiree>>());
        _unitOfWorkMock.Setup(u => u.StatusTransitionLogs).Returns(Mock.Of<IRepository<StatusTransitionLog>>());
        var missingRepo = new Mock<IRepository<MissingDocument>>();
        missingRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<MissingDocument>(Array.Empty<MissingDocument>()));
        _unitOfWorkMock.Setup(u => u.MissingDocuments).Returns(missingRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _sodGuardMock.Setup(s => s.ValidateAsync(7, 1, SoDAction.Approve)).ReturnsAsync(SoDResult.Ok());
        var scoped = new Mock<IBranchScopeProvider>();
        scoped.Setup(s => s.IsHQ()).Returns(true);
        var service = new RetirementFileService(_unitOfWorkMock.Object, _envMock.Object, _sodGuardMock.Object,
            _workflowMock.Object, _riskMock.Object, _auditMock.Object, _snapshotMock.Object,
            Mock.Of<IComplianceEngine>(), Mock.Of<IMissingDocumentService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IFileStorageService>(), scoped.Object, Mock.Of<IEntitledPersonService>(),
            Mock.Of<INotificationService>(), Mock.Of<IFileReviewService>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RetirementFileService>.Instance);

        var deathDate = new DateOnly(2024, 3, 4);
        var result = await service.HandleRetireeDeathAsync(7, deathDate, 1);

        Assert.True(result.IsSuccess);
        Assert.False(retiree.IsAlive);
        Assert.Equal(deathDate, retiree.DeathDate);
        Assert.Equal(HealthStatusConstants.Deceased, retiree.HealthStatusID);
        Assert.Equal(FileStatusConstants.Suspended, file.StatusID);
        Assert.False(recipient.IsActive);
        Assert.Equal(PaymentBookStatus.Cancelled, book.Status);
        missingRepo.Verify(r => r.AddAsync(It.IsAny<MissingDocument>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    private void SetupQuery<T>(IEnumerable<T> items, Expression<Func<IUnitOfWork, IRepository<T>>> repository)
        where T : class
    {
        var repo = new Mock<IRepository<T>>();
        repo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<T>(items));
        _unitOfWorkMock.Setup(repository).Returns(repo.Object);
    }

    [Fact]
    public async Task GetAllowedStatusTransitionsAsync_FileNotFound_ReturnsFail()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.RetirementFiles.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((RetirementFile?)null);

        // Act
        var result = await _service.GetAllowedStatusTransitionsAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("الملف غير موجود", result.Message);
    }

    [Fact]
    public async Task GetAllowedStatusTransitionsAsync_UnderReview_ReturnsActiveAndPending()
    {
        // Arrange
        var fileId = 1;
        var file = new RetirementFile { FileID = fileId, StatusID = FileStatusConstants.UnderReview };
        
        _unitOfWorkMock.Setup(u => u.RetirementFiles.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        var statuses = new List<FileStatus>
        {
            new FileStatus { StatusID = FileStatusConstants.Active, StatusName = "نشط" },
            new FileStatus { StatusID = FileStatusConstants.Pending, StatusName = "معلق" },
            new FileStatus { StatusID = FileStatusConstants.Suspended, StatusName = "موقوف" }
        }.AsQueryable();

        var queryableMock = new TestAsyncEnumerable<FileStatus>(statuses);

        var statusRepoMock = new Mock<IRepository<FileStatus>>();
        statusRepoMock.Setup(r => r.Query()).Returns(queryableMock);
        _unitOfWorkMock.Setup(u => u.FileStatuses).Returns(statusRepoMock.Object);

        // Act
        var result = await _service.GetAllowedStatusTransitionsAsync(fileId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, s => s.StatusID == FileStatusConstants.Active);
        Assert.Contains(result.Data, s => s.StatusID == FileStatusConstants.Pending);
    }

    [Fact]
    public async Task AssignEmployeeAsync_FileNotFound_ReturnsFail()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.RetirementFiles.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((RetirementFile?)null);

        // Act
        var result = await _service.AssignEmployeeAsync(1, 100, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("الملف غير موجود", result.Message);
    }
}

// Helper classes to mock EF Core IQueryable async extensions (e.g., ToListAsync, FirstOrDefaultAsync)
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}
