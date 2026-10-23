using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PensionMS.API.Controllers;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.IntegrationTests;

public class RetirementFileReviewRefreshTests
{
    [Fact]
    public async Task Read_Submit_Read_ReturnsPersistedReviewStateImmediately()
    {
        var status = FileStatusConstants.New;
        var reviewStatus = 0;
        var query = new Mock<IFileQueryService>();
        query.Setup(s => s.GetByIdAsync(42)).ReturnsAsync(() => ApiResponse<RetirementFileDto>.Success(
            new RetirementFileDto { FileID = 42, StatusID = status, ReviewStatus = reviewStatus }));
        var review = new Mock<IFileReviewService>();
        review.Setup(s => s.SubmitReviewAsync(42, 7)).ReturnsAsync(() =>
        {
            status = FileStatusConstants.Pending;
            reviewStatus = 1;
            return ApiResponse<bool>.Success(true, "تم إرسال الملف للمراجعة");
        });
        var controller = new RetirementFilesController(Mock.Of<IRetirementFileService>(), query.Object,
            Mock.Of<IRetirementFileReportService>(), Mock.Of<IFileStatusService>(), review.Object,
            Mock.Of<IDocumentService>(),
            new UploaderStatsReportExporter(new ExcelReportExporter(new ExcelReportTemplate())))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "7")], "test"))
            } }
        };
        var before = Assert.IsType<ApiResponse<RetirementFileDto>>(Assert.IsType<OkObjectResult>(await controller.GetById(42)).Value);
        Assert.Equal(FileStatusConstants.New, before.Data!.StatusID);
        var submit = Assert.IsType<ApiResponse<bool>>(Assert.IsType<OkObjectResult>(await controller.RequestReview(42)).Value);
        Assert.True(submit.IsSuccess);
        Assert.Equal(FileStatusConstants.Pending, status);
        var after = Assert.IsType<ApiResponse<RetirementFileDto>>(Assert.IsType<OkObjectResult>(await controller.GetById(42)).Value);
        Assert.Equal(FileStatusConstants.Pending, after.Data!.StatusID);
        Assert.Equal(1, after.Data.ReviewStatus);
    }
}
