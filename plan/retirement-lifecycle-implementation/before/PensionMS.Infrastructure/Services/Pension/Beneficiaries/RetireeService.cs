using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;

namespace PensionMS.Infrastructure.Services.Pension.Beneficiaries;

public class RetireeService : IRetireeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentService _documentService;
    private readonly IComplianceEngine _complianceEngine;
    private readonly ILogger<RetireeService> _logger;
    private readonly IMemoryCache _cache;

    private const string RetireeCachePrefix = "retiree_";
    private const string FileCachePrefix = "retirement_file_";

    public RetireeService(
        IUnitOfWork unitOfWork, 
        IFileStorageService fileStorage, 
        IDocumentService documentService,
        IComplianceEngine complianceEngine,
        ILogger<RetireeService> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _documentService = documentService;
        _complianceEngine = complianceEngine;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ApiResponse<RetireeDto>> GetByIdAsync(int id)
    {
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(r => r.RetireeID == id && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<RetireeDto>.Fail("المتقاعد غير موجود");

        return ApiResponse<RetireeDto>.Success(MapToDto(retiree));
    }

    public async Task<ApiResponse<PagedResult<RetireeListDto>>> GetAllAsync(int page, int pageSize, string? search = null, SocialResearchQueryDto? filters = null)
    {
        var query = _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.RetirementFiles)
            .Include(r => r.SettlementItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmed = search.Trim();
            var normalizedDigits = StringHelpers.NormalizeDigits(trimmed);
            query = query.Where(r =>
                r.FullName.Contains(trimmed) ||
                r.NationalID.Contains(trimmed) ||
                r.NationalID.Contains(normalizedDigits) ||
                (r.MilitaryNumber != null && (r.MilitaryNumber.Contains(trimmed) || r.MilitaryNumber.Contains(normalizedDigits))));
        }

        if (filters != null)
        {
            if (filters.SocialStatusId.HasValue)
                query = query.Where(r => r.SocialStatusID == filters.SocialStatusId);
            if (filters.HealthStatusId.HasValue)
                query = query.Where(r => r.HealthStatusID == filters.HealthStatusId);
            if (filters.IsAlive.HasValue)
                query = query.Where(r => r.IsAlive == filters.IsAlive);
            if (!string.IsNullOrWhiteSpace(filters.Governorate))
                query = query.Where(r => r.Governorate == filters.Governorate);
            if (!string.IsNullOrWhiteSpace(filters.City))
                query = query.Where(r => r.City == filters.City);
            if (filters.CategoryId.HasValue)
                query = query.Where(r => r.ApprovalItem.CategoryID == filters.CategoryId);
            if (filters.RankId.HasValue)
                query = query.Where(r => r.ApprovalItem.RankID == filters.RankId);
            if (filters.Gender.HasValue)
                query = query.Where(r => r.Gender == filters.Gender);
            if (filters.HasFiles.HasValue && filters.HasFiles.Value)
                query = query.Where(r => r.RetirementFiles.Any());
            if (filters.IsProcessed.HasValue)
                query = query.Where(r => r.ApprovalItem != null && !r.ApprovalItem.IsDeleted && r.ApprovalItem.IsProcessed == filters.IsProcessed.Value);
            if (filters.HasSettlements.HasValue)
            {
                if (filters.HasSettlements.Value)
                    query = query.Where(r => r.SettlementItems.Any());
                else
                    query = query.Where(r => !r.SettlementItems.Any());
            }
            if (!string.IsNullOrWhiteSpace(filters.UnitName))
                query = query.Where(r => r.ApprovalItem != null && r.ApprovalItem.UnitName == filters.UnitName);
            if (filters.RetirementDateFrom.HasValue)
                query = query.Where(r => r.ApprovalItem.RetirementDate >= filters.RetirementDateFrom);
            if (filters.RetirementDateTo.HasValue)
                query = query.Where(r => r.ApprovalItem.RetirementDate <= filters.RetirementDateTo);
        }

        var total = await query.CountAsync();
        var retirees = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = retirees.Select(r => new RetireeListDto
        {
            RetireeID = r.RetireeID,
            NationalID = r.NationalID,
            FullName = r.FullName,
            MilitaryNumber = r.MilitaryNumber,
            LinkNumber = r.LinkNumber,
            Phone = r.Phone,
            City = r.City,
            Governorate = r.Governorate,
            RankName = r.ApprovalItem?.Rank?.RankName,
            RankId = r.ApprovalItem?.RankID,
            CategoryId = r.ApprovalItem?.CategoryID,
            Gender = r.Gender,
            UnitName = r.ApprovalItem?.UnitName,
            SocialStatusName = r.SocialStatus?.StatusName,
            HealthStatusName = r.HealthStatus?.HealthStatusName,
            IsAlive = r.IsAlive,
            FileCount = r.RetirementFiles.Count,
            FileID = r.RetirementFiles.OrderByDescending(f => f.CreatedAt).Select(f => f.FileID).Cast<int?>().FirstOrDefault()
        }).ToList();

        return ApiResponse<PagedResult<RetireeListDto>>.Success(new PagedResult<RetireeListDto>
        {
            Items = items, TotalCount = total, Page = page, PageSize = pageSize
        });
    }

    public async Task<ApiResponse<RetireeDto>> GetByNationalIDAsync(string nationalId)
    {
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(r => r.NationalID == nationalId && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<RetireeDto>.Fail("المتقاعد غير موجود");

        return ApiResponse<RetireeDto>.Success(MapToDto(retiree));
    }

    public async Task<ApiResponse<RetireeDto>> GetByMilitaryNumberAsync(string militaryNumber)
    {
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(r => r.MilitaryNumber == militaryNumber && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<RetireeDto>.Fail("المتقاعد غير موجود");

        return ApiResponse<RetireeDto>.Success(MapToDto(retiree));
    }



    public async Task<ApiResponse<RetireeDto>> UpdateAsync(int id, UpdateRetireeDto dto, int userId)
    {
        var retiree = await _unitOfWork.Retirees.GetByIdAsync(id);
        if (retiree == null)
            return ApiResponse<RetireeDto>.Fail("المتقاعد غير موجود");

        if (dto.HealthStatusID.HasValue &&
            (dto.HealthStatusID == HealthStatusConstants.Deceased || dto.HealthStatusID == HealthStatusConstants.Martyr) !=
            !retiree.IsAlive)
            return ApiResponse<RetireeDto>.Fail("تغيير حالة حياة المتقاعد يتطلب الإجراء المخصص من صفحة الملف");

        if (dto.FullName != null) retiree.FullName = dto.FullName;
        if (dto.NationalID != null) retiree.NationalID = dto.NationalID;
        if (dto.MilitaryNumber != null) retiree.MilitaryNumber = dto.MilitaryNumber;
        if (dto.LinkNumber != null) retiree.LinkNumber = dto.LinkNumber;
        if (dto.Gender.HasValue) retiree.Gender = dto.Gender.Value;
        if (dto.Phone != null) retiree.Phone = dto.Phone;
        if (dto.Address != null) retiree.Address = dto.Address;
        if (dto.City != null) retiree.City = dto.City;
        if (dto.Governorate != null) retiree.Governorate = dto.Governorate;
        if (dto.SocialStatusID.HasValue) retiree.SocialStatusID = dto.SocialStatusID;
        if (dto.HealthStatusID.HasValue) retiree.HealthStatusID = dto.HealthStatusID;

        // تحديث الصورة الشخصية
        if (dto.PersonalPhoto != null && dto.PersonalPhoto.Length > 0)
        {
            if (!string.IsNullOrEmpty(retiree.PersonalPhotoPath))
                await _fileStorage.DeleteFileAsync(retiree.PersonalPhotoPath);

            using var stream = dto.PersonalPhoto.OpenReadStream();
            retiree.PersonalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "photos");
        }

        retiree.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Retirees.Update(retiree);
        await _unitOfWork.SaveChangesAsync();

        // تحديث الهوية الوطنية (دمج تلقائي)
        if (dto.IdDocument != null || dto.IdDocumentBack != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId,
                    RetireeID = retiree.RetireeID,
                    Notes = $"تحديث الهوية الوطنية للمتقاعد: {retiree.FullName}",
                    File = dto.IdDocument!,
                    FileBack = dto.IdDocumentBack
                };

                if (dto.IdDocument == null && dto.IdDocumentBack != null)
                {
                    uploadDto.File = dto.IdDocumentBack;
                    uploadDto.FileBack = null;
                }

                await _documentService.UploadAsync(uploadDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل في تحديث هوية المتقاعد {RetireeID}", retiree.RetireeID);
            }
        }

        // Refresh cache for all files linked to this retiree
        var fileIds = await _unitOfWork.RetirementFiles.Query()
            .Where(f => f.RetireeID == id && !f.IsDeleted)
            .Select(f => f.FileID)
            .ToListAsync();

        foreach (var fileId in fileIds)
        {
            await _complianceEngine.RefreshComplianceCacheAsync(fileId);
        }

        return ApiResponse<RetireeDto>.Success(MapToDto(retiree), "تم تحديث بيانات المتقاعد");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var retiree = await _unitOfWork.Retirees.GetByIdAsync(id);
            if (retiree == null)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<bool>.Fail("المتقاعد غير موجود");
            }

            retiree.IsDeleted = true;
            retiree.DeletedAt = DateTime.UtcNow;
            retiree.DeletedByUserID = deletedByUserId;

            _unitOfWork.Retirees.Update(retiree);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<bool>.Success(true, "تم حذف المتقاعد");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// المباينة الشاملة — جمع كل بيانات المتقاعد
    /// </summary>
    public async Task<ApiResponse<RetireeStatementDto>> GetStatementAsync(int id)
    {
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(r => r.RetireeID == id && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<RetireeStatementDto>.Fail("المتقاعد غير موجود");

        // الموافقة الوزارية (1:1 مع المتقاعد)
        var approvalItems = new List<ApprovalItemSummaryDto>();
        if (retiree.ApprovalItem != null)
        {
            approvalItems.Add(new ApprovalItemSummaryDto
            {
                ApprovalItemID = retiree.ApprovalItem.ApprovalItemID,
                BatchNumber = retiree.ApprovalItem.Batch?.BatchNumber ?? "غير محدد",
                MilitaryNumber = retiree.ApprovalItem.MilitaryNumber,
                FullName = retiree.ApprovalItem.FullName,
                Gender = retiree.ApprovalItem.Gender,
                RankName = retiree.ApprovalItem.Rank?.RankName,
                UnitName = retiree.ApprovalItem.UnitName,
                RetirementDate = retiree.ApprovalItem.RetirementDate,
                Salary = retiree.ApprovalItem.Salary,
                CategoryName = retiree.ApprovalItem.Category?.CategoryName,
                Notes = retiree.ApprovalItem.Notes,
                IsProcessed = retiree.ApprovalItem.IsProcessed
            });
        }

        // ملفات التقاعد (مع الرتبة لعرضها في الاستحقاقات لاحقاً)
        var files = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Rank)
            .Include(f => f.Status)
            .Include(f => f.PensionBranch)
            .Where(f => f.RetireeID == id)
            .OrderByDescending(f => f.FileOpenDate)
            .Select(f => new RetirementFileSummaryDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                StatusName = f.Status != null ? f.Status.StatusName : null,
                RankName = f.Rank != null ? f.Rank.RankName : null,
                BranchName = f.PensionBranch != null ? f.PensionBranch.BranchName : "غير محدد",
                FileOpenDate = f.FileOpenDate
            })
            .ToListAsync();

        // التسويات
        var retireeMilNum = StringHelpers.NormalizeDigits(retiree.MilitaryNumber);
        var approvalIds = approvalItems.Select(a => a.ApprovalItemID).ToList();

        var settlements = await _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .Include(s => s.Batch)
            .Include(s => s.Decision)
            .Include(s => s.CurrentRank)
            .Include(s => s.ProcessedRank)
            .Where(s => !s.IsDeleted && (
                s.RetireeID == id || 
                (!string.IsNullOrEmpty(retireeMilNum) && s.MilitaryNumber == retireeMilNum) ||
                (s.ApprovalItemID != null && approvalIds.Contains(s.ApprovalItemID.Value))
            ))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SettlementSummaryDto
            {
                SettlementItemID = s.SettlementItemID,
                BatchNumber = s.Batch != null ? s.Batch.BatchNumber : "غير محدد",
                DecisionNumber = s.Decision != null ? s.Decision.DecisionNumber : null,
                DecisionDate = s.Decision != null ? s.Decision.DecisionDate : null,
                DecisionType = s.Decision != null ? s.Decision.DecisionType : null,
                CurrentRankName = s.CurrentRank != null ? s.CurrentRank.RankName : "غير محدد",
                ProcessedRankName = s.ProcessedRank != null ? s.ProcessedRank.RankName : "غير محدد",
                Notes = s.Notes,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        // الاستحقاقات المالية — now directly linked to Retiree (الرتبة من ملف التقاعد إن وُجد)
        var entitlements = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => e.RetireeID == id && !e.IsDeleted)
            .OrderByDescending(e => e.EffectiveDate)
            .Select(e => new EntitlementSummaryDto
            {
                EntitlementID = e.EntitlementID,
                RankName = null,
                Amount = e.Amount,
                EffectiveDate = e.EffectiveDate,
                EndDate = e.EndDate
            })
            .ToListAsync();

        // الرتبة للاستحقاقات: من ملف التقاعد، أو قيد الإحالة إلى التقاعد، أو التسوية
        var rankForEntitlements = files.FirstOrDefault(f => !string.IsNullOrEmpty(f.RankName))?.RankName
            ?? approvalItems.FirstOrDefault(a => !string.IsNullOrEmpty(a.RankName))?.RankName
            ?? settlements.FirstOrDefault()?.ProcessedRankName;
        if (!string.IsNullOrEmpty(rankForEntitlements))
        {
            foreach (var e in entitlements)
                e.RankName = rankForEntitlements;
        }

        var statement = new RetireeStatementDto
        {
            Retiree = MapToDto(retiree),
            ApprovalItems = approvalItems,
            RetirementFiles = files,
            Settlements = settlements,
            Entitlements = entitlements
        };

        return ApiResponse<RetireeStatementDto>.Success(statement);
    }



    public async Task<ApiResponse<RetireeFullDataDto>> GetFullDataAsync(int id)
    {
        var retiree = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(r => r.RetireeID == id);

        if (retiree == null)
            return ApiResponse<RetireeFullDataDto>.Fail("المتقاعد غير موجود");

        var dto = new RetireeFullDataDto
        {
            RetireeID = retiree.RetireeID,
            FullName = retiree.FullName,
            NationalID = retiree.NationalID,
            MilitaryNumber = retiree.MilitaryNumber,
            RankName = retiree.ApprovalItem?.Rank?.RankName,
            LinkNumber = retiree.LinkNumber,
            Gender = retiree.Gender,
            BirthDate = retiree.BirthDate,
            Phone = retiree.Phone,
            Address = retiree.Address,
            City = retiree.City,
            Governorate = retiree.Governorate,
            SocialStatusID = retiree.SocialStatusID,
            HealthStatusID = retiree.HealthStatusID,
            IsAlive = retiree.IsAlive,
            DeathDate = retiree.DeathDate,
            PersonalPhotoPath = retiree.PersonalPhotoPath
        };

        return ApiResponse<RetireeFullDataDto>.Success(dto);
    }

    public async Task<ApiResponse<string>> UploadPhotoAsync(int id, IRemoteFile photo)
    {
        var retiree = await _unitOfWork.Retirees.GetByIdAsync(id);
        if (retiree == null)
            return ApiResponse<string>.Fail("المتقاعد غير موجود");

        if (photo == null || photo.Length == 0)
            return ApiResponse<string>.Fail("لم يتم تحديد صورة");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return ApiResponse<string>.Fail("امتداد الصورة غير مسموح. المسموح: jpg, jpeg, png");

        if (photo.Length > 2 * 1024 * 1024) // 2 MB max
            return ApiResponse<string>.Fail("حجم الصورة يجب ألا يتجاوز 2 ميجابايت");

        try
        {
            using var stream = photo.OpenReadStream();
            var relativePath = await _fileStorage.SaveFileAsync(stream, photo.FileName, "photos");

            retiree.PersonalPhotoPath = relativePath;
            retiree.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Retirees.Update(retiree);
            await _unitOfWork.SaveChangesAsync();

            _cache.Remove(RetireeCachePrefix + id);

            var fileIds = await _unitOfWork.RetirementFiles.Query()
                .Where(f => f.RetireeID == id && !f.IsDeleted)
                .Select(f => f.FileID)
                .ToListAsync();

            foreach (var fileId in fileIds)
                _cache.Remove(FileCachePrefix + fileId);

            return ApiResponse<string>.Success(relativePath, "تم رفع وتحديث صورة المتقاعد المخصصة بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading retiree photo for retiree {Id}", id);
            return ApiResponse<string>.Fail("حدث خطأ أثناء رفع الصورة. يرجى المحاولة لاحقاً");
        }
    }

    private static RetireeDto MapToDto(Retiree r) => new()
    {
        RetireeID = r.RetireeID,
        NationalID = r.NationalID,
        FullName = r.FullName,
        MilitaryNumber = r.MilitaryNumber,
        LinkNumber = r.LinkNumber,
        Gender = r.Gender,
        Phone = r.Phone,
        Address = r.Address,
        City = r.City,
        Governorate = r.Governorate,
        UnitName = r.ApprovalItem?.UnitName,
        RankName = r.ApprovalItem?.Rank?.RankName,
        CategoryName = r.ApprovalItem?.Category?.CategoryName,
        EnrollmentDate = r.ApprovalItem?.EnrollmentDate,
        RetirementDate = r.ApprovalItem?.RetirementDate,
        IsAlive = r.IsAlive,
        DeathDate = r.DeathDate,
        PersonalPhotoPath = r.PersonalPhotoPath,
        SocialStatusName = r.SocialStatus?.StatusName,
        HealthStatusName = r.HealthStatus?.HealthStatusName,
        CreatedAt = r.CreatedAt,
        PendingReviewStatus = (int)r.PendingReviewStatus
    };
}
