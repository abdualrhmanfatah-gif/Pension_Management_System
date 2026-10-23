using PensionMS.Infrastructure.Services.Pension.Files;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using Xunit;

namespace PensionMS.Tests.Services;

public class FileReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FileReviewService _service;

    public FileReviewServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new FileReviewService(_unitOfWorkMock.Object);
    }

    private void SetupFile(int statusId)
    {
        var file = new RetirementFile { FileID = 1, StatusID = statusId };
        _unitOfWorkMock.Setup(u => u.RetirementFiles.Query())
            .Returns(new TestAsyncEnumerable<RetirementFile>(new[] { file }));
    }

    [Theory]
    [InlineData(FileStatusConstants.Suspended)]
    [InlineData(FileStatusConstants.Closed)]
    [InlineData(FileStatusConstants.Rejected)]
    public async Task RequestReviewAsync_BlockedStatus_ReturnsFailWithoutStateChange(int statusId)
    {
        SetupFile(statusId);

        var result = await _service.RequestReviewAsync(1, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن طلب مراجعة لملف بحالة موقوف أو مغلق أو مرفوض", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
