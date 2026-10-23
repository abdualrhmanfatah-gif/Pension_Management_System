import fs from 'node:fs';
const root = 'F:/Projects/Pension_Management_System/';
function edit(path, apply) {
  const original = fs.readFileSync(root + path, 'utf8');
  const backup = root + 'plan/retirement-lifecycle-implementation/before/' + path;
  if (!fs.existsSync(backup)) { fs.mkdirSync(backup.slice(0, backup.lastIndexOf('/')), { recursive: true }); fs.writeFileSync(backup, original); }
  fs.writeFileSync(root + path, apply(original).replace(/\r?\n/g, '\r\n'));
}
function replace(s, old, next) { if (!s.includes(old)) throw new Error('Missing anchor: '+old.slice(0,100)); return s.replace(old, next); }
function block(s, begin, end, next) { const start=s.indexOf(begin),finish=s.indexOf(end,start+begin.length); if(start<0||finish<0)throw new Error('Missing block '+begin);return s.slice(0,start)+next+s.slice(finish); }
edit('PensionMS.Core/Interfaces/IFileReviewService.cs', s => replace(s,
'    Task<ApiResponse<bool>> RequestReviewAsync',
`    Task<ApiResponse<bool>> SubmitReviewAsync(int fileId, int userId);
    Task<ApiResponse<bool>> ReinstateEntitlementAsync(int fileId, int userId);
    Task<ApiResponse<bool>> RequestReviewAsync`));
