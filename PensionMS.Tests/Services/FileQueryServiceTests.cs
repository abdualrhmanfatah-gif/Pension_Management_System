using PensionMS.Testing;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using Xunit;

namespace PensionMS.Tests.Services;

public class FileQueryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ITestBranchScope> _scope = new();
    private readonly FileQueryService _service;

    public FileQueryServiceTests()
    {
        _scope.Setup(s => s.IsHQ()).Returns(true);
        _service = new FileQueryService(_unitOfWork.Object,
            Mock.Of<IMissingDocumentService>(), TestPensionAccess.From(_unitOfWork.Object, _scope.Object),
            Mock.Of<IPensionSuspensionService>(), Mock.Of<IDocumentService>());
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public async Task GetAllAsync_AppliesBranchScopeExceptForHeadquarters(bool isHQ, int expectedCount)
    {
        SetFiles(NewFile(1, 2), NewFile(2, 3));
        _scope.Setup(s => s.GetCurrentBranchId()).Returns(2);
        _scope.Setup(s => s.IsHQ()).Returns(isHQ);

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedCount, result.Data.TotalCount);
        Assert.Equal(expectedCount, result.Data.Items.Count);
        if (!isHQ) Assert.All(result.Data.Items, row => Assert.Equal(1, row.FileID));
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBeforeCountingAndPaging()
    {
        var suspended = NewFile(9);
        suspended.StatusID = FileStatusConstants.Pending;
        var otherName = NewFile(8);
        otherName.Retiree.FullName = "اسم مختلف";
        SetFiles(NewFile(3), NewFile(5), NewFile(7), suspended, otherName);

        var result = await _service.GetAllAsync(2, 2, search: "أحمد", statusId: FileStatusConstants.Active);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Equal(3, Assert.Single(result.Data.Items).FileID);
        Assert.Equal(2, result.Data.Page);
    }

    [Fact]
    public async Task GetAllAsync_DateRangeUsesYemenDayAndExclusiveUpperBound()
    {
        var start = new DateTime(2024, 3, 4, 21, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var files = new[] { NewFile(1), NewFile(2), NewFile(3), NewFile(4) };
        var times = new[] { start.AddTicks(-1), start, end.AddTicks(-1), end };
        for (var i = 0; i < files.Length; i++)
            files[i].Documents.Add(new Document { CreatedAt = times[i], UploadedByUser = new User { FullName = "موظف" } });
        SetFiles(files);

        var day = new DateTime(2024, 3, 5);
        var result = await _service.GetAllAsync(1, 10, fromDate: day, toDate: day);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(new[] { 3, 2 }, result.Data.Items.Select(row => row.FileID).ToArray());
    }

    [Fact]
    public async Task GetAllAsync_CursorReturnsNextPageWithoutRepeatingBoundary()
    {
        SetFiles(NewFile(7), NewFile(6), NewFile(5), NewFile(4), NewFile(3));

        var result = await _service.GetAllAsync(1, 2, cursor: 6);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(new[] { 5, 4 }, result.Data.Items.Select(row => row.FileID).ToArray());
        Assert.True(result.Data.HasMore);
        Assert.Equal(4, result.Data.NextCursor);
    }

    [Fact]
    public async Task GetByIdAsync_MissingFileReturnsArabicError()
    {
        SetFiles();

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("الملف غير موجود", result.Message);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetDataEntryFilesByUserAsync_ReturnsCreationDateIndependentlyOfUploads(bool hasDocuments)
    {
        var createdAt = new DateTime(2024, 3, 5, 9, 0, 0, DateTimeKind.Utc);
        var uploadedAt = createdAt.AddDays(7);
        var file = NewFile(1);
        file.CreatedByUserID = 12;
        file.CreatedAt = createdAt;
        if (hasDocuments)
            file.Documents.Add(new Document { CreatedAt = uploadedAt });
        SetFiles(file);

        var day = new DateTime(2024, 3, 5);
        var result = await _service.GetDataEntryFilesByUserAsync(12, 1, 10, day, day);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var item = Assert.Single(result.Data.Items);
        Assert.Equal(createdAt, item.CreatedAt);
        Assert.Equal(hasDocuments, item.HasDocuments);
        if (hasDocuments) Assert.Equal(uploadedAt, item.LastUploadDate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetDataEntryStatsAsync_ReturnsLatestCreationDatePerUserWithinFilter(bool filterByDay)
    {
        var firstDate = new DateTime(2024, 3, 5, 9, 0, 0, DateTimeKind.Utc);
        var files = new[] { NewFile(1), NewFile(2), NewFile(3), NewFile(4) };
        for (var i = 0; i < files.Length; i++)
        {
            files[i].CreatedByUserID = i == 2 ? 13 : 12;
            files[i].CreatedAt = firstDate.AddDays(i);
        }
        files[3].IsDeleted = true;
        files[0].Documents.Add(new Document { CreatedAt = firstDate.AddMonths(1) });
        SetFiles(files);
        var users = new Mock<IRepository<User>>();
        users.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<User>(new[]
        {
            new User { UserID = 12, FullName = "مدخل أول", Username = "entry1" },
            new User { UserID = 13, FullName = "مدخل ثان", Username = "entry2" }
        }));
        _unitOfWork.Setup(u => u.Users).Returns(users.Object);
        var filter = new UploaderStatsFilterDto();
        if (filterByDay) filter.FromDate = filter.ToDate = firstDate.Date;

        var result = await _service.GetDataEntryStatsAsync(filter);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var firstUser = Assert.Single(result.Data, s => s.UserID == 12);
        Assert.Equal(filterByDay ? firstDate : firstDate.AddDays(1), firstUser.LastCreatedDate);
        if (!filterByDay)
            Assert.Equal(firstDate.AddDays(2), Assert.Single(result.Data, s => s.UserID == 13).LastCreatedDate);
    }

    [Theory]
    [InlineData(false, 2, 1)]
    [InlineData(true, 2, 2)]
    [InlineData(false, null, 0)]
    public async Task Stats_RespectBranchScopeForUploadsAndEntries(bool isHQ, int? branchId, int expected)
    {
        _scope.Setup(s => s.IsHQ()).Returns(isHQ);
        _scope.Setup(s => s.GetCurrentBranchId()).Returns(branchId);
        var files = new[] { NewFile(1, 2), NewFile(2, 3) };
        foreach (var file in files) file.CreatedByUserID = 12;
        SetFiles(files);
        var documents = new Mock<IRepository<Document>>();
        documents.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Document>(files.Select(file =>
            new Document { FileID = file.FileID, RetirementFile = file, UploadedByUserID = 12 }).ToArray()));
        _unitOfWork.Setup(u => u.Documents).Returns(documents.Object);
        var users = new Mock<IRepository<User>>();
        users.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<User>(new[] { new User { UserID = 12, Username = "entry" } }));
        _unitOfWork.Setup(u => u.Users).Returns(users.Object);

        if (!isHQ && branchId is null)
        {
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() => _service.GetUploaderStatsAsync(new()));
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() => _service.GetDataEntryStatsAsync(new()));
            return;
        }
        var uploads = await _service.GetUploaderStatsAsync(new());
        var entries = await _service.GetDataEntryStatsAsync(new());

        Assert.True(uploads.IsSuccess);
        Assert.True(entries.IsSuccess);
        Assert.Equal(expected, uploads.Data!.Sum(row => row.UniqueFiles));
        Assert.Equal(expected, uploads.Data!.Sum(row => row.TotalUploads));
        Assert.Equal(expected, entries.Data!.Sum(row => row.TotalFilesCreated));
    }

    private void SetFiles(params RetirementFile[] files)
    {
        var repository = new Mock<IRepository<RetirementFile>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(files));
        _unitOfWork.Setup(u => u.RetirementFiles).Returns(repository.Object);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Details_UsePreparedSalaryAndIdentityAndPreserveRetireeLifeStatus(bool byNumber)
    {
        var file = NewFile(1);
        file.Retiree.IsAlive = false;
        file.Documents.Add(new Document { DocumentTypeID = 1, FilePath = "unselected.png" });
        SetFiles(file);
        var service = DetailsService();
        var result = byNumber ? await service.GetByFileNumberAsync(file.FileNumber) : await service.GetByIdAsync(1);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("نتيجة خدمة الإيقاف", result.Data!.SalaryStatus);
        Assert.Equal("selected.png", result.Data.IdDocumentPath);
        Assert.False(result.Data.IsAlive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Details_MissingRetireeIsRejectedInsteadOfAssumingAlive(bool byNumber)
    {
        var file = NewFile(1);
        file.Retiree = null!;
        SetFiles(file);
        var service = DetailsService();
        var result = byNumber ? await service.GetByFileNumberAsync(file.FileNumber) : await service.GetByIdAsync(1);
        Assert.False(result.IsSuccess);
        Assert.Equal("بيانات المتقاعد غير مرتبطة بهذا الملف", result.Message);
    }

    private FileQueryService DetailsService()
    {
        var status = new Mock<IPensionSuspensionService>();
        status.Setup(s => s.GetSalaryStatusAsync(1)).ReturnsAsync(ApiResponse<string>.Success("نتيجة خدمة الإيقاف"));
        var documents = new Mock<IDocumentService>();
        documents.Setup(d => d.GetRetireeIdentityPathAsync(1)).ReturnsAsync(ApiResponse<string?>.Success("selected.png"));
        var missing = new Mock<IMissingDocumentService>();
        missing.Setup(m => m.GetByFileAsync(1)).ReturnsAsync(ApiResponse<List<MissingDocumentDto>>.Success([]));
        missing.Setup(m => m.GetAutoDetectedByFileAsync(1)).ReturnsAsync(ApiResponse<List<AutoDetectedMissingDocumentDto>>.Success([]));
        missing.Setup(c => c.GetComplianceSummaryAsync(1)).ReturnsAsync(new ComplianceSummary());
        var books = EmptyRepository<PaymentBook>();
        var entitled = EmptyRepository<EntitledPerson>();
        var agents = EmptyRepository<Agent>();
        var guardians = EmptyRepository<Guardian>();
        var pledges = EmptyRepository<DocumentPledge>();
        var exemptions = EmptyRepository<DocumentExemption>();
        var settlements = EmptyRepository<SettlementItem>();
        _unitOfWork.Setup(u => u.PaymentBooks).Returns(books);
        _unitOfWork.Setup(u => u.EntitledPersons).Returns(entitled);
        _unitOfWork.Setup(u => u.Agents).Returns(agents);
        _unitOfWork.Setup(u => u.Guardians).Returns(guardians);
        _unitOfWork.Setup(u => u.DocumentPledges).Returns(pledges);
        _unitOfWork.Setup(u => u.DocumentExemptions).Returns(exemptions);
        _unitOfWork.Setup(u => u.SettlementItems).Returns(settlements);
        return new FileQueryService(_unitOfWork.Object, missing.Object,
            TestPensionAccess.From(_unitOfWork.Object, _scope.Object), status.Object, documents.Object);
    }

    private static IRepository<T> EmptyRepository<T>() where T : class
    {
        var repository = new Mock<IRepository<T>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<T>([]));
        return repository.Object;
    }

    private static RetirementFile NewFile(int id, int branchId = 2) => new()
    {
        FileID = id,
        FileNumber = $"ملف-{id}",
        PensionBranchID = branchId,
        StatusID = FileStatusConstants.Active,
        Retiree = new Retiree { FullName = "أحمد محمد", NationalID = $"هوية-{id}", MilitaryNumber = $"عسكري-{id}" }
    };
}
