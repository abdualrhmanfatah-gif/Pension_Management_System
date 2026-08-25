using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using PensionMS.Core.Constants;
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
            _snapshotMock.Object);
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
    public async Task GetAllowedStatusTransitionsAsync_UnderReview_ReturnsActiveAndClosed()
    {
        // Arrange
        var fileId = 1;
        var file = new RetirementFile { FileID = fileId, StatusID = FileStatusConstants.UnderReview };
        
        _unitOfWorkMock.Setup(u => u.RetirementFiles.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        var statuses = new List<FileStatus>
        {
            new FileStatus { StatusID = FileStatusConstants.Active, StatusName = "نشط" },
            new FileStatus { StatusID = FileStatusConstants.Closed, StatusName = "مغلق" },
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
        Assert.Contains(result.Data, s => s.StatusID == FileStatusConstants.Closed);
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