edit('PensionMS.Infrastructure/Services/Pension/Files/FileReviewService.cs',s=>{
 s=s.replace('    private readonly IUnitOfWork _unitOfWork;', '    private readonly IUnitOfWork _unitOfWork;\n    private readonly ISoDGuard _sodGuard;');
 s=s.replace('public FileReviewService(IUnitOfWork unitOfWork)', 'public FileReviewService(IUnitOfWork unitOfWork, ISoDGuard sodGuard)');
 s=s.replace('        _unitOfWork = unitOfWork;', '        _unitOfWork = unitOfWork;\n        _sodGuard = sodGuard;');
 s=s.replace('f.ReviewStatus == FileReviewStatus.PendingReview)', 'f.StatusID == FileStatusConstants.Pending)');
 s=s.replace('.Where(i => i.RetirementFileID == fileId)\r\n            .OrderBy', '.Where(i => i.RetirementFileID == fileId && i.CreatedAt >= file.ReviewRequestedAt)\r\n            .OrderBy');
 s=block(s,'    public Task<ApiResponse<bool>> RequestReviewAsync','    public Task<ApiResponse<bool>> ApproveReviewAsync',`    // Mutation callers are idempotent; initial preparation only leaves New on explicit submission.
    public Task<ApiResponse<bool>> RequestReviewAsync(int fileId, int userId, DateTime? requestedAt = null)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, false, false));

    public Task<ApiResponse<bool>> SubmitReviewAsync(int fileId, int userId)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, true, false));

    public Task<ApiResponse<bool>> ReinstateEntitlementAsync(int fileId, int userId)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, true, true));

    private async Task<ApiResponse<bool>> RequestReviewCoreAsync(int fileId, int userId, bool submit, bool reinstate)
    {
        var file = await _unitOfWork.RetirementFiles.Query().Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted) return ApiResponse<bool>.Fail("الملف غير موجود");
        if (reinstate && file.StatusID != FileStatusConstants.EntitlementEnded)
            return ApiResponse<bool>.Fail("إعادة الاستحقاق متاحة للملف الذي انتهى استحقاقه فقط");
        if (!submit && file.StatusID == FileStatusConstants.New && file.ReviewStatus == FileReviewStatus.None)
            return ApiResponse<bool>.Success(true, "الملف جديد؛ أرسله للمراجعة بعد استكمال التجهيز");
        if (file.StatusID == FileStatusConstants.Pending && file.ReviewStatus == FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Success(true, "الملف بانتظار المراجعة");
        var now = DateTime.UtcNow;
        if (file.StatusID != FileStatusConstants.Pending)
            file.PreviousStatusID = reinstate ? FileStatusConstants.Active : file.StatusID;
        file.StatusID = FileStatusConstants.Pending;
        file.ReviewStatus = FileReviewStatus.PendingReview;
        file.ReviewRequestedAt = now;
        file.ReviewNotes = null;
        file.UpdatedAt = now;
        var state = await ApprovedPensionState.CaptureAsync(_unitOfWork, fileId, file.RetireeID);
        await _unitOfWork.FileReviewItems.AddAsync(new FileReviewItem
        {
            RetirementFileID = fileId, EntityType = SystemTableConstants.RetirementFiles,
            EntityID = fileId, SectionName = "مراجعة بيانات الملف", SectionType = "File",
            FieldName = reinstate ? "إعادة الاستحقاق" : "إرسال للمراجعة",
            NewValue = "مراجعة جميع بيانات الملف والوثائق المرتبطة به",
            FieldChangesJson = JsonSerializer.Serialize(new[] { new { fieldName = "بيانات الملف الحالية", oldValue = (string?)null, newValue = state.Serialize() } }),
            CreatedAt = now
        });
        if (!await _unitOfWork.PensionSuspensions.Query().AnyAsync(s => s.FileID == fileId && !s.IsDeleted &&
            s.SuspensionSource == RetirementReviewConstants.SuspensionSource && s.Status == SuspensionStatusConstants.Active))
            await _unitOfWork.PensionSuspensions.AddAsync(new PensionSuspension
            {
                FileID = fileId, CreatedByUserID = userId, SuspensionDate = now, CreatedAt = now,
                SuspensionSource = RetirementReviewConstants.SuspensionSource,
                SuspensionReason = RetirementReviewConstants.SuspensionReason, Status = SuspensionStatusConstants.Active
            });
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم إرسال الملف للمراجعة وإيقاف صرف المعاش");
    }

`);
 // All current-cycle decisions exclude history. Historical rejection cannot poison a new cycle.
 s=s.replaceAll('i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Pending',
  'i.RetirementFileID == fileId && i.CreatedAt >= file.ReviewRequestedAt && i.Status == FileReviewItemStatusConstants.Pending');
 s=s.replaceAll('i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Rejected',
  'i.RetirementFileID == fileId && i.CreatedAt >= file.ReviewRequestedAt && i.Status == FileReviewItemStatusConstants.Rejected');
 const approvalAnchor='        var pendingItems = await _unitOfWork.FileReviewItems.Query()';
 const start=s.indexOf('    private async Task<ApiResponse<bool>> ApproveReviewCoreAsync');
 const insert=s.indexOf(approvalAnchor,start);
 s=s.slice(0,insert)+`        var sod = await _sodGuard.ValidateAsync(fileId, userId, SoDAction.Approve);
        if (!sod.IsAllowed) return ApiResponse<bool>.Fail(sod.Reason ?? "تعارض فصل المهام");
        var activationError = file.PreviousStatusID == FileStatusConstants.EntitlementEnded ? null
            : await FileActivationGuard.ValidateAsync(_unitOfWork, fileId);
        if (activationError != null) return ApiResponse<bool>.Fail(activationError);

`+s.slice(insert);
 s=s.replace('        if (item.Status != FileReviewItemStatusConstants.Pending)', '        if (item.CreatedAt < file.ReviewRequestedAt)\n            return ApiResponse<bool>.Fail("هذا البند من مراجعة سابقة؛ حدّث الصفحة");\n\n        if (item.Status != FileReviewItemStatusConstants.Pending)');
 const itemStart=s.indexOf('    private async Task<ApiResponse<bool>> ApproveItemCoreAsync');
 const itemInsert=s.indexOf('        item.Status = FileReviewItemStatusConstants.Approved;',itemStart);
 s=s.slice(0,itemInsert)+`        var sod = await _sodGuard.ValidateAsync(item.RetirementFileID, userId, SoDAction.Approve);
        if (!sod.IsAllowed) return ApiResponse<bool>.Fail(sod.Reason ?? "تعارض فصل المهام");
        var activationError = file.PreviousStatusID == FileStatusConstants.EntitlementEnded ? null
            : await FileActivationGuard.ValidateAsync(_unitOfWork, item.RetirementFileID);
        if (activationError != null) return ApiResponse<bool>.Fail(activationError);

`+s.slice(itemInsert);
 s=block(s,'        // ── ربط حالة الملفات بنتيجة المراجعة ──','        _unitOfWork.RetirementFiles.Update(file);',`        file.StatusID = rejectedCount > 0 ? FileStatusConstants.Pending
            : file.PreviousStatusID == FileStatusConstants.EntitlementEnded
                ? FileStatusConstants.EntitlementEnded : FileStatusConstants.Active;
        if (rejectedCount == 0)
        {
            file.LastReviewedFileState = (await ApprovedPensionState.CaptureAsync(_unitOfWork, fileId, file.RetireeID)).Serialize();
            file.PreviousStatusID = null;
            var reviewHolds = await _unitOfWork.PensionSuspensions.Query().Where(s => s.FileID == fileId && !s.IsDeleted &&
                s.Status == SuspensionStatusConstants.Active && s.SuspensionSource == RetirementReviewConstants.SuspensionSource).ToListAsync();
            foreach (var hold in reviewHolds)
            {
                hold.Status = SuspensionStatusConstants.Lifted;
                hold.EndDate = now;
                hold.EndReason = "اعتماد مراجعة بيانات الملف";
            }
        }

`);
 // Per-item decisions do not unlock or mark the underlying entity approved while other items remain.
 s=s.replace('        await UpdateEntityPendingStatusForItemAsync(item, (int)PendingReviewStatus.None);','');
 return s;
});
edit('PensionMS.API/Controllers/RetirementFilesController.cs',s=>s.replace('await _reviewService.RequestReviewAsync(id, userId)', 'await _reviewService.SubmitReviewAsync(id, userId)'));
