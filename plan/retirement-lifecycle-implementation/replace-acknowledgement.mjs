import fs from 'node:fs';
const p='PensionMS.Infrastructure/Services/Pension/Files/FileReviewService.cs';let s=fs.readFileSync(p,'utf8');
const a=s.indexOf('    public async Task<ApiResponse<bool>> AcknowledgeEntityRejectionAsync('),b=s.indexOf('    private static string GetArabicFieldName(',a);
if(a<0||b<0)throw new Error('acknowledgement anchors');
s=s.slice(0,a)+`    // Compatibility endpoint: resubmit for review; never clear rejection or restore data implicitly.
    public async Task<ApiResponse<bool>> AcknowledgeEntityRejectionAsync(string entityType, int entityId, int userId)
    {
        var fileIds = entityType switch
        {
            "EntitledPerson" => await _unitOfWork.EntitledPersons.Query().Where(e => e.EntitledID == entityId).Select(e => e.FileID).ToListAsync(),
            "Agent" => await _unitOfWork.Agents.Query().Where(e => e.AgentID == entityId).Select(e => e.FileID).ToListAsync(),
            "Guardian" => await _unitOfWork.Guardians.Query().Where(e => e.GuardianID == entityId).Select(e => e.FileID).ToListAsync(),
            "PaymentBook" => await _unitOfWork.PaymentBooks.Query().Where(e => e.PaymentBookID == entityId).Select(e => e.FileID).ToListAsync(),
            "SalaryRecipient" => await _unitOfWork.SalaryRecipients.Query().Where(e => e.RecipientID == entityId).Select(e => e.FileID).ToListAsync(),
            "Retiree" => await Helpers.PensionFileAccess.Files(_unitOfWork, _branchScope).Where(f => f.RetireeID == entityId).Select(f => f.FileID).ToListAsync(),
            _ => new List<int>()
        };
        if (fileIds.Count == 0) return ApiResponse<bool>.Fail("السجل غير موجود أو نوعه غير مدعوم");
        return await Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            foreach (var fileId in fileIds.Distinct().Order())
            {
                var result = await SubmitReviewAsync(fileId, userId);
                if (!result.IsSuccess) return result;
            }
            return ApiResponse<bool>.Success(true, "تم إرسال البيانات للمراجعة؛ يبقى الصرف موقوفًا حتى الاعتماد");
        });
    }

`+s.slice(b);
const start=s.indexOf('    private async Task SetEntityReviewStatusForFileAsync('),end=s.indexOf('    // Compatibility endpoint',start);
let method=s.slice(start,end).replace(/\r?\n\s*&& [a-z]+\.PendingReviewStatus == (?:PendingReviewStatus\.Pending|PendingReviewStatusConstants\.PendingReview)/g,'');
method=method.replace('            && file.Retiree.PendingReviewStatus == PendingReviewStatus.Pending','');
s=s.slice(0,start)+method+s.slice(end);
fs.writeFileSync(p,s.replace(/\r?\n/g,'\r\n'));
for(const name of ['AgentsTab','GuardiansTab','PaymentBooksTab','EntitledPersonsTab']){
const target='ui/src/pages/retirement-files/tabs/'+name+'.tsx';let ui=fs.readFileSync(target,'utf8');ui=ui.replaceAll('قبول الرفض','إعادة المراجعة').replaceAll('تم قبول الرفض','تمت إعادة المراجعة');fs.writeFileSync(target,ui);}
