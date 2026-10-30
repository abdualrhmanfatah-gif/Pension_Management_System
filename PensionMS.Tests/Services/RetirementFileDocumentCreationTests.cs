using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetirementFileDocumentCreationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_UsesDocumentOwnersAndDeferredRefresh(bool alive)
    {
        using var f = new Fixture("none");
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
            It.Is<List<int>>(ids => ids.Count == 2), 1, null), Times.Once);
        f.StoredDocuments.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
        Assert.DoesNotContain("تنبيه", result.Message!);
    }

    [Theory]
    [InlineData(true, "reject")]
    [InlineData(false, "reject")]
    [InlineData(true, "throw")]
    [InlineData(false, "throw")]
    public async Task Create_UploadFailureAbortsInsteadOfReturningSuccessWithWarning(bool alive, string failure)
    {
        using var f = new Fixture(failure);
        if (failure == "throw")
            await Assert.ThrowsAsync<IOException>(() => f.Service.CreateAsync(f.Dto(alive), 1));
        else
        {
            var result = await f.Service.CreateAsync(f.Dto(alive), 1);
            Assert.False(result.IsSuccess);
            Assert.Equal("المرفق غير صالح", result.Message);
            Assert.Equal(new[] { "خطأ صادر من خدمة الوثائق" }, result.Errors);
        }
        Assert.Single(f.Uploads);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Missing.Verify(m => m.StageRequiredAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        f.Documents.Verify(d => d.RefreshAfterUploadAsync(It.IsAny<int>(), It.IsAny<List<int>>(),
            It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithoutAttachments_DoesNotRequireOrUploadDocuments()
    {
        using var f = new Fixture("none");
        var dto = f.Dto(true);
        dto.IdDocument = dto.PrimaryDocument = null;
        Assert.True((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        Assert.Empty(f.Uploads);
        f.Documents.Verify(d => d.RefreshAfterUploadAsync(It.IsAny<int>(), It.IsAny<List<int>>(),
            It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_IdentityUsesOrdinaryUploadForBackOnlyAndPreservesBothSides(bool bothSides)
    {
        using var f = new Fixture("none");
        var dto = f.Dto(true);
        var back = new UploadFile("back.png");
        dto.PrimaryDocument = null;
        dto.IdDocument = bothSides ? new UploadFile("front.png") : null;
        dto.IdDocumentBack = back;
        Assert.True((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        var upload = Assert.Single(f.Uploads);
        Assert.Equal(DocumentTypeConstants.NationalId, upload.DocumentTypeID);
        Assert.Same(dto.IdDocument ?? back, upload.File);
        Assert.Same(bothSides ? back : null, upload.FileBack);
    }

    [Theory]
    [InlineData("identity")]
    [InlineData("death")]
    [InlineData("originals")]
    public async Task Create_ProvidedEmptyAttachmentIsValidatedByDocumentService(string kind)
    {
        using var f = new Fixture("none");
        var dto = f.Dto(kind != "death");
        dto.IdDocument = dto.DeathCertificate = dto.PrimaryDocument = null;
        var empty = new UploadFile(length: 0);
        if (kind == "identity") dto.IdDocumentBack = empty;
        if (kind == "death") dto.DeathCertificate = empty;
        if (kind == "originals") dto.PrimaryDocument = empty;
        f.Documents.Setup(d => d.UploadAsync(It.Is<UploadDocumentDto>(u => u.File == empty), 1, null, true))
            .ReturnsAsync(ApiResponse<DocumentDto>.Fail("لم يتم اختيار ملف"));
        var result = await f.Service.CreateAsync(dto, 1);
        Assert.False(result.IsSuccess);
        Assert.Equal("لم يتم اختيار ملف", result.Message);
        f.Documents.Verify(d => d.UploadAsync(It.Is<UploadDocumentDto>(u => u.File == empty), 1, null, true), Times.Once);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_LaterUploadFailureRunsEarlierDocumentCompensation()
    {
        using var f = new Fixture("none");
        f.Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, true))
            .ReturnsAsync((UploadDocumentDto dto, int user, string? request, bool defer) =>
            {
                f.Uploads.Add(dto);
                if (dto.DocumentTypeID == DocumentTypeConstants.FileOriginals)
                    return ApiResponse<DocumentDto>.Fail("فشل المرفق الثاني");
                f.Unit.Object.OnRollback(() => f.Storage.Object.DeleteFileAsync("documents/new.pdf"));
                return ApiResponse<DocumentDto>.Success(new DocumentDto { DocumentID = 9 });
            });
        Assert.False((await f.Service.CreateAsync(f.Dto(true), 1)).IsSuccess);
        Assert.Equal(2, f.Uploads.Count);
        f.Storage.Verify(s => s.DeleteFileAsync("documents/new.pdf"), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_LiveNotificationFailureDoesNotTurnCommittedCreationIntoError()
    {
        using var f = new Fixture("none");
        f.Notifications.Setup(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDto>()))
            .ThrowsAsync(new IOException("تعذر الإرسال المباشر"));
        var result = await f.Service.CreateAsync(f.Dto(true), 1);
        Assert.True(result.IsSuccess, result.Message);
        f.Notifications.Verify(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Never);
        f.Notifications.Verify(n => n.EnqueueAsync(It.Is<CreateNotificationDto>(d =>
            d.UserID == 1 && d.RelatedRecordID == 7)), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_ResponseReadsFinishBeforeCommit()
    {
        using var f = new Fixture("none");
        var committed = false;
        f.Unit.Setup(u => u.CommitAsync()).Callback(() => committed = true).Returns(Task.CompletedTask);
        Mock.Get(f.Unit.Object.RetirementFiles).Setup(r => r.Query()).Returns(() =>
        {
            Assert.False(committed, "يجب تجهيز الاستجابة قبل تأكيد المعاملة");
            return new TestAsyncEnumerable<RetirementFile>(f.Files);
        });
        Assert.True((await f.Service.CreateAsync(f.Dto(true), 1)).IsSuccess);
        Assert.True(committed);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_EnqueueFailureRollsBackBeforeCommit()
    {
        using var f = new Fixture("none");
        f.Notifications.Setup(n => n.EnqueueAsync(It.IsAny<CreateNotificationDto>()))
            .ThrowsAsync(new IOException("تعذر تجهيز الإشعار"));
        await Assert.ThrowsAsync<IOException>(() => f.Service.CreateAsync(f.Dto(true), 1));
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_EnqueuesThenSavesOutboxBeforeCommit()
    {
        using var f = new Fixture("none");
        var steps = new List<string>();
        f.Notifications.Setup(n => n.EnqueueAsync(It.IsAny<CreateNotificationDto>()))
            .Callback(() => steps.Add("enqueue")).Returns(Task.CompletedTask);
        f.Unit.Setup(u => u.SaveChangesAsync()).Callback(() => steps.Add("save")).ReturnsAsync(1);
        f.Unit.Setup(u => u.CommitAsync()).Callback(() =>
        {
            Assert.Equal(new[] { "enqueue", "save" }, steps.TakeLast(2));
            steps.Add("commit");
        }).Returns(Task.CompletedTask);
        Assert.True((await f.Service.CreateAsync(f.Dto(true), 1)).IsSuccess);
        Assert.Equal("commit", steps.Last());
    }

    [Fact]
    public async Task Create_CommitExceptionDoesNotRollbackAnAlreadyClosedTransaction()
    {
        using var f = new Fixture("none");
        f.Unit.Setup(u => u.CommitAsync()).Callback(() => f.Unit.Setup(u => u.HasActiveTransaction).Returns(false))
            .ThrowsAsync(new IOException("تعذر إكمال تأكيد المعاملة"));
        await Assert.ThrowsAsync<IOException>(() => f.Service.CreateAsync(f.Dto(true), 1));
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
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

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public async Task BR01_RejectsAnyPreviousFileAfterLockWithoutDisclosure(int branch, bool deleted)
    {
        using var f = new Fixture("none");
        f.Files.Add(new RetirementFile { FileID = 999, RetireeID = 2, PensionBranchID = branch,
            StatusID = FileStatusConstants.EntitlementEnded, IsDeleted = deleted, FileNumber = "سري" });
        f.Unit.Setup(u => u.LockRetireeEntitlementsAsync(2)).Callback(() =>
            Assert.Empty(f.Unit.Invocations.Where(i => i.Method.Name == nameof(IUnitOfWork.SaveChangesAsync))));
        var response = await f.Service.CreateAsync(f.Dto(true), 1);
        Assert.False(response.IsSuccess);
        Assert.Equal("يوجد ملف سابق لهذا المتقاعد؛ لا يمكن فتح ملف آخر", response.Message);
        Assert.DoesNotContain("سري", response.Message!);
        f.Unit.Verify(u => u.LockRetireeEntitlementsAsync(2), Times.Once);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Theory]
    [InlineData("retiree-missing")]
    [InlineData("retiree-deleted")]
    [InlineData("referral-missing")]
    [InlineData("referral-deleted")]
    [InlineData("unprocessed")]
    [InlineData("enrollment-null")]
    [InlineData("enrollment-default")]
    [InlineData("retirement-null")]
    [InlineData("retirement-default")]
    [InlineData("equal")]
    [InlineData("earlier")]
    [InlineData("future")]
    public async Task BR02To05_RejectsInvalidSourceDespiteRequestDates(string scenario)
    {
        using var f = new Fixture("none");
        var approval = f.Retiree.ApprovalItem;
        switch (scenario)
        {
            case "retiree-missing": f.Retirees.Clear(); break;
            case "retiree-deleted": f.Retiree.IsDeleted = true; break;
            case "referral-missing": f.Retiree.ApprovalItem = null!; break;
            case "referral-deleted": approval.IsDeleted = true; break;
            case "unprocessed": approval.IsProcessed = false; break;
            case "enrollment-null": approval.EnrollmentDate = null; break;
            case "enrollment-default": approval.EnrollmentDate = default(DateOnly); break;
            case "retirement-null": approval.RetirementDate = null; break;
            case "retirement-default": approval.RetirementDate = default(DateOnly); break;
            case "equal": approval.RetirementDate = approval.EnrollmentDate; break;
            case "earlier": approval.RetirementDate = approval.EnrollmentDate!.Value.AddDays(-1); break;
            case "future": approval.RetirementDate = Core.Time.YemenTime.Today.AddDays(1); break;
        }
        var dto = f.Dto(true);
        dto.ServiceStartDate = new DateOnly(1990, 1, 1);
        dto.ServiceEndDate = dto.RetirementDate = new DateOnly(2021, 1, 1);
        dto.Phone = "لا يحفظ";
        var response = await f.Service.CreateAsync(dto, 1);
        Assert.False(response.IsSuccess);
        Assert.Equal("هاتف سابق", f.Retiree.Phone);
        Assert.Null(f.CreatedFile);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Financial.Verify(s => s.CreateForFileAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task BR03_SourceDatesOverrideEvenInconsistentRequestDates()
    {
        using var f = new Fixture("none");
        var dto = f.Dto(true);
        dto.ServiceStartDate = new DateOnly(2030, 1, 1);
        dto.ServiceEndDate = dto.RetirementDate = new DateOnly(1900, 1, 1);
        Assert.True((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        Assert.Equal(f.Retiree.ApprovalItem.EnrollmentDate, f.CreatedFile!.ServiceStartDate);
        Assert.Equal(f.Retiree.ApprovalItem.RetirementDate, f.CreatedFile.RetirementDate);
        Assert.Equal(f.CreatedFile.RetirementDate, f.CreatedFile.ServiceEndDate);
    }

    [Theory]
    [InlineData("no-branch")]
    [InlineData("inactive-user")]
    [InlineData("inactive-branch")]
    [InlineData("missing-branch")]
    [InlineData("other-branch")]
    [InlineData("outside-permission")]
    public async Task BR06_RejectsInvalidBranchWithoutStartingTransaction(string scenario)
    {
        using var f = new Fixture("none");
        var dto = f.Dto(true);
        if (scenario == "no-branch") f.User.BranchID = null;
        if (scenario == "inactive-user") f.User.IsActive = false;
        if (scenario == "inactive-branch") f.Branch.IsActive = false;
        if (scenario == "missing-branch") f.Branch.PensionBranchID = 99;
        if (scenario == "other-branch") dto.BranchID = 2;
        if (scenario == "outside-permission") { f.User.BranchID = f.Branch.PensionBranchID = 2; dto.BranchID = 2; }
        Assert.False((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task BR06_MissingCreatePermission_RejectsBeforeQueriesOrMutation()
    {
        using var f = new Fixture("none");
        f.Access.Setup(a => a.ForPermissionAsync("File.Create")).ThrowsAsync(new BranchScopeAccessException());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.CreateAsync(f.Dto(true), 1));
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task WF02_NonexistentRankRollsBackBeforeAttachments()
    {
        using var f = new Fixture("none");
        f.Ranks.Clear();
        Assert.False((await f.Service.CreateAsync(f.Dto(true), 1)).IsSuccess);
        Assert.Empty(f.Uploads);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task WF02_FailedFinancialPreparationDeletesOnlyTheNewStagedPhoto()
    {
        using var f = new Fixture("none");
        f.Financial.Setup(s => s.CreateForFileAsync(7, 1)).ReturnsAsync(ApiResponse<FinancialEntitlementDto>.Fail("فشل مالي"));
        var dto = f.Dto(true);
        dto.PersonalPhoto = new UploadFile("photo.jpg");
        Assert.False((await f.Service.CreateAsync(dto, 1)).IsSuccess);
        f.Storage.Verify(s => s.DeleteFileAsync("photos/new.jpg"), Times.Once);
        f.Storage.Verify(s => s.DeleteFileAsync("photos/old.jpg"), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_WithoutAttachments_UsesExistingProfileWorkflowWithoutUploading()
    {
        using var f = new Fixture("none");
        f.SeedExistingFile();
        var oldPhoto = f.Retiree.PersonalPhotoPath;
        var result = await f.Service.UpdateAsync(7, new UpdateRetirementFileDto { Notes = "تعديل" }, 1);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Empty(f.Uploads);
        Assert.Equal(oldPhoto, f.Retiree.PersonalPhotoPath);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_IdentityUsesOrdinaryUploadAndPreservesBothSides(bool bothSides)
    {
        using var f = new Fixture("none");
        f.SeedExistingFile();
        var back = new UploadFile("back.png");
        var dto = new UpdateRetirementFileDto { IdDocumentBack = back,
            IdDocument = bothSides ? new UploadFile("front.png") : null };
        var result = await f.Service.UpdateAsync(7, dto, 1);
        Assert.True(result.IsSuccess, result.Message);
        var upload = Assert.Single(f.Uploads);
        Assert.Equal(DocumentTypeConstants.NationalId, upload.DocumentTypeID);
        Assert.Same(dto.IdDocument ?? back, upload.File);
        Assert.Same(bothSides ? back : null, upload.FileBack);
        Assert.Equal(2, upload.RetireeID);
        Assert.Equal(7, upload.RetirementFileID);
        f.Documents.Verify(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, false), Times.Once);
    }

    [Theory]
    [InlineData("reject")]
    [InlineData("throw")]
    public async Task Update_UploadFailureAbortsAndDoesNotReturnSuccessWithErrors(string failure)
    {
        using var f = new Fixture(failure);
        f.SeedExistingFile();
        var dto = new UpdateRetirementFileDto { IdDocument = new UploadFile(), DeathCertificate = new UploadFile() };
        if (failure == "throw")
            await Assert.ThrowsAsync<IOException>(() => f.Service.UpdateAsync(7, dto, 1));
        else
        {
            var result = await f.Service.UpdateAsync(7, dto, 1);
            Assert.False(result.IsSuccess);
            Assert.Equal("المرفق غير صالح", result.Message);
            Assert.Equal(new[] { "خطأ صادر من خدمة الوثائق" }, result.Errors);
        }
        Assert.Single(f.Uploads);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Review.Verify(r => r.RequestReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Theory]
    [InlineData("identity")]
    [InlineData("death")]
    public async Task Update_ProvidedEmptyAttachmentIsNotSilentlySkipped(string kind)
    {
        using var f = new Fixture("none");
        f.SeedExistingFile();
        var empty = new UploadFile(length: 0);
        var dto = new UpdateRetirementFileDto
        {
            IdDocumentBack = kind == "identity" ? empty : null,
            DeathCertificate = kind == "death" ? empty : null
        };
        f.Documents.Setup(d => d.UploadAsync(It.Is<UploadDocumentDto>(u => u.File == empty), 1, null, false))
            .ReturnsAsync(ApiResponse<DocumentDto>.Fail("لم يتم اختيار ملف"));
        var result = await f.Service.UpdateAsync(7, dto, 1);
        Assert.False(result.IsSuccess);
        Assert.Equal("لم يتم اختيار ملف", result.Message);
        f.Documents.Verify(d => d.UploadAsync(It.Is<UploadDocumentDto>(u => u.File == empty), 1, null, false), Times.Once);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_LaterUploadFailureRunsEarlierDocumentCompensation()
    {
        using var f = new Fixture("none");
        f.SeedExistingFile();
        f.Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, false))
            .ReturnsAsync((UploadDocumentDto dto, int user, string? request, bool defer) =>
            {
                f.Uploads.Add(dto);
                if (dto.DocumentTypeID == DocumentTypeConstants.DeathCertificate)
                    return ApiResponse<DocumentDto>.Fail("فشل المرفق الثاني");
                f.Unit.Object.OnRollback(() => f.Storage.Object.DeleteFileAsync("documents/new.pdf"));
                return ApiResponse<DocumentDto>.Success(new DocumentDto { DocumentID = 9 });
            });
        var result = await f.Service.UpdateAsync(7, new UpdateRetirementFileDto
            { IdDocument = new UploadFile(), DeathCertificate = new UploadFile() }, 1);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, f.Uploads.Count);
        f.Storage.Verify(s => s.DeleteFileAsync("documents/new.pdf"), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_ForeignFileRejectsBeforeUploading()
    {
        using var f = new Fixture("none");
        f.SeedExistingFile().PensionBranchID = 2;
        Assert.False((await f.Service.UpdateAsync(7, new UpdateRetirementFileDto { IdDocument = new UploadFile() }, 1)).IsSuccess);
        Assert.Empty(f.Uploads);
    }

    [Fact]
    public async Task Update_PhotoGoesThroughRetireeServiceOnly()
    {
        using var f = new Fixture("none");
        f.SeedExistingFile();
        Assert.True((await f.Service.UpdateAsync(7, new UpdateRetirementFileDto { PersonalPhoto = new UploadFile("photo.jpg") }, 1)).IsSuccess);
        Assert.Empty(f.Uploads);
        Assert.Equal("photos/new.jpg", f.Retiree.PersonalPhotoPath);
        f.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), "photo.jpg", "photos",
            It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Once);
    }

    private sealed class Fixture : IDisposable
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IMissingDocumentService> Missing { get; } = new();
        public Mock<IDocumentService> Documents { get; } = new();
        public Mock<IFinancialEntitlementService> Financial { get; } = new();
        public Mock<IPensionSuspensionService> Suspensions { get; } = new();
        public Mock<IFileReviewService> Review { get; } = new();
        public Mock<INotificationService> Notifications { get; } = new();
        public RetirementFile? CreatedFile { get; private set; }
        public Mock<IRepository<Document>> StoredDocuments { get; }
        public List<UploadDocumentDto> Uploads { get; } = [];
        public Retiree Retiree { get; } = new() { RetireeID = 2, FullName = "متقاعد",
            NationalID = "رقم سابق", Phone = "هاتف سابق", PersonalPhotoPath = "photos/old.jpg",
            ApprovalItem = new MinistryApprovalItem { IsProcessed = true,
                EnrollmentDate = new DateOnly(1980, 1, 1), RetirementDate = new DateOnly(2020, 1, 1) } };
        public List<RetirementFile> Files { get; } = [];
        public List<Retiree> Retirees { get; } = [];
        public List<Rank> Ranks { get; } = [new Rank { RankID = 77 }];
        public User User { get; } = new() { UserID = 1, BranchID = 1, IsActive = true };
        public PensionFundBranch Branch { get; } = new() { PensionBranchID = 1, IsActive = true };
        public Mock<IAccessiblePensionRepository> Access { get; } = new();
        public Mock<IFileStorageService> Storage { get; } = new();
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        public RetirementFileService Service { get; }

        public Fixture(string failure)
        {
            var compensations = new List<Func<Task>>();
            Unit.Setup(u => u.OnRollback(It.IsAny<Func<Task>>())).Callback<Func<Task>>(compensations.Add);
            Unit.Setup(u => u.RollbackAsync()).Returns(async () =>
            {
                foreach (var compensation in compensations.AsEnumerable().Reverse()) await compensation();
                compensations.Clear();
            });
            var retiree = Retiree;
            var files = Bind<RetirementFile>(u => u.RetirementFiles, []);
            var stored = Files;
            files.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<RetirementFile>(stored));
            files.Setup(r => r.AddAsync(It.IsAny<RetirementFile>())).Callback<RetirementFile>(file =>
            {
                file.FileID = 7;
                file.Retiree = retiree;
                CreatedFile = file;
                stored.Add(file);
            }).Returns(Task.CompletedTask);
            Retirees.Add(retiree);
            Bind(u => u.Retirees, Retirees).Setup(r => r.GetByIdAsync(2)).ReturnsAsync(retiree);
            Bind(u => u.Users, [User]);
            Bind(u => u.PensionFundBranches, [Branch]);
            Bind(u => u.Ranks, Ranks);
            Bind<SettlementItem>(u => u.SettlementItems, []);
            Bind<EntitledPerson>(u => u.EntitledPersons, []);
            Bind<Agent>(u => u.Agents, []);
            Bind<Guardian>(u => u.Guardians, []);
            Bind<PaymentBook>(u => u.PaymentBooks, []);
            Bind<AuditLog>(u => u.AuditLogs, []);
            Missing.Setup(m => m.GetByFileAsync(7)).ReturnsAsync(ApiResponse<List<Core.DTOs.MissingDocuments.MissingDocumentDto>>.Success([]));
            Missing.Setup(m => m.GetAutoDetectedByFileAsync(7)).ReturnsAsync(ApiResponse<List<Core.DTOs.MissingDocuments.AutoDetectedMissingDocumentDto>>.Success([]));
            Review.Setup(r => r.RequestReviewAsync(7, 1, It.IsAny<DateTime?>())).ReturnsAsync(ApiResponse<bool>.Success(true));
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
            Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 1, null, It.IsAny<bool>()))
                .ReturnsAsync((UploadDocumentDto dto, int user, string? request, bool defer) =>
                {
                    Uploads.Add(dto);
                    if (dto.DocumentTypeID != DocumentTypeConstants.FileOriginals && failure == "throw")
                        throw new IOException("تعذر رفع المرفق");
                    if (dto.DocumentTypeID != DocumentTypeConstants.FileOriginals && failure == "reject")
                        return ApiResponse<DocumentDto>.Fail("المرفق غير صالح", ["خطأ صادر من خدمة الوثائق"]);
                    return ApiResponse<DocumentDto>.Success(new DocumentDto { DocumentID = Uploads.Count });
                });
            Documents.Setup(d => d.RefreshAfterUploadAsync(7, It.IsAny<List<int>>(), 1, null))
                .ReturnsAsync(ApiResponse<RefreshAfterUploadResult>.Success(new()));
            var identity = new Mock<ITestBranchScope>();
            identity.Setup(i => i.GetCurrentBranchId()).Returns(1);
            var access = TestPensionAccess.From(Unit.Object, identity.Object);
            Access.Setup(a => a.ForPermissionAsync(It.IsAny<string>())).Returns((string p) => access.ForPermissionAsync(p));
            Access.Setup(a => a.ForCurrentOperationAsync()).Returns(() => access.ForCurrentOperationAsync());
            Service = new RetirementFileService(Unit.Object, Mock.Of<IImmutableAuditService>(),
                Missing.Object, Documents.Object,
                Access.Object, Mock.Of<IEntitledPersonService>(),
                Notifications.Object, Review.Object, NullLogger<RetirementFileService>.Instance,
                new RetireeService(Unit.Object, Storage.Object, Documents.Object,
                    Missing.Object, NullLogger<RetireeService>.Instance,
                    _cache, Access.Object), Financial.Object, Suspensions.Object);
        }

        public CreateRetirementFileDto Dto(bool alive) => new()
        {
            RetireeID = 2, BranchID = 1, IsAlive = alive,
            DeathDate = alive ? null : new DateOnly(2024, 3, 4),
            IdDocument = alive ? new UploadFile() : null,
            DeathCertificate = alive ? null : new UploadFile(), PrimaryDocument = new UploadFile()
        };

        public RetirementFile SeedExistingFile()
        {
            var file = new RetirementFile { FileID = 7, RetireeID = 2, Retiree = Retiree,
                PensionBranchID = 1, StatusID = FileStatusConstants.New, AssignedEmployeeID = 1 };
            Files.Add(file);
            return file;
        }

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
