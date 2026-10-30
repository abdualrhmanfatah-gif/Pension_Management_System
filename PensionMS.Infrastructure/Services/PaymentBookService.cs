using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.PaymentBooks;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class PaymentBookService : IPaymentBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IFileReviewService _reviewService;

    public PaymentBookService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, IFileReviewService reviewService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _reviewService = reviewService;
    }

    public async Task<ApiResponse<PagedResult<PaymentBookReviewDto>>> GetReviewAsync(PaymentBookReviewQueryDto query)
    {
        var q = _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Include(pb => pb.IssuingBranch)
            .Include(pb => pb.RetirementFile)
                .ThenInclude(rf => rf.Retiree)
            .Where(pb => !pb.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(pb => pb.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.BookType))
            q = q.Where(pb => pb.BookType == query.BookType);

        if (query.BranchId.HasValue)
            q = q.Where(pb => pb.IssuingBranchID == query.BranchId.Value);

        if (query.ExpiringWithinDays.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(query.ExpiringWithinDays.Value);
            q = q.Where(pb => pb.ExpiryDate <= cutoff && pb.Status == PaymentBookStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(pb =>
                (pb.BookNumber != null && pb.BookNumber.ToLower().Contains(search)) ||
                (pb.RecipientName != null && pb.RecipientName.ToLower().Contains(search)) ||
                (pb.RetirementFile != null && pb.RetirementFile.FileNumber != null && pb.RetirementFile.FileNumber.ToLower().Contains(search)) ||
                (pb.RetirementFile != null && pb.RetirementFile.Retiree != null && pb.RetirementFile.Retiree.FullName.ToLower().Contains(search)) ||
                (pb.RetirementFile != null && pb.RetirementFile.Retiree != null && pb.RetirementFile.Retiree.NationalID != null && pb.RetirementFile.Retiree.NationalID.Contains(search)) ||
                (pb.RetirementFile != null && pb.RetirementFile.Retiree != null && pb.RetirementFile.Retiree.MilitaryNumber != null && pb.RetirementFile.Retiree.MilitaryNumber.Contains(search)) ||
                (pb.RetirementFile != null && pb.RetirementFile.Retiree != null && pb.RetirementFile.Retiree.Phone != null && pb.RetirementFile.Retiree.Phone.Contains(search))
            );
        }

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(pb => pb.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // Fetch book images for all items in batch
        var fileIds = items.Where(pb => pb.RetirementFile != null).Select(pb => pb.RetirementFile!.FileID).Distinct().ToList();
        var bookImages = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => fileIds.Contains(d.FileID!.Value)
                     && d.DocumentTypeID == DocumentTypeConstants.PaymentBookImage
                     && !d.IsDeleted)
            .GroupBy(d => d.FileID)
            .Select(g => new { FileID = g.Key, Images = g.OrderByDescending(d => d.CreatedAt).Select(d => d.FilePath).ToList() })
            .ToDictionaryAsync(x => x.FileID, x => x.Images);

        var result = items.Select(pb => new PaymentBookReviewDto
        {
            PaymentBookID = pb.PaymentBookID,
            BookNumber = pb.BookNumber,
            RecipientName = pb.RecipientName,
            RecipientType = pb.RecipientType,
            FileNumber = pb.RetirementFile?.FileNumber,
            RetireeName = pb.RetirementFile?.Retiree?.FullName,
            RetireeId = pb.RetirementFile?.RetireeID,
            NationalID = pb.RetirementFile?.Retiree?.NationalID,
            MilitaryNumber = pb.RetirementFile?.Retiree?.MilitaryNumber,
            Phone = pb.RetirementFile?.Retiree?.Phone,
            BookType = pb.BookType,
            LegacyColor = pb.LegacyColor,
            IssueDate = pb.IssueDate,
            ExpiryDate = pb.ExpiryDate,
            Status = pb.Status,
            IssuingBranchName = pb.IssuingBranch?.BranchName,
            ChangeReason = pb.ChangeReason,
            DaysUntilExpiry = pb.Status == PaymentBookStatus.Active
                ? (int?)(pb.ExpiryDate - DateTime.UtcNow).TotalDays
                : null,
            IsExpiringSoon = pb.Status == PaymentBookStatus.Active
                && pb.ExpiryDate <= DateTime.UtcNow.AddDays(30),
            BookImagePaths = pb.RetirementFile != null
                && bookImages.TryGetValue(pb.RetirementFile.FileID, out var paths) ? paths : new List<string>()
        }).ToList();

        return ApiResponse<PagedResult<PaymentBookReviewDto>>.Success(new PagedResult<PaymentBookReviewDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<PaymentBookStatsDto>> GetStatsAsync()
    {
        var books = _unitOfWork.PaymentBooks.Query().AsNoTracking().Where(pb => !pb.IsDeleted);

        var stats = new PaymentBookStatsDto
        {
            TotalActive = await books.CountAsync(pb => pb.Status == PaymentBookStatus.Active),
            TotalExpired = await books.CountAsync(pb => pb.Status == PaymentBookStatus.Expired),
            TotalLost = await books.CountAsync(pb => pb.Status == PaymentBookStatus.Lost),
            TotalDamaged = await books.CountAsync(pb => pb.Status == PaymentBookStatus.Damaged),
            ExpiringWithin30Days = await books.CountAsync(pb =>
                pb.Status == PaymentBookStatus.Active
                && pb.ExpiryDate <= DateTime.UtcNow.AddDays(30))
        };

        return ApiResponse<PaymentBookStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<List<PaymentBookDto>>> GetByFileIdAsync(int fileId)
    {
        var books = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Include(pb => pb.IssuingBranch)
            .Where(pb => pb.FileID == fileId && !pb.IsDeleted)
            .OrderByDescending(pb => pb.CreatedAt)
            .Select(pb => MapToDto(pb))
            .ToListAsync();

        return ApiResponse<List<PaymentBookDto>>.Success(books);
    }

    public async Task<ApiResponse<PaymentBookDto>> GetByIdAsync(int id)
    {
        var book = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Include(pb => pb.IssuingBranch)
            .FirstOrDefaultAsync(pb => pb.PaymentBookID == id && !pb.IsDeleted);
        if (book == null)
            return ApiResponse<PaymentBookDto>.Fail("دفتر الصرف غير موجود");

        return ApiResponse<PaymentBookDto>.Success(MapToDto(book));
    }

    public async Task<ApiResponse<PaymentBookDto?>> GetActiveByFileIdAsync(int fileId)
    {
        var book = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Include(pb => pb.IssuingBranch)
            .Where(pb => pb.FileID == fileId && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        return ApiResponse<PaymentBookDto?>.Success(book != null ? MapToDto(book) : null);
    }

    public async Task<ApiResponse<bool>> CheckBookNumberExistsAsync(string bookNumber)
    {
        if (string.IsNullOrWhiteSpace(bookNumber))
            return ApiResponse<bool>.Success(false);

        var exists = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .AnyAsync(pb => pb.BookNumber == bookNumber && !pb.IsDeleted);

        return ApiResponse<bool>.Success(exists);
    }

    public async Task<ApiResponse<PaymentBookDto>> CreateAsync(CreatePaymentBookDto dto, int userId)
    {
        // Validate file exists
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<PaymentBookDto>.Fail("الملف غير موجود");

        var isLegacy = dto.BookType == "يدوي";

        PaymentBook book;

        if (isLegacy)
        {
            // ── دفتر قديم (يدوي) ──
            book = new PaymentBook
            {
                FileID = dto.FileID,
                BookNumber = dto.BookNumber,
                IssueDate = dto.IssueDate,
                Status = PaymentBookStatus.Expired, // لا يكون فعالاً
                BookType = "يدوي",
                LegacyColor = dto.LegacyColor,
                RecipientName = dto.RecipientName,
                RecipientType = dto.RecipientType,
                Notes = dto.Notes,
                IssuingBranchID = dto.IssuingBranchID,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserID = userId
            };
        }
        else
        {
            // ── دفتر إلكتروني (جديد) ──
            var previousStatus = GetPreviousStatus(dto.ChangeReason);

            // Deactivate the current active book
            var currentActive = await _unitOfWork.PaymentBooks.Query()
                .Where(pb => pb.FileID == dto.FileID && pb.Status == PaymentBookStatus.Active)
                .FirstOrDefaultAsync();

            if (currentActive != null)
            {
                currentActive.Status = previousStatus;
                currentActive.UpdatedAt = DateTime.UtcNow;
                currentActive.UpdatedByUserID = userId;
                _unitOfWork.PaymentBooks.Update(currentActive);
            }

            book = new PaymentBook
            {
                FileID = dto.FileID,
                BookNumber = await GenerateNextBookNumberAsync(),
                IssueDate = dto.IssueDate,
                ExpiryDate = dto.ExpiryDate ?? dto.IssueDate.AddYears(1),
                Status = PaymentBookStatus.Active,
                BookType = "إلكتروني",
                ChangeReason = dto.ChangeReason,
                RecipientName = dto.RecipientName,
                RecipientType = dto.RecipientType,
                Notes = dto.Notes,
                IssuingBranchID = dto.IssuingBranchID,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserID = userId
            };
        }

        await _unitOfWork.PaymentBooks.AddAsync(book);

        // Handle auto-creating SalaryRecipient for electronic books
        if (!isLegacy && dto.AutoCreateSalaryRecipient &&
            (dto.RetireeID.HasValue || dto.AgentID.HasValue || dto.EntitledID.HasValue || dto.GuardianID.HasValue))
        {
            var activeRecipients = await _unitOfWork.SalaryRecipients.Query()
                .Where(s => s.FileID == dto.FileID && s.IsActive)
                .ToListAsync();

            foreach (var r in activeRecipients)
            {
                r.IsActive = false;
                r.EndDate = dto.IssueDate;
                r.UpdatedAt = DateTime.UtcNow;
                r.UpdatedByUserID = userId;
                r.Notes = "تم إيقافه آلياً بسبب إصدار دفتر صرف جديد";
                _unitOfWork.SalaryRecipients.Update(r);
            }

            var salaryRecipientType = dto.RetireeID.HasValue ? "متقاعد" :
                                      dto.AgentID.HasValue ? "وكيل" :
                                      dto.EntitledID.HasValue ? "مستحق" :
                                      dto.GuardianID.HasValue ? "وصي" : "";

            var newRecipient = new SalaryRecipient
            {
                FileID = dto.FileID,
                RetireeID = dto.RetireeID,
                AgentID = dto.AgentID,
                EntitledID = dto.EntitledID,
                GuardianID = dto.GuardianID,
                RecipientType = salaryRecipientType,
                StartDate = dto.IssueDate,
                IsActive = true,
                Notes = $"مستلم المعاش آلي عبر دفتر الصرف رقم {book.BookNumber}",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserID = userId
            };
            await _unitOfWork.SalaryRecipients.AddAsync(newRecipient);
        }

        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        // Upload book image if provided
        if (dto.BookImage != null)
        {
            var filePath = await SaveFileAsync(dto.BookImage, "documents");
            var doc = new Document
            {
                FileID = dto.FileID,
                DocumentTypeID = DocumentTypeConstants.PaymentBookImage,
                OwnerType = DocumentOwnerType.RetirementFile,
                FilePath = filePath,
                FileName = dto.BookImage.FileName,
                FileSize = dto.BookImage.Length,
                MimeType = dto.BookImage.ContentType,
                UploadedByUserID = userId,
                Verified = false,
                Notes = $"صورة دفتر الصرف رقم {book.BookNumber}"
            };
            await _unitOfWork.Documents.AddAsync(doc);
            await _unitOfWork.SaveChangesAsync();

            // Auto-fulfill any matching MissingDocument for PaymentBookImage
            var missingDocs = await _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == dto.FileID
                         && m.DocumentTypeID == DocumentTypeConstants.PaymentBookImage
                         && (m.Status == MissingDocumentStatusConstants.Pending
                          || m.Status == MissingDocumentStatusConstants.Pledged))
                .ToListAsync();

            foreach (var md in missingDocs)
            {
                md.Status = MissingDocumentStatusConstants.Fulfilled;
                md.SubmittedAt = DateTime.UtcNow;
                _unitOfWork.MissingDocuments.Update(md);
            }

            if (missingDocs.Count > 0)
                await _unitOfWork.SaveChangesAsync();
        }
        else if (isLegacy)
        {
            // For legacy (manual) books only: register missing document if no image provided
            var existingMissing = await _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == dto.FileID
                         && m.DocumentTypeID == DocumentTypeConstants.PaymentBookImage
                         && m.Status == MissingDocumentStatusConstants.Pending)
                .FirstOrDefaultAsync();

            if (existingMissing != null)
            {
                var books = (existingMissing.Notes ?? "")
                    .Split('،')
                    .Select(n => n.Trim())
                    .Where(n => n.Length > 0)
                    .ToHashSet();
                books.Add(book.BookNumber);
                existingMissing.Notes = string.Join("، ", books.OrderBy(b => b));
            }
            else
            {
                var missingDoc = new MissingDocument
                {
                    FileID = dto.FileID,
                    DocumentTypeID = DocumentTypeConstants.PaymentBookImage,
                    Status = MissingDocumentStatusConstants.Pending,
                    Notes = $"دفتر الصرف رقم {book.BookNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.MissingDocuments.AddAsync(missingDoc);
            }
            await _unitOfWork.SaveChangesAsync();
        }
        // For electronic books: no image required, no missing document created

        await _reviewService.RequestReviewAsync(dto.FileID, userId, now);
        return ApiResponse<PaymentBookDto>.Success(MapToDto(book), "تم إصدار دفتر الصرف بنجاح");
    }

    public async Task<ApiResponse<PaymentBookDto>> UpdateAsync(int id, UpdatePaymentBookDto dto, int userId)
    {
        var book = await _unitOfWork.PaymentBooks.GetByIdAsync(id);
        if (book == null)
            return ApiResponse<PaymentBookDto>.Fail("الدفتر غير موجود");

        if (!string.IsNullOrWhiteSpace(dto.BookNumber)) book.BookNumber = dto.BookNumber;
        if (dto.IssueDate.HasValue) book.IssueDate = dto.IssueDate.Value;
        if (dto.ExpiryDate.HasValue) book.ExpiryDate = dto.ExpiryDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.Status)) book.Status = dto.Status;
        if (dto.LegacyColor != null) book.LegacyColor = dto.LegacyColor;
        if (dto.ChangeReason != null) book.ChangeReason = dto.ChangeReason;
        if (dto.RecipientName != null) book.RecipientName = dto.RecipientName;
        if (dto.RecipientType != null) book.RecipientType = dto.RecipientType;
        if (dto.Notes != null) book.Notes = dto.Notes;
        if (dto.IssuingBranchID.HasValue) book.IssuingBranchID = dto.IssuingBranchID;
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedByUserID = userId;

        _unitOfWork.PaymentBooks.Update(book);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        await _reviewService.RequestReviewAsync(book.FileID, userId, now);

        return ApiResponse<PaymentBookDto>.Success(MapToDto(book), "تم تحديث الدفتر");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var book = await _unitOfWork.PaymentBooks.GetByIdAsync(id);
        if (book == null || book.IsDeleted)
            return ApiResponse<bool>.Fail("الدفتر غير موجود");

        if (book.Status == PaymentBookStatus.Cancelled)
            return ApiResponse<bool>.Fail("الدفتر ملغي مسبقاً");

        book.Status = PaymentBookStatus.Cancelled;
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedByUserID = userId;

        _unitOfWork.PaymentBooks.Update(book);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        await _reviewService.RequestReviewAsync(book.FileID, userId, now);

        return ApiResponse<bool>.Success(true, "تم إلغاء تفعيل الدفتر");
    }

    private async Task<string> GenerateNextBookNumberAsync()
    {
        // Use local time for the year prefix (Iraq timezone)
        var year = DateTime.Now.Year;
        var prefix = $"PB-{year}-";

        // Retry loop to handle race conditions where two concurrent requests
        // might read the same lastBook and try to generate the same number
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var lastBook = await _unitOfWork.PaymentBooks.Query()
                .Where(b => b.BookNumber.StartsWith(prefix))
                .OrderByDescending(b => b.BookNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (lastBook != null)
            {
                var parts = lastBook.BookNumber.Split('-');
                if (parts.Length > 2 && int.TryParse(parts[2], out int lastSeq))
                    nextSeq = lastSeq + 1;
            }

            var candidate = $"{prefix}{nextSeq:D5}";

            // Check if this number already exists (race condition guard)
            var exists = await _unitOfWork.PaymentBooks.Query()
                .AnyAsync(b => b.BookNumber == candidate);

            if (!exists)
                return candidate;
        }

        // Fallback: use timestamp suffix
        return $"{prefix}{DateTime.UtcNow.Ticks % 100000:D5}";
    }

    private static string GetPreviousStatus(string? changeReason) => changeReason switch
    {
        "بدل فاقد" => PaymentBookStatus.Lost,
        "بدل تالف" => PaymentBookStatus.Damaged,
        _ => PaymentBookStatus.Expired
    };

    private static PaymentBookDto MapToDto(PaymentBook pb) => new()
    {
        PaymentBookID = pb.PaymentBookID,
        FileID = pb.FileID,
        BookNumber = pb.BookNumber,
        IssueDate = pb.IssueDate,
        ExpiryDate = pb.ExpiryDate,
        Status = pb.Status,
        BookType = pb.BookType,
        LegacyColor = pb.LegacyColor,
        ChangeReason = pb.ChangeReason,
        RecipientName = pb.RecipientName,
        RecipientType = pb.RecipientType,
        Notes = pb.Notes,
        IssuingBranchID = pb.IssuingBranchID,
        IssuingBranchName = pb.IssuingBranch?.BranchName,
        CreatedAt = pb.CreatedAt,
        UpdatedAt = pb.UpdatedAt,
        PendingReviewStatus = pb.PendingReviewStatus
    };

    private async Task<string> SaveFileAsync(IRemoteFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("الملف فارغ أو غير موجود");

        using var stream = file.OpenReadStream();
        return await _fileStorage.SaveFileAsync(stream, file.FileName, folder);
    }
}
