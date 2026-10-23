import fs from 'node:fs';
const p='PensionMS.Infrastructure/Services/Pension/Files/FileReviewService.cs';
let s=fs.readFileSync(p,'utf8');
// Keep historical decisions; derive the working set from the newest row for each record.
s=s.replaceAll(`var pendingItems = await _unitOfWork.FileReviewItems.Query()
            .Where(i => i.RetirementFileID == fileId && i.CreatedAt >= file.ReviewRequestedAt && i.Status == FileReviewItemStatusConstants.Pending)
            .ToListAsync();`.replaceAll('\n','\r\n'),`var pendingItems = (await FileReviewCycle.LoadCurrentAsync(_unitOfWork.FileReviewItems.Query(), file))
            .Where(i => i.Status == FileReviewItemStatusConstants.Pending).ToList();`);
s=s.replaceAll('if (item.CreatedAt < file.ReviewRequestedAt)', 'if (!(await FileReviewCycle.LoadCurrentAsync(_unitOfWork.FileReviewItems.Query(), file)).Any(i => i.ItemID == itemId))');
const rejectStart=s.indexOf('    private async Task<ApiResponse<bool>> RejectItemCoreAsync');
const rejectInsert=s.indexOf('        if (item.Status != FileReviewItemStatusConstants.Pending)',rejectStart);
s=s.slice(0,rejectInsert)+`        if (!(await FileReviewCycle.LoadCurrentAsync(_unitOfWork.FileReviewItems.Query(), file)).Any(i => i.ItemID == itemId))
            return ApiResponse<bool>.Fail("هذا البند من مراجعة سابقة؛ حدّث الصفحة");

`+s.slice(rejectInsert);
const a=s.indexOf('        var pendingCount =',s.indexOf('    private async Task AutoResolveFileReviewCoreAsync'));
const b=s.indexOf('        var now = DateTime.UtcNow;',a);
if(a<0||b<0)throw new Error('Resolve anchor');
s=s.slice(0,a)+`        if (file.ReviewStatus != FileReviewStatus.PendingReview) return;
        var currentItems = await FileReviewCycle.LoadCurrentAsync(_unitOfWork.FileReviewItems.Query(), file);
        if (currentItems.Count == 0 || currentItems.Any(i => i.Status == FileReviewItemStatusConstants.Pending)) return;
        var rejectedCount = currentItems.Count(i => i.Status == FileReviewItemStatusConstants.Rejected);

`+s.slice(b);
// The old audit-reconstruction path and its mutation helpers are superseded by the save pipeline.
function remove(begin,end){const start=s.indexOf(begin),finish=s.indexOf(end,start+begin.length);if(start<0||finish<0)throw new Error(begin);s=s.slice(0,start)+s.slice(finish);}
remove('    private async Task MarkAffectedEntitiesAsPendingAsync(', '    private async Task SetEntityReviewStatusForFileAsync(');
remove('    private async Task UpdateEntityPendingStatusForItemAsync(', '    private static string GetArabicFieldName(');
s=s.replace('        await UpdateEntityPendingStatusForItemAsync(item, (int)PendingReviewStatus.Rejected);','');
s=s.replace('        // إعادة تعيين PendingReviewStatus للكيان بعد الموافقة على التغيير\r\n\r\n','');
// Rejecting a review never changes approved data back implicitly.
s=s.replace('        // تعليم الكيان بأنه مرفوض\r\n','');
fs.writeFileSync(p,s.replace(/\r?\n/g,'\r\n'));
