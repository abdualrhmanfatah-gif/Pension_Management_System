import fs from 'node:fs';
const root='F:/Projects/Pension_Management_System/';
function edit(path,apply){const s=fs.readFileSync(root+path,'utf8'),backup=root+'plan/retirement-lifecycle-implementation/before/'+path;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,s);}fs.writeFileSync(root+path,apply(s).replace(/\r?\n/g,'\r\n'));}
edit('PensionMS.Infrastructure/Services/Pension/Files/RetirementFileService.cs',s=>{
 // Reinstatement happens on the same file, never by opening another file for that retiree.
 s=s.replace('&& f.StatusID != FileStatusConstants.Closed','');
 s=s.replaceAll('FileStatusConstants.Closed','FileStatusConstants.EntitlementEnded');
 s=s.replaceAll('FileStatusConstants.UnderReview','FileStatusConstants.Pending');
 s=s.replaceAll('file.StatusID = FileStatusConstants.Suspended;', 'file.StatusID = FileStatusConstants.Pending;');
 s=s.replaceAll('NewStatusID = FileStatusConstants.Suspended,', 'NewStatusID = FileStatusConstants.Pending,');
 s=s.replace('file.PreviousStatusID = FileStatusConstants.Suspended;', 'file.PreviousStatusID = oldFileStatus;');
 s=s.replace(/        if \(file.StatusID == FileStatusConstants.EntitlementEnded\)\r?\n            return ApiResponse<bool>.Fail\("لا يمكن تغيير حالة الحياة لملف مغلق"\);\r?\n/g,'');
 s=s.replace(/        if \(file.StatusID == FileStatusConstants.EntitlementEnded\)\r?\n            return ApiResponse<RetirementFileDto>.Fail\([^\n]*\);\r?\n/g,'');
 return s;
});
edit('PensionMS.Infrastructure/Services/Pension/Beneficiaries/GuardianService.cs',s=>s.replace('f.StatusID != FileStatusConstants.Closed && f.StatusID != FileStatusConstants.Suspended && f.StatusID != FileStatusConstants.Rejected','FileStatusConstants.IsCurrent(f.StatusID ?? 0)'));
// SQL-translated query cannot call our CLR status helper.
edit('PensionMS.Infrastructure/Services/Pension/Beneficiaries/GuardianService.cs',s=>s.replace('FileStatusConstants.IsCurrent(f.StatusID ?? 0)','(f.StatusID == FileStatusConstants.New || f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Pending || f.StatusID == FileStatusConstants.EntitlementEnded)'));
edit('PensionMS.Infrastructure/Services/Pension/Files/FileQueryService.cs',s=>s.replaceAll('FileStatusConstants.Closed','FileStatusConstants.EntitlementEnded')
 .replace('f.StatusID == FileStatusConstants.UnderReview ||\r\n                    ','')
 .replace('f.StatusID == FileStatusConstants.Rejected','f.ReviewStatus == FileReviewStatus.Rejected'));
edit('PensionMS.API/Controllers/RetirementFilesController.cs',s=>s.replace('    [HttpPost("{id:int}/review/approve")]',`    [HttpPost("{id:int}/reinstate-entitlement")]
    [Authorize(Policy = "File.Edit")]
    public async Task<IActionResult> ReinstateEntitlement(int id)
    {
        var result = await _reviewService.ReinstateEntitlementAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/review/approve")]`));
edit('PensionMS.Infrastructure/Data/Configurations/Lookups/FileStatusConfiguration.cs',s=>{
 s=s.replace('StatusName = "مغلق"','StatusName = "انتهى استحقاقه"');
 return s.split('\n').filter(l=>!['StatusID = 3,','StatusID = 5,','StatusID = 7,','StatusID = 8,'].some(v=>l.includes(v)))
  .join('\n').replace(/(new FileStatus \{ StatusID = 6,[^\n]*\}),\r?\n/, '$1\n');
});
