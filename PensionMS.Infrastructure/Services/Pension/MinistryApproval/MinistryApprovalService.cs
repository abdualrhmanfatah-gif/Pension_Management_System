using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ministry;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.MinistryApprovals;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.Pension.MinistryApproval;

public class MinistryApprovalService : IMinistryApprovalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ExcelReportExporter _excelExporter;
    private readonly PdfReportExporter _pdfExporter;

    public MinistryApprovalService(IUnitOfWork unitOfWork, INotificationService notificationService, ExcelReportExporter excelExporter, PdfReportExporter pdfExporter)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    public async Task<ApiResponse<List<MinistryApprovalBatchDto>>> GetAllBatchesAsync()
    {
        var batches = await _unitOfWork.MinistryApprovalBatches.Query()
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .Include(b => b.ImportedByUser)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToBatchDto(b))
            .ToListAsync();

        return ApiResponse<List<MinistryApprovalBatchDto>>.Success(batches);
    }

    public async Task<ApiResponse<MinistryApprovalBatchDto>> GetBatchByIdAsync(int id)
    {
        var batch = await _unitOfWork.MinistryApprovalBatches.Query()
            .AsNoTracking()
            .Include(b => b.ImportedByUser)
            .FirstOrDefaultAsync(b => b.BatchID == id && !b.IsDeleted);

        if (batch == null)
            return ApiResponse<MinistryApprovalBatchDto>.Fail("كشف الإحالة إلى التقاعد غير موجود");

        return ApiResponse<MinistryApprovalBatchDto>.Success(MapToBatchDto(batch));
    }

    public async Task<ApiResponse<MinistryApprovalBatchDto>> CreateBatchAsync(CreateBatchDto dto, int userId)
    {
        var batch = new MinistryApprovalBatch
        {
            BatchNumber = dto.BatchNumber,
            BatchDate = dto.BatchDate,
            ReceivedDate = dto.ReceivedDate,
            Notes = dto.Notes,
            ImportedByUserID = userId,
            Status = BatchStatusConstants.New,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MinistryApprovalBatches.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
            {
                UserID = 0,
                Title = "كشف إحالة جديد",
                Message = $"تم إنشاء كشف إحالة إلى التقاعد رقم ({batch.BatchNumber}) بانتظار استيراد البيانات.",
                Type = NotificationConstants.Types.Info,
                Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedRecordID = batch.BatchID
                });
            }
            catch { }

            // Reload with ImportedByUser
        var savedBatch = await _unitOfWork.MinistryApprovalBatches.Query()
            .Include(b => b.ImportedByUser)
            .FirstAsync(b => b.BatchID == batch.BatchID);

        return ApiResponse<MinistryApprovalBatchDto>.Success(MapToBatchDto(savedBatch), "تم إنشاء الكشف بنجاح");
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(int batchId, IRemoteFile file)
    {
        var batch = await _unitOfWork.MinistryApprovalBatches.Query()
            .Include(b => b.MinistryApprovalItems)
            .FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);

        if (batch == null) return ApiResponse<int>.Fail("كشف الإحالة إلى التقاعد غير موجود");

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var stream = new MemoryStream();
        using (var fileStream = file.OpenReadStream())
        {
            await fileStream.CopyToAsync(stream);
        }
        stream.Position = 0;
        using var package = new ExcelPackage(stream);
        
        var worksheet = package.Workbook.Worksheets[0];
        if (worksheet == null || worksheet.Dimension == null) 
            return ApiResponse<int>.Fail("ملف Excel فارغ أو لا يحتوي على بيانات");

        int rowCount = worksheet.Dimension.Rows;
        int importedCount = 0;
        var failedRows = new List<int>();
        var ranks = await _unitOfWork.Ranks.Query().AsNoTracking().ToListAsync();
        var categories = await _unitOfWork.RetireeCategories.Query().AsNoTracking().ToListAsync();

        var categoryLookup = categories.ToDictionary(
            c => NormalizeText(c.CategoryName),
            c => c
        );

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var newCategoryNames = new HashSet<string>();
            for (int r = 2; r <= rowCount; r++)
            {
                var catName = worksheet.Cells[r, 9].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(catName)) continue;
                var normalized = NormalizeText(catName);
                if (!categoryLookup.ContainsKey(normalized))
                    newCategoryNames.Add(catName);
            }

            if (newCategoryNames.Count > 0)
            {
                var seenNormalized = new HashSet<string>();
                foreach (var name in newCategoryNames)
                {
                    var norm = NormalizeText(name);
                    if (seenNormalized.Contains(norm)) continue;
                    seenNormalized.Add(norm);

                    var newCat = new RetireeCategory
                    {
                        CategoryName = name.Trim(),
                        Description = "تم إنشاؤها تلقائياً من كشف الإحالة إلى التقاعد",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.RetireeCategories.AddAsync(newCat);
                    categoryLookup[norm] = newCat;
                }
                await _unitOfWork.SaveChangesAsync();
            }

            var existingMilNumbers = batch.MinistryApprovalItems
                .Where(i => !i.IsDeleted)
                .Select(i => i.MilitaryNumber)
                .ToHashSet();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var militaryNumber = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(militaryNumber)) continue;

                    if (existingMilNumbers.Contains(militaryNumber))
                    {
                        failedRows.Add(row);
                        continue;
                    }

                    var rankName = worksheet.Cells[row, 3].Value?.ToString();
                    var rank = ranks.FirstOrDefault(r => NormalizeText(r.RankName) == NormalizeText(rankName));
                    var fullName = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";

                    var categoryNameRaw = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                    RetireeCategory? category = null;
                    if (!string.IsNullOrWhiteSpace(categoryNameRaw))
                        categoryLookup.TryGetValue(NormalizeText(categoryNameRaw), out category);

                    var item = new MinistryApprovalItem
                    {
                        BatchID = batchId,
                        SerialNumber = int.TryParse(worksheet.Cells[row, 1].Value?.ToString(), out int sr) ? sr : 0,
                        MilitaryNumber = militaryNumber,
                        RankID = rank?.RankID,
                        FullName = fullName,
                        UnitName = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                        EnrollmentDate = ParseDate(worksheet.Cells[row, 6].Value),
                        RetirementDate = ParseDate(worksheet.Cells[row, 7].Value),
                        Salary = decimal.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out decimal sal) ? sal : null,
                        CategoryID = category?.CategoryID,
                        LinkNumber = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
                        Gender = ParseGender(worksheet.Cells[row, 11].Value),
                        IsProcessed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.MinistryApprovalItems.AddAsync(item);
                    existingMilNumbers.Add(militaryNumber);
                    importedCount++;
                }
                catch
                {
                    failedRows.Add(row);
                }
            }

            if (importedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
                batch.TotalCount = await _unitOfWork.MinistryApprovalItems.Query().CountAsync(i => i.BatchID == batchId && !i.IsDeleted);
                batch.Status = BatchStatusConstants.Processing;
                batch.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.MinistryApprovalBatches.Update(batch);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();

            var message = $"تم استيراد {importedCount} قيد بنجاح";
            if (failedRows.Count > 0)
                message += $" | تم تخطي {failedRows.Count} صف (تكرار أو خطأ)";

            return ApiResponse<int>.Success(importedCount, message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<int>.Fail("خطأ أثناء الاستيراد. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<ApiResponse<PagedResult<MinistryApprovalItemDto>>> GetBatchItemsAsync(int batchId, int pageNumber, int pageSize, string? searchTerm, byte? gender = null)
    {
        var query = _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            query = query.Where(i => i.FullName.Contains(s) || i.MilitaryNumber.Contains(s));
        }

        if (gender.HasValue)
            query = query.Where(i => i.Gender == gender.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(i => i.SerialNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new MinistryApprovalItemDto
            {
                ApprovalItemID = i.ApprovalItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.MilitaryNumber,
                FullName = i.FullName,
                RankID = i.RankID,
                RankName = i.Rank != null ? i.Rank.RankName : null,
                UnitName = i.UnitName,
                EnrollmentDate = i.EnrollmentDate,
                RetirementDate = i.RetirementDate,
                Salary = i.Salary,
                CategoryID = i.CategoryID,
                CategoryName = i.Category != null ? i.Category.CategoryName : null,
                Gender = i.Gender,
                Notes = i.Notes,
                LinkNumber = i.LinkNumber,
                IsProcessed = i.IsProcessed,
                RetireeID = i.Retiree != null ? i.Retiree.RetireeID : null,
                BatchNumber = i.Batch != null ? i.Batch.BatchNumber : null,
                BatchDate = i.Batch != null ? i.Batch.BatchDate : null,
                ProcessedAt = i.ProcessedAt,
                ProcessedByUser = i.ProcessedByUser != null ? i.ProcessedByUser.FullName : null
            })
            .ToListAsync();

        var result = new PagedResult<MinistryApprovalItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<MinistryApprovalItemDto>>.Success(result);
    }

    public async Task<ApiResponse<PagedResult<MinistryApprovalItemDto>>> GetAllItemsAsync(int pageNumber, int pageSize, string? searchTerm, bool? isProcessed, byte? gender = null)
    {
        var query = _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            query = query.Where(i => i.FullName.Contains(s) || i.MilitaryNumber.Contains(s));
        }

        if (isProcessed.HasValue)
        {
            query = query.Where(i => i.IsProcessed == isProcessed.Value);
        }

        if (gender.HasValue)
            query = query.Where(i => i.Gender == gender.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new MinistryApprovalItemDto
            {
                ApprovalItemID = i.ApprovalItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.MilitaryNumber,
                FullName = i.FullName,
                RankID = i.RankID,
                RankName = i.Rank != null ? i.Rank.RankName : null,
                UnitName = i.UnitName,
                EnrollmentDate = i.EnrollmentDate,
                RetirementDate = i.RetirementDate,
                Salary = i.Salary,
                CategoryID = i.CategoryID,
                CategoryName = i.Category != null ? i.Category.CategoryName : null,
                Gender = i.Gender,
                Notes = i.Notes,
                LinkNumber = i.LinkNumber,
                IsProcessed = i.IsProcessed,
                RetireeID = i.Retiree != null ? i.Retiree.RetireeID : null,
                BatchNumber = i.Batch != null ? i.Batch.BatchNumber : null,
                BatchDate = i.Batch != null ? i.Batch.BatchDate : null,
                ProcessedAt = i.ProcessedAt,
                ProcessedByUser = i.ProcessedByUser != null ? i.ProcessedByUser.FullName : null
            })
            .ToListAsync();

        var result = new PagedResult<MinistryApprovalItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<MinistryApprovalItemDto>>.Success(result);
    }

    public async Task<ApiResponse<MinistryApprovalItemDto>> GetItemByIdAsync(int itemId)
    {
        var item = await _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Where(i => i.ApprovalItemID == itemId && !i.IsDeleted)
            .Select(i => new MinistryApprovalItemDto
            {
                ApprovalItemID = i.ApprovalItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.MilitaryNumber,
                FullName = i.FullName,
                RankID = i.RankID,
                RankName = i.Rank != null ? i.Rank.RankName : null,
                UnitName = i.UnitName,
                EnrollmentDate = i.EnrollmentDate,
                RetirementDate = i.RetirementDate,
                Salary = i.Salary,
                CategoryID = i.CategoryID,
                CategoryName = i.Category != null ? i.Category.CategoryName : null,
                Gender = i.Gender,
                Notes = i.Notes,
                LinkNumber = i.LinkNumber,
                IsProcessed = i.IsProcessed,
                RetireeID = i.Retiree != null ? i.Retiree.RetireeID : null,
                BatchNumber = i.Batch != null ? i.Batch.BatchNumber : null,
                BatchDate = i.Batch != null ? i.Batch.BatchDate : null,
                ProcessedAt = i.ProcessedAt,
                ProcessedByUser = i.ProcessedByUser != null ? i.ProcessedByUser.FullName : null
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return ApiResponse<MinistryApprovalItemDto>.Fail("القيد غير موجود");

        return ApiResponse<MinistryApprovalItemDto>.Success(item);
    }

    private byte? ParseGender(object? value)
    {
        if (value == null) return 1;
        var val = value.ToString()?.Trim();
        if (val.Contains("أنثى") || val.Contains("انثى") || val == GenderConstants.FemaleCode) return GenderConstants.Female;
        return GenderConstants.Male;
    }

    public async Task<ApiResponse<MinistryApprovalItemDto>> AddManualItemAsync(int batchId, AddManualItemDto dto, int userId)
    {
        var batch = await _unitOfWork.MinistryApprovalBatches.Query()
            .FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);

        if (batch == null) return ApiResponse<MinistryApprovalItemDto>.Fail("كشف الإحالة إلى التقاعد غير موجود");

        var militaryNumber = dto.MilitaryNumber.Trim();

        var duplicate = await _unitOfWork.MinistryApprovalItems.Query()
            .AnyAsync(i => i.BatchID == batchId && i.MilitaryNumber == militaryNumber && !i.IsDeleted);
        if (duplicate)
            return ApiResponse<MinistryApprovalItemDto>.Fail("الرقم العسكري موجود مسبقاً في هذا الكشف");

        var lastSerial = await _unitOfWork.MinistryApprovalItems.Query()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Select(i => (int?)i.SerialNumber)
            .MaxAsync() ?? 0;

        var item = new MinistryApprovalItem
        {
            BatchID = batchId,
            SerialNumber = lastSerial + 1,
            MilitaryNumber = militaryNumber,
            FullName = dto.FullName.Trim(),
            RankID = dto.RankID,
            UnitName = dto.UnitName,
            EnrollmentDate = dto.EnrollmentDate,
            RetirementDate = dto.RetirementDate,
            Salary = dto.Salary,
            CategoryID = dto.CategoryID,
            Gender = dto.Gender ?? 1,
            Notes = dto.Notes,
            LinkNumber = dto.LinkNumber,
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MinistryApprovalItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        await RefreshBatchCountersAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<MinistryApprovalItemDto>.Success(new MinistryApprovalItemDto { ApprovalItemID = item.ApprovalItemID }, "تم إضافة القيد بنجاح");
    }

    public async Task<ApiResponse<MinistryApprovalItemDto>> UpdateManualItemAsync(int itemId, UpdateManualItemDto dto)
    {
        var item = await _unitOfWork.MinistryApprovalItems.Query()
            .FirstOrDefaultAsync(i => i.ApprovalItemID == itemId && !i.IsDeleted);

        if (item == null) return ApiResponse<MinistryApprovalItemDto>.Fail("القيد غير موجود");
        if (item.IsProcessed) return ApiResponse<MinistryApprovalItemDto>.Fail("لا يمكن تعديل قيد تمت معالجته");

        var militaryNumber = dto.MilitaryNumber.Trim();

        var duplicate = await _unitOfWork.MinistryApprovalItems.Query()
            .AnyAsync(i => i.BatchID == item.BatchID && i.ApprovalItemID != itemId && i.MilitaryNumber == militaryNumber && !i.IsDeleted);
        if (duplicate)
            return ApiResponse<MinistryApprovalItemDto>.Fail("الرقم العسكري موجود مسبقاً في هذا الكشف");

        item.MilitaryNumber = militaryNumber;
        item.FullName = dto.FullName.Trim();
        item.RankID = dto.RankID;
        item.UnitName = dto.UnitName;
        item.EnrollmentDate = dto.EnrollmentDate;
        item.RetirementDate = dto.RetirementDate;
        item.Salary = dto.Salary;
        item.CategoryID = dto.CategoryID;
        if (dto.Gender.HasValue) item.Gender = dto.Gender;
        item.Notes = dto.Notes;
        item.LinkNumber = dto.LinkNumber;
        _unitOfWork.MinistryApprovalItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<MinistryApprovalItemDto>.Success(new MinistryApprovalItemDto { ApprovalItemID = item.ApprovalItemID }, "تم تعديل القيد بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteManualItemAsync(int itemId, int userId)
    {
        var item = await _unitOfWork.MinistryApprovalItems.Query()
            .FirstOrDefaultAsync(i => i.ApprovalItemID == itemId && !i.IsDeleted);

        if (item == null) return ApiResponse<bool>.Fail("القيد غير موجود");
        if (item.IsProcessed) return ApiResponse<bool>.Fail("لا يمكن حذف قيد تمت معالجته");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserID = userId;
            _unitOfWork.MinistryApprovalItems.Update(item);
            await _unitOfWork.SaveChangesAsync();

            var batch = await _unitOfWork.MinistryApprovalBatches.Query()
                .FirstOrDefaultAsync(b => b.BatchID == item.BatchID && !b.IsDeleted);
            if (batch != null)
            {
                await RefreshBatchCountersAsync(batch);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();

            try
            {
                await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                {
                    UserID = 0,
                    Title = "حذف قيد إحالة",
                    Message = $"تم حذف قيد الإحالة رقم ({item.MilitaryNumber}) من كشف الإحالة إلى التقاعد.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedRecordID = item.BatchID
                });
            }
            catch { }

            return ApiResponse<bool>.Success(true, "تم حذف القيد بنجاح");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail("خطأ في حذف القيد. يرجى المحاولة لاحقاً");
        }
    }

    private async Task RefreshBatchCountersAsync(MinistryApprovalBatch batch)
    {
        var items = _unitOfWork.MinistryApprovalItems.Query()
            .Where(i => i.BatchID == batch.BatchID && !i.IsDeleted);

        batch.TotalCount = await items.CountAsync();
        batch.ProcessedCount = await items.CountAsync(i => i.IsProcessed);

        if (batch.TotalCount == 0) batch.Status = BatchStatusConstants.New;
        else if (batch.ProcessedCount >= batch.TotalCount) batch.Status = BatchStatusConstants.Completed;
        else if (batch.Status == BatchStatusConstants.Completed) batch.Status = BatchStatusConstants.Processing;

        batch.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.MinistryApprovalBatches.Update(batch);
    }

    public async Task<ApiResponse<object>> ProcessItemAsync(int itemId, int processedByUserId)
    {
        var item = await _unitOfWork.MinistryApprovalItems.Query()
            .Include(i => i.Batch)
            .FirstOrDefaultAsync(i => i.ApprovalItemID == itemId && !i.IsDeleted);
        if (item == null) return ApiResponse<object>.Fail("القيد غير موجود");
        if (item.IsProcessed) return ApiResponse<object>.Fail("هذا القيد تمت معالجته مسبقاً");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var retiree = await _unitOfWork.Retirees.Query().FirstOrDefaultAsync(r => r.MilitaryNumber == item.MilitaryNumber && !r.IsDeleted);
            if (retiree == null)
            {
                retiree = Retiree.CreateFromApprovalItem(item);
                await _unitOfWork.Retirees.AddAsync(retiree);
                await _unitOfWork.SaveChangesAsync();
            }

            item.IsProcessed = true;
            item.ProcessedAt = DateTime.UtcNow;
            item.ProcessedByUserID = processedByUserId;
            _unitOfWork.MinistryApprovalItems.Update(item);

            var unmatchedSettlements = await _unitOfWork.SettlementItems.Query()
                .Where(s => s.MilitaryNumber == item.MilitaryNumber && s.RetireeID == null && !s.IsDeleted)
                .ToListAsync();
                
            foreach(var settlement in unmatchedSettlements)
            {
                settlement.RetireeID = retiree.RetireeID;
                settlement.ApprovalItemID = item.ApprovalItemID;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.SettlementItems.Update(settlement);
            }

            await _unitOfWork.SaveChangesAsync();

            var batch = item.Batch;
            if (batch != null)
            {
                batch.ProcessedCount = await _unitOfWork.MinistryApprovalItems.Query().CountAsync(i => i.BatchID == batch.BatchID && i.IsProcessed && !i.IsDeleted);
                if (batch.ProcessedCount >= batch.TotalCount) batch.Status = BatchStatusConstants.Completed;
                else batch.Status = BatchStatusConstants.Processing;
                batch.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.MinistryApprovalBatches.Update(batch);
                
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();
            return ApiResponse<object>.Success(new { retireeID = retiree.RetireeID }, "تمت معالجة القيد بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<object>.Fail("خطأ في معالجة القيد. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBatchAsync(int id, int userId)
    {
        var batch = await _unitOfWork.MinistryApprovalBatches.Query()
            .Include(b => b.MinistryApprovalItems)
            .FirstOrDefaultAsync(b => b.BatchID == id && !b.IsDeleted);

        if (batch == null) return ApiResponse<bool>.Fail("الكشف غير موجود");
        if (batch.MinistryApprovalItems.Any(i => i.IsProcessed && !i.IsDeleted)) 
            return ApiResponse<bool>.Fail("لا يمكن حذف الكشف لاحتوائه على قيود تمت معالجتها");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.UtcNow;
            batch.DeletedByUserID = userId;
            _unitOfWork.MinistryApprovalBatches.Update(batch);

            foreach (var item in batch.MinistryApprovalItems.Where(i => !i.IsDeleted)) 
            { 
                item.IsDeleted = true; 
                item.DeletedAt = DateTime.UtcNow;
                _unitOfWork.MinistryApprovalItems.Update(item);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            try
            {
                await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                {
                    UserID = 0,
                    Title = "حذف كشف إحالة",
                    Message = $"تم حذف كشف الإحالة إلى التقاعد رقم ({batch.BatchNumber}).",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedRecordID = id
                });
            }
            catch { }

            return ApiResponse<bool>.Success(true, "تم حذف الكشف بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail("خطأ في حذف الكشف. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<ApiResponse<ForceDashboardStatsDto>> GetDashboardStatsAsync()
    {
        var activeBatchIds = await _unitOfWork.MinistryApprovalBatches.Query()
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .Select(b => b.BatchID)
            .ToListAsync();

        var stats = new ForceDashboardStatsDto
        {
            TotalBatches = activeBatchIds.Count,
            ProcessedRecords = await _unitOfWork.MinistryApprovalItems.Query()
                .CountAsync(i => activeBatchIds.Contains(i.BatchID) && i.IsProcessed && !i.IsDeleted),
            PendingRecords = await _unitOfWork.MinistryApprovalItems.Query()
                .CountAsync(i => activeBatchIds.Contains(i.BatchID) && !i.IsProcessed && !i.IsDeleted),
            ReconciliationGaps = await _unitOfWork.MinistryApprovalItems.Query()
                .CountAsync(i => activeBatchIds.Contains(i.BatchID) && i.IsProcessed && i.Retiree == null && !i.IsDeleted)
        };
        return ApiResponse<ForceDashboardStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<List<ForceGlobalSearchDto>>> SearchGlobalForceAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return ApiResponse<List<ForceGlobalSearchDto>>.Success(new List<ForceGlobalSearchDto>());
        var results = await _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Include(i => i.Batch)
            .Include(i => i.Rank)
            .Where(i => !i.IsDeleted && (i.MilitaryNumber.Contains(searchTerm) || i.FullName.Contains(searchTerm)))
            .OrderBy(i => i.FullName).Take(100)
            .Select(i => new ForceGlobalSearchDto 
            { 
                ApprovalItemID = i.ApprovalItemID,
                MilitaryID = i.MilitaryNumber, 
                FullName = i.FullName, 
                RankName = i.Rank != null ? i.Rank.RankName : null,
                UnitName = i.UnitName,
                BatchName = i.Batch != null ? i.Batch.BatchNumber : null, 
                Gender = i.Gender,
                Status = i.IsProcessed ? BatchStatusConstants.Completed : BatchStatusConstants.Processing,
                IsProcessed = i.IsProcessed,
                RetireeID = i.Retiree != null ? i.Retiree.RetireeID : null
            })
            .ToListAsync();
        return ApiResponse<List<ForceGlobalSearchDto>>.Success(results);
    }

    public async Task<ApiResponse<PagedResult<ReconciliationRowDto>>> GetReconciliationMatrixAsync(int batchId, int pageNumber, int pageSize)
    {
        var query = _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(i => i.SerialNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var militaryNumbers = items.Select(i => i.MilitaryNumber).ToList();
        var existingRetirees = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Where(r => militaryNumbers.Contains(r.MilitaryNumber!) && !r.IsDeleted)
            .ToDictionaryAsync(r => r.MilitaryNumber!);

        var matrix = items.Select(item => {
            existingRetirees.TryGetValue(item.MilitaryNumber, out var retiree);
            string gap = retiree == null ? "غير موجود" : (NormalizeText(item.FullName) != NormalizeText(retiree.FullName) ? "اختلاف اسم" : "مطابق");
            return new ReconciliationRowDto { ApprovalItemID = item.ApprovalItemID, MilitaryId = item.MilitaryNumber, MinistryName = item.FullName, DbName = retiree?.FullName, StatusGap = gap, IsActionRequired = retiree == null };
        }).ToList();

        var result = new PagedResult<ReconciliationRowDto>
        {
            Items = matrix,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<ReconciliationRowDto>>.Success(result);
    }

    public async Task<ApiResponse<int>> ProcessBulkApprovalsAsync(List<int> approvalIds, int userId)
    {
        if (approvalIds == null || !approvalIds.Any()) return ApiResponse<int>.Fail("لم يتم تحديد قيود");
        await _unitOfWork.BeginTransactionAsync();
        try {
            var items = await _unitOfWork.MinistryApprovalItems.Query()
                .Include(i => i.Batch)
                .Where(i => approvalIds.Contains(i.ApprovalItemID) && !i.IsProcessed && !i.IsDeleted)
                .ToListAsync();
            if (!items.Any()) return ApiResponse<int>.Fail("لا توجد قيود صالحة");
            
            var militaryNumbers = items.Select(i => i.MilitaryNumber).ToList();
            var existingRetirees = await _unitOfWork.Retirees.Query()
                .Where(r => militaryNumbers.Contains(r.MilitaryNumber!) && !r.IsDeleted)
                .ToDictionaryAsync(r => r.MilitaryNumber!);

            var unmatchedSettlements = await _unitOfWork.SettlementItems.Query()
                .Where(s => militaryNumbers.Contains(s.MilitaryNumber) && s.RetireeID == null && !s.IsDeleted)
                .ToListAsync();

            foreach (var item in items) {
                if (!existingRetirees.TryGetValue(item.MilitaryNumber, out var retiree)) {
                    retiree = Retiree.CreateFromApprovalItem(item);
                    await _unitOfWork.Retirees.AddAsync(retiree);
                    existingRetirees[item.MilitaryNumber] = retiree;
                }
                item.IsProcessed = true; item.ProcessedAt = DateTime.UtcNow; item.ProcessedByUserID = userId;
                _unitOfWork.MinistryApprovalItems.Update(item);
                
                var itemSettlements = unmatchedSettlements.Where(s => s.MilitaryNumber == item.MilitaryNumber);
                foreach(var settlement in itemSettlements)
                {
                    settlement.RetireeID = retiree.RetireeID;
                    settlement.ApprovalItemID = item.ApprovalItemID;
                    settlement.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.SettlementItems.Update(settlement);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            
            foreach (var b in items.Select(i => i.Batch).Distinct()) {
                if (b != null)
                {
                    b.ProcessedCount = await _unitOfWork.MinistryApprovalItems.Query().CountAsync(i => i.BatchID == b.BatchID && i.IsProcessed && !i.IsDeleted);
                    if (b.ProcessedCount >= b.TotalCount) b.Status = BatchStatusConstants.Completed;
                    _unitOfWork.MinistryApprovalBatches.Update(b);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            try
            {
                await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                {
                    UserID = 0,
                    Title = "معالجة جماعية لقيود الإحالة",
                    Message = $"تمت معالجة {items.Count} قيد إحالة بنجاح.",
                    Type = NotificationConstants.Types.Success,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedRecordID = items.FirstOrDefault()?.BatchID ?? 0
                });
            }
            catch { }

            return ApiResponse<int>.Success(items.Count, $"تمت معالجة {items.Count} قيد");
        } catch (Exception ex) { await _unitOfWork.RollbackAsync(); return ApiResponse<int>.Fail("حدث خطأ أثناء العملية. يرجى المحاولة لاحقاً"); }
    }

    public async Task<ApiResponse<List<ForceAuditLogDto>>> GetBatchAuditTrailAsync(int batchId)
    {
        var batchTableName = "MinistryApprovalBatches";
        var itemTableName = "MinistryApprovalItems";

        var tableIds = await _unitOfWork.SystemTables.Query()
            .AsNoTracking()
            .Where(t => t.TableNameEn == batchTableName || t.TableNameEn == itemTableName)
            .ToDictionaryAsync(t => t.TableNameEn, t => t.TableID);

        tableIds.TryGetValue(batchTableName, out var batchTableId);
        tableIds.TryGetValue(itemTableName, out var itemTableId);

        var itemIds = await _unitOfWork.MinistryApprovalItems.Query()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Select(i => i.ApprovalItemID)
            .ToListAsync();

        var logs = await _unitOfWork.AuditLogs.Query()
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => 
                (batchTableId != 0 && l.TableID == batchTableId && l.RecordID == batchId) ||
                (itemTableId != 0 && l.TableID == itemTableId && l.RecordID.HasValue && itemIds.Contains(l.RecordID.Value))
            )
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new ForceAuditLogDto 
            { 
                ActionDate = l.CreatedAt, 
                UserName = l.User != null ? l.User.FullName : "النظام", 
                ActionType = TranslateAction(l.Action), 
                OldStatus = l.OldValues, 
                NewStatus = l.NewValues, 
                Notes = l.EntityDisplayName ?? "عملية على " + (l.TableID == batchTableId ? "الكشف" : "القيد")
            })
            .ToListAsync();

        return ApiResponse<List<ForceAuditLogDto>>.Success(logs);
    }

    private static string TranslateAction(string action)
    {
        return action switch
        {
            "Create" => "إنشاء / رفع",
            "Update" => "تعديل / معالجة",
            "Delete" => "حذف",
            _ => action
        };
    }

    public async Task<byte[]> ExportBatchReportAsync(int batchId, string format)
    {
        var batch = await _unitOfWork.MinistryApprovalBatches.Query()
            .Include(b => b.MinistryApprovalItems)
                .ThenInclude(i => i.Rank)
            .Include(b => b.MinistryApprovalItems)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);
        if (batch == null) return Array.Empty<byte>();
        var items = batch.MinistryApprovalItems.Where(i => !i.IsDeleted).OrderBy(i => i.SerialNumber).ToList();

        var headers = new[] { "م", "الرقم العسكري", "الرتبة", "الاسم الكامل", "الوحدة", "تاريخ الالتحاق", "تاريخ الإحالة", "المعاش", "الفئة التقاعدية", "الربط", "الجنس", "الحالة" };
        var rows = items.Select((it, i) => new object?[]
        {
            it.SerialNumber,
            it.MilitaryNumber,
            it.Rank?.RankName,
            it.FullName,
            it.UnitName,
            it.EnrollmentDate?.ToString("yyyy/MM/dd"),
            it.RetirementDate?.ToString("yyyy/MM/dd"),
            it.Salary,
            it.Category?.CategoryName,
            it.LinkNumber,
            it.Gender == 2 ? "أنثى" : "ذكر",
            it.IsProcessed ? BatchStatusConstants.Processed : BatchStatusConstants.Pending,
        }).ToArray();

        if (format.ToLower() == ExportFormatConstants.Excel)
            return _excelExporter.ExportRows("كشف الإحالة إلى التقاعد", headers, rows,
                new ExcelReportOptions { ReportId = batchId.ToString(), IncludeFooter = false });

        if (format.ToLower() == ExportFormatConstants.Pdf)
            return _pdfExporter.ExportRows("تقرير اعتماد القوة", headers, rows, batch.BatchNumber);

        return Array.Empty<byte>();
    }

    private static MinistryApprovalBatchDto MapToBatchDto(MinistryApprovalBatch b) => new() {
        BatchID = b.BatchID, BatchNumber = b.BatchNumber, BatchDate = b.BatchDate, ReceivedDate = b.ReceivedDate,
        TotalCount = b.TotalCount, ProcessedCount = b.ProcessedCount, Status = b.Status, Notes = b.Notes,
        ImportedByUser = b.ImportedByUser?.FullName, CreatedAt = b.CreatedAt
    };

    private DateOnly? ParseDate(object? value) {
        if (value == null) return null;
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is double d) return DateOnly.FromDateTime(DateTime.FromOADate(d));
        var str = value.ToString()?.Trim();
        if (string.IsNullOrEmpty(str)) return null;

        str = str.Replace("٠", "0").Replace("١", "1").Replace("٢", "2").Replace("٣", "3").Replace("٤", "4")
                 .Replace("٥", "5").Replace("٦", "6").Replace("٧", "7").Replace("٨", "8").Replace("٩", "9");

        if (double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double oa) && oa > 1 && oa < 2958466)
            return DateOnly.FromDateTime(DateTime.FromOADate(oa));

        var formats = new[] {
            "d/M/yyyy", "dd/MM/yyyy", "d/M/yy", "dd/MM/yy",
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy/M/d",
            "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss",
            "d/M/yyyy h:mm:ss tt", "yyyy/MM/dd h:mm:ss tt",
            "M/d/yyyy", "MM/dd/yyyy", "M/d/yyyy h:mm:ss tt",
            "d-M-yyyy", "dd-MM-yyyy",
            "yyyy.MM.dd",
        };
        if (DateTime.TryParseExact(str, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            return DateOnly.FromDateTime(dt);
        if (DateTime.TryParse(str, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            return DateOnly.FromDateTime(dt);
        return null;
    }

    private string NormalizeText(string? text) {
        if (string.IsNullOrWhiteSpace(text)) return "";
        return text.Trim().Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا").Replace("ة", "ه").Replace("ى", "ي").Replace("ـ", "");
    }
}
