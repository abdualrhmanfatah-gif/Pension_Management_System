using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetireeProfileUpdateTests
{
    [Fact]
    public async Task FileOpening_RequiresCallersTransaction()
    {
        using var f = new Fixture();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Retirees.StageFileOpeningAsync(2,
            new UpdateRetireeDto(), false, new DateOnly(2024, 3, 4), 7));
        Assert.True(f.Retiree.IsAlive);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_RejectsExistingOrSoftDeletedFileBeforeChangingLife(bool deleted)
    {
        using var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.File.IsDeleted = deleted;
        var result = await f.Retirees.StageFileOpeningAsync(2,
            new UpdateRetireeDto { FullName = "معدل", PersonalPhoto = new Photo() }, false, new DateOnly(2024, 3, 4), 7);

        Assert.False(result.IsSuccess);
        Assert.True(f.Retiree.IsAlive);
        Assert.Equal("متقاعد", f.Retiree.FullName);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_StagesInitialLifeWithSharedFieldsWithoutSaving(bool alive)
    {
        using var f = new Fixture();
        f.Files.Clear();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Retiree.IsAlive = !alive;
        f.Retiree.DeathDate = new DateOnly(2020, 1, 1);
        var deathDate = new DateOnly(2024, 3, 4);
        var result = await f.Retirees.StageFileOpeningAsync(2, new UpdateRetireeDto
        {
            BirthDate = new DateOnly(1950, 1, 1), Phone = "جديد", PersonalPhoto = new Photo(),
            HealthStatusID = alive ? HealthStatusConstants.Healthy : HealthStatusConstants.Deceased
        }, alive, deathDate, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal(alive, f.Retiree.IsAlive);
        Assert.Equal(alive ? (DateOnly?)null : deathDate, f.Retiree.DeathDate);
        Assert.Equal(new DateOnly(1950, 1, 1), f.Retiree.BirthDate);
        Assert.Equal("جديد", f.Retiree.Phone);
        Assert.Equal("photos/new.jpg", f.Retiree.PersonalPhotoPath);
        Assert.Equal(7, f.Retiree.UpdatedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Storage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_RejectsConflictingHealthBeforeChangingLife(bool inherited)
    {
        using var f = new Fixture();
        f.Files.Clear();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        if (inherited) f.Retiree.HealthStatusID = HealthStatusConstants.Healthy;
        var result = await f.Retirees.StageFileOpeningAsync(2,
            new UpdateRetireeDto { HealthStatusID = inherited ? null : HealthStatusConstants.Healthy }, false, new DateOnly(2024, 3, 4), 7);
        Assert.False(result.IsSuccess);
        Assert.True(f.Retiree.IsAlive);
        Assert.Null(f.Retiree.DeathDate);
    }

    [Fact]
    public async Task StageProfileUpdate_UpdatesCalendarDateAndActorWithoutSavingOrRefreshing()
    {
        using var f = new Fixture();
        var before = DateTime.UtcNow;
        var result = await f.Retirees.StageProfileUpdateAsync(2, new UpdateRetireeDto
        {
            FullName = "اسم معدل", NationalID = "123456", BirthDate = new DateOnly(1960, 5, 6),
            MilitaryNumber = "789", LinkNumber = "42", Gender = 2, Phone = "777000000",
            Address = "عنوان", City = "عدن", Governorate = "عدن", SocialStatusID = 2,
            HealthStatusID = HealthStatusConstants.Healthy
        }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal("اسم معدل", f.Retiree.FullName);
        Assert.Equal("123456", f.Retiree.NationalID);
        Assert.Equal(new DateOnly(1960, 5, 6), f.Retiree.BirthDate);
        Assert.Equal("789", f.Retiree.MilitaryNumber);
        Assert.Equal("42", f.Retiree.LinkNumber);
        Assert.Equal(2, f.Retiree.Gender);
        Assert.Equal("777000000", f.Retiree.Phone);
        Assert.Equal("عنوان", f.Retiree.Address);
        Assert.Equal("عدن", f.Retiree.City);
        Assert.Equal("عدن", f.Retiree.Governorate);
        Assert.Equal(2, f.Retiree.SocialStatusID);
        Assert.Equal(7, f.Retiree.UpdatedByUserID);
        Assert.True(f.Retiree.UpdatedAt >= before);
        Assert.Equal(DateTimeKind.Utc, f.Retiree.UpdatedAt!.Value.Kind);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.Retirees.Update(It.IsAny<Retiree>()), Times.Never);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(It.IsAny<int>()), Times.Never);
        f.Review.Verify(r => r.RequestReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task StageProfileUpdate_EmptyIdentityFieldsPreserveExistingValues(string? value)
    {
        using var f = new Fixture();
        var result = await f.Retirees.StageProfileUpdateAsync(2,
            new UpdateRetireeDto { FullName = value, NationalID = value }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal("متقاعد", f.Retiree.FullName);
        Assert.Equal("original-id", f.Retiree.NationalID);
        Assert.Equal(new DateOnly(1961, 1, 2), f.Retiree.BirthDate);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
    }

    [Theory]
    [InlineData("stage")]
    [InlineData("retiree")]
    [InlineData("photo")]
    [InlineData("file")]
    public async Task PhotoRoutes_RejectInvalidPhotoBeforeChangingAnyFields(string route)
    {
        using var f = new Fixture();
        Assert.False(await f.EditAsync(route, new Photo("bad.pdf")));
        Assert.Equal("متقاعد", f.Retiree.FullName);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
        Assert.Null(f.File.Notes);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData("photo.jpg", 0)]
    [InlineData("photo.jpg", 2097153)]
    [InlineData("photo.svg", 8)]
    [InlineData("photo", 8)]
    public async Task PhotoPolicy_RejectsEmptyOversizedOrUnsupportedFile(string name, long length)
    {
        using var f = new Fixture();
        var result = await f.Retirees.StageProfileUpdateAsync(2,
            new UpdateRetireeDto { PersonalPhoto = new Photo(name, length) }, 7);
        Assert.False(result.IsSuccess);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Never);
    }

    [Theory]
    [InlineData("retiree")]
    [InlineData("photo")]
    [InlineData("file")]
    public async Task PhotoRoutes_UseSameStoragePolicyRetainOldPhotoAndRefreshLinkedFiles(string route)
    {
        using var f = new Fixture();
        f.Files.Add(new RetirementFile { FileID = 3, RetireeID = 2 });
        f.Files.Add(new RetirementFile { FileID = 4, RetireeID = 2, IsDeleted = true });
        f.Files.Add(new RetirementFile { FileID = 5, RetireeID = 99 });
        foreach (var key in new[] { "retiree_2", "retirement_file_1", "retirement_file_3", "retirement_file_4", "retirement_file_5" })
            f.Cache.Set(key, "cached");

        Assert.True(await f.EditAsync(route, new Photo("photo.JPEG", 2097152)));

        Assert.Equal("photos/new.jpg", f.Retiree.PersonalPhotoPath);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), "photo.JPEG", "photos",
            It.Is<string[]?>(extensions => extensions != null && extensions.SequenceEqual(new[] { ".jpg", ".jpeg", ".png" })),
            2097152L), Times.Once);
        f.Storage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        Assert.False(f.Cache.TryGetValue("retiree_2", out _));
        Assert.False(f.Cache.TryGetValue("retirement_file_1", out _));
        Assert.False(f.Cache.TryGetValue("retirement_file_3", out _));
        Assert.True(f.Cache.TryGetValue("retirement_file_4", out _));
        Assert.True(f.Cache.TryGetValue("retirement_file_5", out _));
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(1), Times.Once);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(3), Times.Once);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(4), Times.Never);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(5), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
        if (route == "file")
        {
            Assert.Equal(new DateOnly(1965, 4, 3), f.Retiree.BirthDate);
            Assert.Equal("تعديل الملف", f.File.Notes);
            Assert.Equal(7, f.Retiree.UpdatedByUserID);
            f.Review.Verify(r => r.RequestReviewAsync(1, 7, It.IsAny<DateTime?>()), Times.Once);
            f.Unit.Verify(u => u.RetirementFiles.Update(It.IsAny<RetirementFile>()), Times.Never);
        }
    }

    [Theory]
    [InlineData("retiree")]
    [InlineData("photo")]
    [InlineData("file")]
    public async Task PhotoRoutes_StorageFailurePreservesProfileAndDoesNotCommit(string route)
    {
        using var f = new Fixture();
        f.Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "photos",
            It.IsAny<string[]?>(), It.IsAny<long?>())).ThrowsAsync(new IOException("تعذر التخزين"));

        Assert.False(await f.EditAsync(route, new Photo()));
        Assert.Equal("متقاعد", f.Retiree.FullName);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
        Assert.Null(f.File.Notes);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Theory]
    [InlineData("retiree")]
    [InlineData("photo")]
    [InlineData("file")]
    public async Task PhotoRoutes_DenySharedRetireeAcrossUnauthorizedBranchBeforeStorage(string route)
    {
        using var f = new Fixture(restricted: true);
        f.Files.Add(new RetirementFile { FileID = 3, RetireeID = 2, PensionBranchID = 99 });

        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.EditAsync(route, new Photo()));
        Assert.Equal("متقاعد", f.Retiree.FullName);
        Assert.Equal("photos/old.jpg", f.Retiree.PersonalPhotoPath);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task StageProfileUpdate_RejectsSoftDeletedRetiree()
    {
        using var f = new Fixture();
        f.Retiree.IsDeleted = true;
        Assert.False((await f.Retirees.StageProfileUpdateAsync(2, new UpdateRetireeDto { FullName = "معدل" }, 7)).IsSuccess);
        Assert.Equal("متقاعد", f.Retiree.FullName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_ReportsIdentityUploadFailureWhileSavingProfile(bool throws)
    {
        using var f = new Fixture();
        var upload = f.Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 7, null, false));
        if (throws) upload.ThrowsAsync(new IOException("تعذر الرفع"));
        else upload.ReturnsAsync(ApiResponse<DocumentDto>.Fail("الوثيقة مرفوضة"));
        var photo = new Photo();
        var result = await f.Retirees.UpdateAsync(2,
            new UpdateRetireeDto { FullName = "معدل", IdDocumentBack = photo }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal("معدل", result.Data!.FullName);
        Assert.NotEmpty(result.Errors!);
        f.Documents.Verify(d => d.UploadAsync(It.Is<UploadDocumentDto>(dto => dto.RetireeID == 2 &&
            dto.DocumentTypeID == DocumentTypeConstants.NationalId && dto.File == photo && dto.FileBack == null), 7, null, false), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task FileUpdate_ReviewFailureRollsBackAndSkipsRefresh()
    {
        using var f = new Fixture();
        f.Review.Setup(r => r.RequestReviewAsync(1, 7, It.IsAny<DateTime?>()))
            .ReturnsAsync(ApiResponse<bool>.Fail("تعذر طلب المراجعة"));

        Assert.False(await f.EditAsync("file", new Photo()));
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(It.IsAny<int>()), Times.Never);
        f.Storage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("retiree")]
    [InlineData("photo")]
    [InlineData("file")]
    public async Task PhotoRoutes_NestedOperationLeavesTransactionOwnershipToCaller(string route)
    {
        using var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);

        Assert.True(await f.EditAsync(route, new Photo()));
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly RetirementLifecycleFixture _lifecycle = new();
        public Mock<IUnitOfWork> Unit => _lifecycle.Unit;
        public RetirementFile File => _lifecycle.File;
        public Retiree Retiree => File.Retiree;
        public List<RetirementFile> Files { get; } = [];
        public Mock<IFileStorageService> Storage { get; } = new();
        public Mock<IDocumentService> Documents { get; } = new();
        public Mock<IMissingDocumentService> Compliance { get; } = new();
        public Mock<IFileReviewService> Review { get; } = new();
        public MemoryCache Cache { get; } = new(new MemoryCacheOptions());
        public RetireeService Retirees { get; }
        private readonly RetirementFileService _files;

        public Fixture(bool restricted = false)
        {
            Files.Add(File);
            File.PensionBranchID = 1;
            File.AssignedEmployeeID = 7;
            Retiree.NationalID = "original-id";
            Retiree.BirthDate = new DateOnly(1961, 1, 2);
            Retiree.PersonalPhotoPath = "photos/old.jpg";
            Mock.Get(Unit.Object.RetirementFiles).Setup(r => r.Query())
                .Returns(() => new TestAsyncEnumerable<RetirementFile>(Files));
            Mock.Get(Unit.Object.Retirees).Setup(r => r.GetByIdAsync(2)).ReturnsAsync(Retiree);
            Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "photos",
                It.IsAny<string[]?>(), It.IsAny<long?>())).ReturnsAsync("photos/new.jpg");
            Review.Setup(r => r.RequestReviewAsync(1, 7, It.IsAny<DateTime?>()))
                .ReturnsAsync(ApiResponse<bool>.Success(true));
            var scope = new Mock<ITestBranchScope>();
            scope.Setup(s => s.IsHQ()).Returns(!restricted);
            scope.Setup(s => s.GetCurrentBranchId()).Returns(1);
            var access = TestPensionAccess.From(Unit.Object, scope.Object);
            var suspensions = new Mock<IPensionSuspensionService>();
            suspensions.Setup(s => s.GetSalaryStatusAsync(1)).ReturnsAsync(ApiResponse<string>.Success("موقوف"));
            Documents.Setup(d => d.GetRetireeIdentityPathAsync(1)).ReturnsAsync(ApiResponse<string?>.Success(null));
            Retirees = new RetireeService(Unit.Object, Storage.Object, Documents.Object, Compliance.Object,
                NullLogger<RetireeService>.Instance, Cache, access);
            var missing = Compliance;
            missing.Setup(m => m.GetByFileAsync(1)).ReturnsAsync(ApiResponse<List<MissingDocumentDto>>.Success([]));
            missing.Setup(m => m.GetAutoDetectedByFileAsync(1)).ReturnsAsync(ApiResponse<List<AutoDetectedMissingDocumentDto>>.Success([]));
            _files = new RetirementFileService(Unit.Object, Mock.Of<IImmutableAuditService>(),
                missing.Object, Documents.Object, access, Mock.Of<IEntitledPersonService>(),
                Mock.Of<INotificationService>(), Review.Object, NullLogger<RetirementFileService>.Instance, Retirees,
                Mock.Of<IFinancialEntitlementService>(), suspensions.Object);
        }

        public async Task<bool> EditAsync(string route, IRemoteFile photo) => route switch
        {
            "stage" => (await Retirees.StageProfileUpdateAsync(2, new UpdateRetireeDto { FullName = "معدل", PersonalPhoto = photo }, 7)).IsSuccess,
            "retiree" => (await Retirees.UpdateAsync(2, new UpdateRetireeDto { FullName = "معدل", PersonalPhoto = photo }, 7)).IsSuccess,
            "photo" => (await Retirees.UploadPhotoAsync(2, photo)).IsSuccess,
            "file" => (await _files.UpdateAsync(1, new UpdateRetirementFileDto
                { FullName = "معدل", PersonalPhoto = photo, BirthDate = new DateOnly(1965, 4, 3), Notes = "تعديل الملف" }, 7)).IsSuccess,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

        public void Dispose() => Cache.Dispose();
    }

    private sealed class Photo(string name = "photo.jpg", long length = 8) : IRemoteFile
    {
        public string FileName => name;
        public string ContentType => "image/jpeg";
        public long Length => length;
        public Stream OpenReadStream() => new MemoryStream([1, 2, 3]);
    }
}
