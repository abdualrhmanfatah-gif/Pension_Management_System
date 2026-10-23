import fs from 'node:fs';
const root='F:/Projects/Pension_Management_System/';
function edit(path,apply){const s=fs.readFileSync(root+path,'utf8'),backup=root+'plan/retirement-lifecycle-implementation/before/'+path;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,s);}fs.writeFileSync(root+path,apply(s).replace(/\r?\n/g,'\r\n'));}
function block(s,begin,end,next){const a=s.indexOf(begin),b=s.indexOf(end,a+begin.length);if(a<0||b<0)throw new Error('Missing '+begin);return s.slice(0,a)+next+s.slice(b);}
edit('PensionMS.Infrastructure/Services/Payroll/PayrollGenerationService.cs',s=>{
 s='using PensionMS.Infrastructure.Services.Pension.Files;\n'+s;
 s=s.replace('var onDate = YemenTime.LocalDate(new DateTimeOffset(DateTime.SpecifyKind(runAt, DateTimeKind.Utc)));',`var onDate = batch.BatchMonth == default
            ? YemenTime.LocalDate(new DateTimeOffset(DateTime.SpecifyKind(runAt, DateTimeKind.Utc)))
            : DateOnly.FromDateTime(batch.BatchMonth);
        var periodEnd = onDate.ToDateTime(TimeOnly.MaxValue);`);
 s=s.replace('f.StatusID == FileStatusConstants.Active && !f.IsDeleted','(f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Pending && f.PreviousStatusID != FileStatusConstants.EntitlementEnded) && !f.IsDeleted');
 s=s.replace('s.Status == SuspensionStatusConstants.Active && !s.IsDeleted', '(s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive) && !s.IsDeleted');
 s=s.replace('r.IsActive && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected && fileIds.Contains(r.FileID)','r.IsActive && !r.IsDeleted && fileIds.Contains(r.FileID)');
 s=s.replace('var runDate = YemenTime.LocalDate(new DateTimeOffset(DateTime.SpecifyKind(runAt, DateTimeKind.Utc))).ToDateTime(TimeOnly.MinValue);','var runDate = onDate.ToDateTime(TimeOnly.MinValue);');
 s=s.replace('e.EffectiveDate <= runAt','e.EffectiveDate <= periodEnd');
 s=s.replace(' && b.PendingReviewStatus != PendingReviewStatusConstants.PendingReview && b.PendingReviewStatus != PendingReviewStatusConstants.Rejected','');
 s=block(s,'            // 2. Skip if suspended','            // 3. Skip if no active payment book',`            // Suspension prevents payment, never accrual. Use approved data during review.
            var approved = ApprovedPensionState.Read(file.LastReviewedFileState);
            if (approved != null)
            {
                file.Retiree = approved.Retiree;
                var book = approved.Books.Where(b => b.Status == PaymentBookStatus.Active && !b.IsDeleted && b.IssueDate <= onDate && b.ExpiryDate >= onDate)
                    .OrderByDescending(b => b.CreatedAt).FirstOrDefault();
                if (book != null) booksLookup[file.FileID] = book;
                else booksLookup.Remove(file.FileID);
                var financial = approved.Entitlements.Where(e => !e.IsDeleted && e.EffectiveDate <= periodEnd &&
                    (e.EndDate == null || e.EndDate.Value.Date >= runDate)).OrderByDescending(e => e.EffectiveDate).ThenByDescending(e => e.EntitlementID).FirstOrDefault();
                if (financial != null) entitlementLookup[file.RetireeID] = financial;
                else entitlementLookup.Remove(file.RetireeID);
            }

`);
 s=s.replace('var recipients = recipientsLookup[file.FileID].OrderBy(r => r.RecipientID).ToList();','var recipients = (approved?.Recipients ?? recipientsLookup[file.FileID].ToList()).Where(r => r.IsActive && !r.IsDeleted).OrderBy(r => r.RecipientID).ToList();');
 s=s.replace('var required = entitledLookup[file.FileID].ToList();','var required = (approved?.EntitledPersons ?? entitledLookup[file.FileID].ToList()).Where(e => e.IsActive && !e.IsDeleted && !e.IsDeceased).ToList();\n            var fileGuardianLinks = approved?.GuardianLinks.Where(g => g.IsActive).Select(g => (g.GuardianID, g.EntitledID)).ToList()\n                ?? guardianLinks.Select(g => (g.GuardianID, g.EntitledID)).ToList();');
 s=s.replace('!guardianLinks.Any(', '!fileGuardianLinks.Any(');
 return s;
});
edit('PensionMS.Infrastructure/Services/Payroll/SalaryPaymentBatchService.cs',s=>{
 s='using PensionMS.Infrastructure.Services.Pension.Files;\n'+s;
 s=s.replace('        item.StatusID = dto.StatusID;',`        if (await PensionPaymentEligibility.IsHeldAsync(_unitOfWork, item.FileID))
            return ApiResponse<bool>.Fail("صرف المعاش موقوف حتى اعتماد المراجعة وزوال جميع الإيقافات");
        if (item.StatusID == TransactionStatusConstants.Received)
            return ApiResponse<bool>.Fail("لا يمكن تغيير بند سبق استلامه");
        item.StatusID = dto.StatusID;`);
 const exportStart=s.indexOf('    public async Task<byte[]> ExportBatchToExcelAsync');
 const exportWhere=s.indexOf('.Where(i => i.BatchID == batchId && !i.IsDeleted)',exportStart);
 s=s.slice(0,exportWhere)+s.slice(exportWhere).replace('.Where(i => i.BatchID == batchId && !i.IsDeleted)',`.Where(i => i.BatchID == batchId && !i.IsDeleted &&
                i.StatusID == TransactionStatusConstants.PendingSend && i.RetirementFile.StatusID == FileStatusConstants.Active &&
                !i.RetirementFile.PensionSuspensions.Any(h => !h.IsDeleted &&
                    (h.Status == SuspensionStatusConstants.Active || h.Status == SuspensionStatusConstants.CodeActive)))`);
 s=s.replace('s.Status == SuspensionStatusConstants.CodeActive && !s.IsDeleted','(s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive) && !s.IsDeleted');
 s=s.replace('            // Match status (Fuzzy matching)',`            if (await PensionPaymentEligibility.IsHeldAsync(_unitOfWork, item.FileID) || item.StatusID == TransactionStatusConstants.Received)
            {
                result.Errors.Add($"سطر {row}: صرف البند موقوف أو سبق استلامه");
                result.SkippedCount++;
                continue;
            }

            // Match status (Fuzzy matching)`);
 s=s.replace('SalaryStatus = (i.RetirementFile?.PensionSuspensions.Any(s => s.Status == SuspensionStatusConstants.Active && !s.IsDeleted) ?? false) ? "موقوف" : "نشط"',`SalaryStatus = (i.RetirementFile?.StatusID != FileStatusConstants.Active ||
                (i.RetirementFile.PensionSuspensions.Any(s => (s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive) && !s.IsDeleted))) ? "موقوف" : "قابل للصرف"`);
 // Refuse regeneration when any item has already entered bank processing.
 s=s.replace('            foreach (var item in existingItems)\r\n            {',`            if (existingItems.Any(i => i.StatusID != TransactionStatusConstants.PendingSend || i.SendDate.HasValue || i.ReceiveDate.HasValue))
            {
                if (ownsTransaction) await _unitOfWork.RollbackAsync();
                return ApiResponse<GenerateItemsResultDto>.Fail("لا يمكن إعادة توليد كشف بدأ إرسال أو استلام بنوده");
            }
            foreach (var item in existingItems)
            {`);
 return s;
});
