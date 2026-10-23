using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetirementFileDocumentCreationTests
{
    [Theory]
    [InlineData(true, "none")]
    [InlineData(false, "none")]
    [InlineData(true, "reject")]
    [InlineData(false, "reject")]
    [InlineData(true, "throw")]
    [InlineData(false, "throw")]
    public async Task Create_UsesDocumentOwnersAndDeferredRefresh_AndKeepsPartialUploadWarnings(bool alive, string failure)
    {
        using var f = new Fixture(failure);
        var result = await f.Service.CreateAsync(f.Dto(alive), 1);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileStatusConstants.New, result.Data!.StatusID);
        Assert.Equal("موقوف", result.Data.SalaryStatus);
        Assert.Equal("identity/selected.png", result.Data.IdDocumentPath);
        Assert.Equal(77, f.CreatedFile!.RankID);
        f.Financial.Verify(s => s.CreateForFileAsync(7, 1), Times.Once);
        var calls = f.Unit.Invocations.Select(i => i.Method.Name).ToList();
        Assert.True(calls.IndexOf(nameof(IUnitOfWork.LockRetireeEntitlementsAsync)) <
            calls.IndexOf(nameof(IUnitOfWork.SaveChangesAsync)));
        var personal = Assert.Single(f.Uploads.Where(d => d.DocumentTypeID != DocumentTypeConstants.FileOriginals));
        Assert.Equal(alive ? DocumentTypeConstants.NationalId : DocumentTypeConstants.DeathCertificate, personal.DocumentTypeID);
        Assert.Equal(2, personal.RetireeID);
        Assert.Equal(7, personal.RetirementFileID);
        var originals = Assert.Single(f.Uploads.Where(d => d.DocumentTypeID == DocumentTypeConstants.FileOriginals));
        Assert.Null(originals.RetireeID);
        Assert.Equal(7, originals.RetirementFileID);
        f.Missing.Verify(m => m.StageRequiredAsync(7, 1), Times.Once);
        f.Documents.Verify(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, true), Times.Exactly(2));
        f.Documents.Verify(d => d.RefreshAfterUploadAsync(7,
            It.Is<List<int>>(ids => ids.Count == (failure == "none" ? 2 : 1)), 1, null), Times.Once);
        f.StoredDocuments.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
        if (failure != "none") Assert.Contains("مع تنبيه", result.Message!);
    }

    [Fact]
    public async Task Create_FailedFinalRefresh_RollsBackFileCreation()
    {
        using var f = new Fixture("none");
        f.Documents.Setup(d => d.RefreshAfterUploadAsync(7, It.IsAny<List<int>>(), 1, null))
            .ReturnsAsync(ApiResponse<RefreshAfterUploadResult>.Fail("تعذر تحديث المراجعة"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.CreateAsync(f.Dto(false), 1));
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData("photo.pdf", 3, false)]
    [InlineData("photo.jpg", 0, false)]
    [InlineData("photo.png", 2097153, false)]
    [InlineData("photo.JPEG", 2097152, true)]
    public async Task Create_UsesSharedPhotoPolicyAndPreservesUnspecifiedProfile(string name, long length, bool succeeds)
    {
        using var f = new Fixture("none");
        var dto = f.Dto(true);
        dto.PersonalPhoto = new UploadFile(name, length);
        dto.BirthDate = new DateOnly(1960, 2, 3);
        var result = await f.Service.CreateAsync(dto, 1);

        Assert.Equal(succeeds, result.IsSuccess);
        Assert.Equal("رقم سابق", f.Retiree.NationalID);
        Assert.Equal("هاتف سابق", f.Retiree.Phone);
        f.Storage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        f.Unit.Verify(u => u.Retirees.Update(It.IsAny<Retiree>()), Times.Never);
        if (succeeds)
        {
            Assert.Equal("photos/new.jpg", f.Retiree.PersonalPhotoPath);
            Assert.Equal(dto.BirthDate, f.Retiree.BirthDate);
            Assert.Equal(1, f.Retiree.UpdatedByUserID);
            f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), name, "photos",
                It.Is<string[]?>(e => e != null && e.SequenceEqual(new[] { ".jpg", ".jpeg", ".png" })), 2097152L), Times.Once);
            f.Unit.Verify(u => u.CommitAsync(), Times.Once);
        }
        else
        {
            Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
            Assert.Null(f.Retiree.BirthDate);
            f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
            f.Unit.Verify(u => u.RetirementFiles.AddAsync(It.IsAny<RetirementFile>()), Times.Never);
            f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
            f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        }
    }

    [Fact]
    public async Task Create_PhotoStorageFailureDoesNotSaveOrChangeLifeStatus()
    {
        using var f = new Fixture("none");
        f.Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "photos",
            It.IsAny<string[]?>(), It.IsAny<long?>())).ThrowsAsync(new IOException("تعذر التخزين"));
        var dto = f.Dto(false);
        dto.PersonalPhoto = new UploadFile();

        Assert.False((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        Assert.True(f.Retiree.IsAlive);
        Assert.Null(f.Retiree.DeathDate);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData("معاش الإحالة مفقود")]
    [InlineData("تاريخ التقاعد مفقود")]
    [InlineData("المعالجة غير مكتملة")]
    [InlineData("تعارض مالي")]
    public async Task Create_FinancialFailureRejectsWholeOpeningBeforeDocuments(string error)
    {
        using var f = new Fixture("none");
        f.Financial.Setup(s => s.CreateForFileAsync(7, 1)).ReturnsAsync(ApiResponse<FinancialEntitlementDto>.Fail(error));
        var result = await f.Service.CreateAsync(f.Dto(true), 1);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Message);
        Assert.Empty(f.Uploads);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Missing.Verify(s => s.StageRequiredAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Create_FinancialResponseWithoutRankRejectsWholeOpening()
    {
        using var f = new Fixture("none");
        f.Financial.Setup(s => s.CreateForFileAsync(7, 1)).ReturnsAsync(
            ApiResponse<FinancialEntitlementDto>.Success(new FinancialEntitlementDto { Amount = 120000 }));
        Assert.False((await f.Service.CreateAsync(f.Dto(true), 1)).IsSuccess);
        Assert.Empty(f.Uploads);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_ResponsePreparationFailureRollsBackBeforeCommit(bool identityFailure)
    {
        using var f = new Fixture("none");
        if (identityFailure)
            f.Documents.Setup(d => d.GetRetireeIdentityPathAsync(7)).ReturnsAsync(ApiResponse<string?>.Fail("تعذر قراءة الهوية"));
        else
            f.Suspensions.Setup(s => s.GetSalaryStatusAsync(7)).ReturnsAsync(ApiResponse<string>.Fail("تعذر قراءة حالة الصرف"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.CreateAsync(f.Dto(true), 1));
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    private sealed class Fixture : IDisposable
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IMissingDocumentService> Missing { get; } = new();
        public Mock<IDocumentService> Documents { get; } = new();
        public Mock<IFinancialEntitlementService> Financial { get; } = new();
        public Mock<IPensionSuspensionService> Suspensions { get; } = new();
        public RetirementFile? CreatedFile { get; private set; }
        public Mock<IRepository<Document>> StoredDocuments { get; }
        public List<UploadDocumentDto> Uploads { get; } = [];
        public Retiree Retiree { get; } = new() { RetireeID = 2, FullName = "متقاعد",
            NationalID = "رقم سابق", Phone = "هاتف سابق", PersonalPhotoPath = "photos/old.jpg" };
        public Mock<IFileStorageService> Storage { get; } = new();
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        public RetirementFileService Service { get; }

        public Fixture(string failure)
        {
            var retiree = Retiree;
            var files = Bind<RetirementFile>(u => u.RetirementFiles, []);
            var stored = new List<RetirementFile>();
            files.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<RetirementFile>(stored));
            files.Setup(r => r.AddAsync(It.IsAny<RetirementFile>())).Callback<RetirementFile>(file =>
            {
                file.FileID = 7;
                file.Retiree = retiree;
                CreatedFile = file;
                stored.Add(file);
            }).Returns(Task.CompletedTask);
            Bind(u => u.Retirees, [retiree]).Setup(r => r.GetByIdAsync(2)).ReturnsAsync(retiree);
            Bind(u => u.Users, [new User { UserID = 1, BranchID = 1, IsActive = true }]);
            Bind(u => u.PensionFundBranches, [new PensionFundBranch { PensionBranchID = 1, IsActive = true }]);
            Bind<SettlementItem>(u => u.SettlementItems, []);
            StoredDocuments = Bind<Document>(u => u.Documents, []);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Financial.Setup(s => s.CreateForFileAsync(7, 1)).ReturnsAsync(
                ApiResponse<FinancialEntitlementDto>.Success(new FinancialEntitlementDto { Amount = 120000, RankID = 77 }));
            Suspensions.Setup(s => s.GetSalaryStatusAsync(7)).ReturnsAsync(ApiResponse<string>.Success("موقوف"));
            Documents.Setup(d => d.GetRetireeIdentityPathAsync(7)).ReturnsAsync(ApiResponse<string?>.Success("identity/selected.png"));
            Unit.Setup(u => u.BeginTransactionAsync()).Callback(() => Unit.Setup(u => u.HasActiveTransaction).Returns(true))
                .Returns(Task.CompletedTask);
            Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "photos",
                It.IsAny<string[]?>(), It.IsAny<long?>())).ReturnsAsync("photos/new.jpg");
            Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, true))
                .ReturnsAsync((UploadDocumentDto dto, int user, string? request, bool defer) =>
                {
                    Uploads.Add(dto);
                    if (dto.DocumentTypeID != DocumentTypeConstants.FileOriginals && failure == "throw")
                        throw new IOException("تعذر رفع المرفق");
                    if (dto.DocumentTypeID != DocumentTypeConstants.FileOriginals && failure == "reject")
                        return ApiResponse<DocumentDto>.Fail("المرفق غير صالح");
                    return ApiResponse<DocumentDto>.Success(new DocumentDto { DocumentID = Uploads.Count });
                });
            Documents.Setup(d => d.RefreshAfterUploadAsync(7, It.IsAny<List<int>>(), 1, null))
                .ReturnsAsync(ApiResponse<RefreshAfterUploadResult>.Success(new()));
            Service = new RetirementFileService(Unit.Object, Mock.Of<IImmutableAuditService>(),
                Missing.Object, Documents.Object,
                TestPensionAccess.All(Unit.Object), Mock.Of<IEntitledPersonService>(),
                Mock.Of<INotificationService>(), Mock.Of<IFileReviewService>(), NullLogger<RetirementFileService>.Instance,
                new RetireeService(Unit.Object, Storage.Object, Documents.Object,
                    Missing.Object, NullLogger<RetireeService>.Instance,
                    _cache, TestPensionAccess.All(Unit.Object)), Financial.Object, Suspensions.Object);
        }

        public CreateRetirementFileDto Dto(bool alive) => new()
        {
            RetireeID = 2, BranchID = 1, IsAlive = alive,
            DeathDate = alive ? null : new DateOnly(2024, 3, 4),
            IdDocument = alive ? new UploadFile() : null,
            DeathCertificate = alive ? null : new UploadFile(), PrimaryDocument = new UploadFile()
        };

        public void Dispose() => _cache.Dispose();

        private Mock<IRepository<T>> Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, List<T> rows) where T : class
        {
            var repository = new Mock<IRepository<T>>();
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            Unit.Setup(property).Returns(repository.Object);
            return repository;
        }
    }

    private sealed class UploadFile(string name = "document.png", long length = 3) : IRemoteFile
    {
        public string FileName => name;
        public string ContentType => "image/png";
        public long Length => length;
        public Stream OpenReadStream() => new MemoryStream([1, 2, 3]);
    }
}
