import fs from 'node:fs';
const root='F:/Projects/Pension_Management_System/';
function edit(path,apply){const s=fs.readFileSync(root+path,'utf8');const backup=root+'plan/retirement-lifecycle-implementation/before/'+path;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,s);}fs.writeFileSync(root+path,apply(s).replace(/\r?\n/g,'\r\n'));}
function block(s,begin,end,next){const a=s.indexOf(begin),b=s.indexOf(end,a+begin.length);if(a<0||b<0)throw new Error('Missing '+begin);return s.slice(0,a)+next+s.slice(b);}
edit('PensionMS.Infrastructure/Services/Pension/Files/FileStatusService.cs',s=>{
 s=s.replace('    private readonly IUnitOfWork _unitOfWork;', '    private readonly IUnitOfWork _unitOfWork;\n    private readonly IFileReviewService _reviewService;');
 s=s.replace('        INotificationService notificationService)', '        INotificationService notificationService, IFileReviewService reviewService)');
 s=s.replace('        _unitOfWork = unitOfWork;', '        _unitOfWork = unitOfWork;\n        _reviewService = reviewService;');
 s=block(s,'    public async Task<ApiResponse<bool>> ChangeStatusAsync','    public async Task<ApiResponse<List<FileStatusDto>>> GetAllowedStatusTransitionsAsync',`    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, int newStatusId, string? reason, int userId)
    {
        if (newStatusId == FileStatusConstants.Pending)
            return await _reviewService.SubmitReviewAsync(id, userId);
        if (newStatusId != FileStatusConstants.EntitlementEnded)
            return ApiResponse<bool>.Fail("التفعيل يتم تلقائياً باعتماد المراجعة؛ الإيقاف يخص المعاش فقط");
        return await Repositories.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await _unitOfWork.LockFileReviewAsync(id);
            var file = await _unitOfWork.RetirementFiles.GetByIdAsync(id);
            if (file == null || file.IsDeleted) return ApiResponse<bool>.Fail("الملف غير موجود");
            await _unitOfWork.RetirementFiles.ReloadAsync(file);
            if (file.StatusID != FileStatusConstants.Active || file.ReviewStatus != FileReviewStatus.Approved)
                return ApiResponse<bool>.Fail("يجب اعتماد المراجعة قبل إنهاء استحقاق الملف");
            var sod = await _sodGuard.ValidateAsync(id, userId, SoDAction.Approve);
            if (!sod.IsAllowed) return ApiResponse<bool>.Fail(sod.Reason ?? "تعارض فصل المهام");
            var now = DateTime.UtcNow;
            if (await _unitOfWork.FinancialEntitlements.Query().AnyAsync(e => e.RetireeID == file.RetireeID &&
                !e.IsDeleted && (e.EndDate == null || e.EndDate >= now)))
                return ApiResponse<bool>.Fail("لا يمكن إنهاء الملف ما دامت هناك استحقاقات مالية لم تنتهِ");
            file.StatusID = FileStatusConstants.EntitlementEnded;
            file.ClosedDate = now;
            file.StatusReason = reason;
            file.UpdatedAt = now;
            file.UpdatedByUserID = userId;
            await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
            {
                TableID = SystemTableConstants.RetirementFiles, RecordID = id,
                OldStatusID = FileStatusConstants.Active, NewStatusID = FileStatusConstants.EntitlementEnded,
                ChangedByUserID = userId, Reason = reason, TransitionDate = now
            });
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync(userId, "", "EntitlementEnded", "RetirementFile", id, "2", "4");
            return ApiResponse<bool>.Success(true, "انتهت جميع استحقاقات الملف مع الاحتفاظ ببياناته وسجله");
        });
    }

`);
 s=s.replace('            allowedTransitions.RemoveWhere(statusId => statusId != FileStatusConstants.UnderReview);','            allowedTransitions.Clear();');
 s=s.replace(/StatusName = s\.StatusID == FileStatusConstants\.UnderReview\s*\? "قيد المراجعة"\s*: s\.StatusName,/g, 'StatusName = FileStatusConstants.Name(s.StatusID),');
 s=block(s,'        var allowedIds = new[]','        var allStatuses =',`        var allowedIds = new[] { FileStatusConstants.New, FileStatusConstants.Active,
            FileStatusConstants.Pending, FileStatusConstants.EntitlementEnded };

`);
 s=s.replace('p.RetirementFile.StatusID == FileStatusConstants.Active','(p.RetirementFile.StatusID == FileStatusConstants.Active || p.RetirementFile.StatusID == FileStatusConstants.Pending)');
 s=block(s,'            // تحديث حالة الملفات إلى موقوف','        }\r\n\r\n        await _unitOfWork.SaveChangesAsync();','');
 s=s.replace('إيقاف الملف - انتهاء تعهد','إيقاف المعاش - انتهاء تعهد').replace('تم إيقاف الملف رقم','تم إيقاف معاش الملف رقم');
 s=block(s,'    internal static HashSet<int> GetAllowedTransitions','    /// <summary>\r\n    /// جلب أنواع الوثائق',`    internal static HashSet<int> GetAllowedTransitions(int currentStatus) => currentStatus switch
    {
        FileStatusConstants.New => new() { FileStatusConstants.Pending },
        FileStatusConstants.Active => new() { FileStatusConstants.EntitlementEnded },
        _ => new()
    };

`);
 return s;
});
edit('PensionMS.Infrastructure/Services/Pension/Entitlements/PensionSuspensionService.cs',s=>{
 s=block(s,'        // Check for existing active suspension','        // 1. Create Suspension',`        if (dto.SuspensionSource == RetirementReviewConstants.SuspensionSource)
            return ApiResponse<SuspensionDto>.Fail("إيقاف المراجعة يديره مسار المراجعة تلقائياً");

`);
 s=block(s,'        // 3. Update file status to Suspended','        await SendFileNotificationAsync(', '');
 s=s.replace('        // End suspension',`        if (suspension.SuspensionSource == RetirementReviewConstants.SuspensionSource)
            return ApiResponse<bool>.Fail("لا ينتهي إيقاف المراجعة إلا باعتماد مراجعة الملف");

        // End only this pension hold; other holds and file review remain unchanged.`);
 s=block(s,'        // Reactivate file — restore previous status if available','        await SendFileNotificationAsync(',`        var file = suspension.RetirementFile;
        await _unitOfWork.SaveChangesAsync();

`);
 s=s.replaceAll('تم إنهاء الإيقاف وإعادة تفعيل الملف', 'تم إنهاء الإيقاف المحدد للمعاش');
 return s;
});
edit('PensionMS.Infrastructure/Helpers/PensionFileAccess.cs',s=>{
 s=s.replace(/public static bool Editable\(RetirementFile file\) =>[\s\S]*?;/,'public static bool Editable(RetirementFile file) => !file.IsDeleted && FileStatusConstants.IsCurrent(file.StatusID ?? 0);');return s;
});
edit('PensionMS.Infrastructure/Services/Pension/Files/WorkflowEngine.cs',s=>{
 s=s.replace(/file.StatusID == FileStatusConstants.New \|\|\s*file.StatusID == FileStatusConstants.Incomplete \|\|\s*file.StatusID == FileStatusConstants.Pending/g,'file.StatusID == FileStatusConstants.New');
 return s.replaceAll('FileStatusConstants.UnderReview','FileStatusConstants.Pending');
});
