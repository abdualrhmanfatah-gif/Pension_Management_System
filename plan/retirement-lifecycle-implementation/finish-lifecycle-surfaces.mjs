import fs from 'node:fs';
function edit(p,fn){const before=fs.readFileSync(p,'utf8'),backup='plan/retirement-lifecycle-implementation/before/'+p;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,before);}fs.writeFileSync(p,fn(before).replace(/\r?\n/g,'\r\n'));}
edit('PensionMS.Infrastructure/Services/Reporting/DashboardService.cs',s=>s
 .replace('(f.StatusID == FileStatusConstants.New || f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Incomplete)','f.StatusID == FileStatusConstants.Active')
 .replace('f.StatusID == FileStatusConstants.Suspended','f.StatusID == FileStatusConstants.EntitlementEnded')
 .replace('f.StatusID == FileStatusConstants.Closed','f.ReviewStatus == FileReviewStatus.Rejected'));
edit('PensionMS.Infrastructure/Services/Reporting/DashboardService.Details.cs',s=>{
 s=s.replaceAll('f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.New','f.StatusID == FileStatusConstants.Active');
 s=s.replaceAll('f.StatusID == FileStatusConstants.UnderReview || f.StatusID == FileStatusConstants.Pending','f.StatusID == FileStatusConstants.Pending');
 s=s.replaceAll('FileStatusConstants.Closed','FileStatusConstants.EntitlementEnded').replaceAll('"مغلق"','"انتهى استحقاقه"');
 s=s.replace('var suspendedFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.Suspended);',`var visibleFileIds = allFiles.Select(f => f.FileID).ToList();
        var suspendedFiles = await _unitOfWork.PensionSuspensions.Query().AsNoTracking()
            .Where(s => visibleFileIds.Contains(s.FileID) && !s.IsDeleted &&
                (s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive))
            .Select(s => s.FileID).Distinct().CountAsync();`);
 return s.split('\n').filter(l=>!['{ FileStatusConstants.Suspended,','{ FileStatusConstants.Incomplete,','{ FileStatusConstants.UnderReview,','{ FileStatusConstants.Rejected,'].some(t=>l.includes(t))).join('\n');
});
edit('ui/src/pages/pension/FileReviewListPage.tsx',s=>s.replace(/reviewStatus === ([012])/g,(_,id)=>'reviewStatus === '+(Number(id)+1)));
edit('ui/src/pages/retirement-files/tabs/ReviewTab.tsx',s=>s.replace('if (ok) rejectItemMutation.mutate({ itemId: change.itemId, reason: "" });','if (ok && reviewNotes.trim()) rejectItemMutation.mutate({ itemId: change.itemId, reason: reviewNotes.trim() });'));
edit('ui/src/components/domain/retirement/CreateFileWizard.tsx',s=>s.replaceAll('ملف نشط','ملف موجود'));
edit('PensionMS.Infrastructure/Services/Pension/Files/RetirementFileService.cs',s=>s.replace('        // ── Phase 1.4: منع تعديل ملف مغلق ──\r\n\r\n',''));
// Replacing media must keep the approved file available to review/history.
edit('PensionMS.Infrastructure/Services/Documents/DocumentService.cs',s=>s.replace(/        \/\/ حذف الملف القديم بعد نجاح التثبيت\r?\n        if \(oldFilePath != null && oldFilePath != doc.FilePath\)\r?\n        \{[\s\S]*?\r?\n        \}/,'        // الاحتفاظ بالأصل المعتمد ضمن سجل المراجعة بدلاً من حذفه عند التعديل.'));
edit('PensionMS.Infrastructure/Services/Pension/Beneficiaries/RetireeService.cs',s=>s.replace('                await _fileStorage.DeleteFileAsync(retiree.PersonalPhotoPath);','                // الاحتفاظ بالصورة السابقة لأغراض المراجعة والسجل التاريخي.'));
