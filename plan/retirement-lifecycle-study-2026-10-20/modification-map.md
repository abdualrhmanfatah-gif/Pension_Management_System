# مواضع التغيير والعقود المتأثرة

الأرقام تشير إلى نسخة المصدر المحفوظة في source-manifest.json. CHANGE: سلوك يتطلب التغيير. CONTRACT: عقد أو اعتماد متأثر، لا يعني بالضرورة تعديل الملف نفسه.

## PensionMS.Core/Constants/FileStatusConstants.cs

- التصنيف: CHANGE
- القواعد: R01,R06
- المطلوب: تعريف الحالات الأربع والتوافق مع أرقام الحالات القديمة، دون خلط موقوف المعاش بحالة الملف.

| السطر | الموضع |
|---|---|
| 6 | `class FileStatusConstants` |
| 11 | `Closed =` |
| 13 | `Pending =` |
| 14 | `UnderReview =` |

## PensionMS.Infrastructure/Data/Configurations/Lookups/FileStatusConfiguration.cs

- التصنيف: CHANGE
- القواعد: R01,R10
- المطلوب: أسماء وقائمة حالات الملف الجديدة مع المحافظة على التاريخ في التحويل.

| السطر | الموضع |
|---|---|
| 17 | `builder.HasData(` |

## PensionMS.Infrastructure/Services/Pension/Files/FileStatusService.cs

- التصنيف: CHANGE
- القواعد: R01,R05,R06,R07
- المطلوب: تغيير مصفوفة الانتقالات، توحيد شروط الاعتماد والتفعيل، فصل انتهاء الاستحقاق والإيقافات؛ إزالة الكتابة لحالة Suspended.

| السطر | الموضع |
|---|---|
| 43 | `ChangeStatusAsync(` |
| 79 | `case FileStatusConstants.Active` |
| 207 | `GetAllowedStatusTransitionsAsync(` |
| 240 | `GetFileStatusesAsync(` |
| 277 | `ProcessExpiredPledgesAsync(` |
| 379 | `GetAllowedTransitions(int` |

## PensionMS.Infrastructure/Services/Pension/Files/FileReviewService.cs

- التصنيف: CHANGE
- القواعد: R02,R03,R04,R05,R06,R07,R09,R10,R11,R12
- المطلوب: تمييز التجهيز والتعديل والإرسال والمراجعة الانتقالية وإعادة الاستحقاق؛ حصر القرار بالدورة والبند؛ ربط التعليق بإيقاف المراجعة؛ الرفض يبقي معلقاً والاعتماد يفعّل بعد الفحوص.

| السطر | الموضع |
|---|---|
| 142 | `WithReviewTransactionAsync(` |
| 165 | `RequestReviewCoreAsync(` |
| 189 | `var existingAll =` |
| 262 | `var tableDefs =` |
| 418 | `ApproveReviewCoreAsync(` |
| 452 | `RejectReviewCoreAsync(` |
| 495 | `ApproveItemCoreAsync(` |
| 529 | `RejectItemCoreAsync(` |
| 571 | `AutoResolveFileReviewCoreAsync(int` |
| 681 | `SetEntityReviewStatusForFileAsync(int` |
| 729 | `AcknowledgeEntityRejectionAsync(` |
| 811 | `UpdateEntityPendingStatusForItemAsync(FileReviewItem` |

## PensionMS.Infrastructure/Services/Pension/Files/RetirementFileService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R06,R10,R11
- المطلوب: ضبط إنشاء الملف وإدخال بياناته الأولى، منع فتح ملف بديل لتجاوز إعادة الاستحقاق، مسارات الوفاة والتصحيح والتعديل والحذف والإسناد، ومعاملات الحفظ مع التعليق.

| السطر | الموضع |
|---|---|
| 63 | `CreateAsync(` |
| 66 | `existingActive =` |
| 201 | `StatusID = FileStatusConstants.New` |
| 412 | `HandleRetireeDeathAsync(` |
| 609 | `CorrectRetireeLifeStatusAsync(` |
| 797 | `UpdateAsync(` |
| 1011 | `AssignEmployeeAsync(` |
| 1046 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Pension/Entitlements/PensionSuspensionService.cs

- التصنيف: CHANGE
- القواعد: R07,R09
- المطلوب: دعم أسباب إيقاف مستقلة ومتزامنة؛ عدم تغيير حالة الملف عند إنشاء/إنهاء إيقاف؛ تمييز إيقاف المراجعة ورفع سببه فقط وإعادة تقييم المبالغ المحجوزة.

| السطر | الموضع |
|---|---|
| 29 | `CreateWithPledgeAsync(` |
| 36 | `existingSuspension =` |
| 103 | `file.StatusID = FileStatusConstants.Suspended` |
| 144 | `EndSuspensionAsync(` |
| 168 | `file.StatusID = file.PreviousStatusID` |
| 332 | `CreateExemptionAsync(` |

## PensionMS.Infrastructure/Services/Payroll/PayrollGenerationService.cs

- التصنيف: CHANGE
- القواعد: R08,R09
- المطلوب: فصل استحقاق إنشاء المبلغ عن السماح بصرفه؛ إنشاء البنود المستحقة الموقوفة؛ حساب الفترة المطلوبة من الكشف ومراعاة النسخة المعتمدة.

| السطر | الموضع |
|---|---|
| 25 | `GenerateForBatchAsync(` |
| 29 | `var onDate =` |
| 31 | `var activeFiles =` |
| 98 | `fileIdsWithSuspension.Contains` |
| 55 | `var recipientsByFile =` |
| 74 | `var allEntitlements =` |
| 86 | `var activeBooks =` |
| 219 | `StatusID = TransactionStatusConstants.PendingSend` |

## PensionMS.Infrastructure/Services/Payroll/SalaryPaymentBatchService.cs

- التصنيف: CHANGE
- القواعد: R08,R09,R11
- المطلوب: فحص الحجز في الاعتماد والتصدير وتغيير الحالة واستيراد النتائج؛ المحافظة على البنود المتراكمة وهويتها وعدم إعادة صرف السابق؛ مراجعة الإجماليات والقيد المحاسبي للكشف المختلط.

| السطر | الموضع |
|---|---|
| 157 | `ApproveCoreAsync(` |
| 249 | `GenerateItemsAsync(` |
| 361 | `UpdateItemStatusAsync(` |
| 390 | `ExportBatchToExcelAsync(` |
| 490 | `SuspensionStatusConstants.CodeActive` |
| 521 | `ImportTransactionResultsAsync(` |
| 331 | `MapItemToDto(` |

## PensionMS.Infrastructure/Services/Pension/Beneficiaries/RetireeService.cs

- التصنيف: CHANGE
- القواعد: R04,R11
- المطلوب: مسارات مباشرة لا تطلب المراجعة حالياً؛ إضافة سياق الملف محل المعاملة وحماية البيانات المشتركة والصور.

| السطر | الموضع |
|---|---|
| 195 | `UpdateAsync(` |
| 274 | `DeleteAsync(` |
| 465 | `UploadPhotoAsync(` |

## PensionMS.Infrastructure/Services/Pension/Beneficiaries/EntitledPersonService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R06,R11
- المطلوب: جميع عمليات المستحق بما فيها الإيقاف والوفاة والاستعادة، وآثارها على الوكيل والمستلم؛ حفظ التعديل والتعليق والإيقاف كوحدة متسقة.

| السطر | الموضع |
|---|---|
| 84 | `CreateAsync(` |
| 230 | `UpdateAsync(` |
| 202 | `ReEvaluateEntitlementStatusAsync(` |
| 381 | `DeleteAsync(` |
| 407 | `StopAsync(` |
| 517 | `ReactivateAsync(` |
| 657 | `RecordDeathAsync(` |
| 705 | `ManualActivateAsync(` |

## PensionMS.Infrastructure/Services/Pension/Beneficiaries/AgentService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: الحفاظ على قفل الملف الموجود، وفصل التجهيز الأولي عن المراجعة؛ التقاط الإضافة والحذف والصورة وآثار إنهاء الوكالة.

| السطر | الموضع |
|---|---|
| 42 | `WithinFileAsync` |
| 59 | `CreateCoreAsync(` |
| 62 | `UpdateCoreAsync(` |
| 65 | `DeactivateCoreAsync(` |
| 68 | `DeleteCoreAsync(` |
| 591 | `UpdatePhotoAsync(` |

## PensionMS.Infrastructure/Services/Pension/Beneficiaries/GuardianService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: تعديل الوصي المباشر بلا مراجعة، وربط/فك القاصر وتغيير مستلم الحصة؛ مراعاة المفتاح المركب للربط.

| السطر | الموضع |
|---|---|
| 41 | `CanEditAsync(` |
| 82 | `AssignCoreAsync(` |
| 218 | `EndGuardianshipAsync(` |
| 261 | `UpdateAsync(` |
| 303 | `AddMinorCoreAsync(` |
| 146 | `TransferSalaryToGuardianAsync(` |
| 244 | `ReturnSalaryToEntitledAsync(` |

## PensionMS.Infrastructure/Services/Payroll/PaymentBookService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: التجهيز الأولي، إلغاء/استبدال الدفتر القديم وآثاره، وتعليق متسق يشمل حذف الدفتر.

| السطر | الموضع |
|---|---|
| 193 | `CreateCoreAsync(` |
| 362 | `UpdateAsync(` |
| 389 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Payroll/SalaryRecipientService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: تعيين/إنهاء مستلم وصورته وتأثيرها على كيان الشخص المشترك، مع سياق الملف والإيقاف الصحيح.

| السطر | الموضع |
|---|---|
| 49 | `CreateCoreAsync(` |
| 349 | `EndCoreAsync(` |
| 405 | `UpdatePhotoAsync(` |

## PensionMS.Infrastructure/Services/Documents/DocumentService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: الرفع الفوري والمؤجل والجماعي؛ الإيقاف عند الحفظ لا بعد طلب لاحق؛ النسخة الأصلية والملف الثنائي؛ توحيد مراجعة الوثيقة مع قرار الملف وكشف المحذوف.

| السطر | الموضع |
|---|---|
| 264 | `UploadAsync(` |
| 264 | `deferPostProcessing` |
| 585 | `RefreshAfterUploadAsync(` |
| 555 | `ProcessFileLevelPostUploadAsync(` |
| 389 | `ResolveOwnerFileAsync(` |
| 908 | `DeleteAsync(` |
| 956 | `UpdateAsync(` |
| 721 | `ReviewAsync(` |

## PensionMS.Infrastructure/Services/Pension/Entitlements/FinancialEntitlementService.cs

- التصنيف: CHANGE
- القواعد: R04,R06,R08,R11
- المطلوب: إنشاء/إنهاء الاستحقاق لا يطلب مراجعة حالياً؛ البيانات مرتبطة بالمتقاعد لا بملف محدد؛ حفظ المعتمد للفترات وإعادة الاستحقاق.

| السطر | الموضع |
|---|---|
| 127 | `CreateAsync(` |
| 238 | `EndAsync(` |

## PensionMS.Infrastructure/Services/Pension/Entitlements/EntitlementEngine.cs

- التصنيف: CHANGE
- القواعد: R04,R06,R08
- المطلوب: تمييز المبلغ المستحق عن المنع من الصرف، وتنسيق إعادة التقييم الآلية وآثارها دون دورة مراجعة ذاتية.

| السطر | الموضع |
|---|---|
| 19 | `CheckCoreEligibilityAsync(` |
| 66 | `ReEvaluateEntitlementStatusAsync(` |
| 92 | `DeactivateLinkedAgentsAndRecipientsAsync(` |

## PensionMS.Infrastructure/Services/Documents/DocumentPledgeService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: التعهد والتسوية والحذف تغيّر متطلبات الملف دون طلب مراجعة؛ فصل آثارها عن سبب الإيقاف.

| السطر | الموضع |
|---|---|
| 54 | `CreateAsync(` |
| 125 | `MarkFulfilledAsync(` |
| 168 | `UpdateAsync(` |
| 196 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Documents/DocumentExemptionService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: الإعفاء وربط النواقص ومرفق التوجيه وتغيير متطلبات الاعتماد.

| السطر | الموضع |
|---|---|
| 46 | `CreateAsync(` |
| 105 | `UpdateAsync(` |
| 124 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Documents/MissingDocumentService.cs

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: إضافة/تسوية/حذف النواقص، وآثار الرفع التي تستوفيها تلقائياً.

| السطر | الموضع |
|---|---|
| 107 | `CreateAsync(` |
| 168 | `MarkFulfilledAsync(` |
| 193 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Pension/Files/FileNoteService.cs

- التصنيف: CONTRACT
- القواعد: R04
- المطلوب: الملاحظات كيانات ملف مرتبطة؛ حسم شمول العمليات الإدارية ضمن عبارة أي كيان، وعدم الخلط مع الملاحظات الآلية.

| السطر | الموضع |
|---|---|
| 30 | `CreateAsync(` |
| 54 | `DeleteAsync(` |

## PensionMS.Infrastructure/Services/Pension/Files/MinistryApprovalService.cs

- التصنيف: CONTRACT
- القواعد: R03,R04
- المطلوب: إنشاء المتقاعد من الإحالة وتعديل بيانات المصدر؛ عدم إطلاق مراجعة ملف لم يُنشأ بعد، ومراجعة انعكاس تعديلات المصدر بعد اعتماده.

| السطر | الموضع |
|---|---|
| 459 | `UpdateManualItemAsync(` |
| 559 | `ProcessItemAsync(` |
| 755 | `ProcessBulkApprovalsAsync(` |

## PensionMS.Infrastructure/Services/Pension/Files/ComplianceEngine.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R11
- المطلوب: تحديث عدادات النواقص تلقائياً يحفظ الملف؛ منع إنتاج مراجعة متكررة من العدادات والطوابع الزمنية.

| السطر | الموضع |
|---|---|
| 132 | `RefreshComplianceCacheAsync(` |

## PensionMS.Infrastructure/Services/Pension/Files/FileQueryService.cs

- التصنيف: CHANGE
- القواعد: R01,R02,R10
- المطلوب: الفلاتر وقوائم الإدخال والرفع والإحصاءات والبيانات المحذوفة؛ التجهيز مقابل المعلق/المنتهي ومراجعة كل الملفات القديمة.

| السطر | الموضع |
|---|---|
| 111 | `GetAllAsync(` |
| 316 | `GetByRetireeIdAsync(` |
| 402 | `GetFileChangeLogAsync(` |
| 753 | `GetForDataEntryAsync(` |
| 809 | `GetForDocumentUploadAsync(` |
| 909 | `GetDocumentUploadStatsAsync(` |
| 1057 | `GetDataEntryStatsAsync(` |

## PensionMS.Infrastructure/Services/Pension/Files/RetirementFileDtoMapper.cs

- التصنيف: CHANGE
- القواعد: R01,R02
- المطلوب: تسمية معلق وانتهى استحقاقه، وإظهار حالة المراجعة ووضع المعاش بصورة مستقلة.

| السطر | الموضع |
|---|---|
| 33 | `FileStatusName =` |
| 71 | `ReviewStatus =` |

## PensionMS.Infrastructure/Services/Pension/Files/WorkflowEngine.cs

- التصنيف: CHANGE
- القواعد: R01,R03,R05
- المطلوب: مهام التجهيز والتصحيح والمراجعة وتفعيل الملف من القرار النهائي مع إكمال المهام السابقة.

| السطر | الموضع |
|---|---|
| 21 | `RouteFileAsync(` |
| 44 | `FileStatusConstants.Incomplete` |
| 50 | `FileStatusConstants.UnderReview` |

## PensionMS.Infrastructure/Helpers/PensionFileAccess.cs

- التصنيف: CHANGE
- القواعد: R01,R06
- المطلوب: إزالة حظر التعديل المبني على موقوف/مرفوض وإتاحة تصحيح بيانات المنتهي بالإجراء المحدد.

| السطر | الموضع |
|---|---|
| 19 | `Editable(RetirementFile file)` |

## PensionMS.Infrastructure/Helpers/RecipientScope.cs

- التصنيف: CONTRACT
- القواعد: R04,R08,R11
- المطلوب: تُستخدم في أهلية الصرف وإنهاء المستلمين؛ لا تخفض المستحق المتراكم بسبب حالة مراجعة مؤقتة.

| السطر | الموضع |
|---|---|
| 18 | `End(SalaryRecipient` |
| 38 | `Reviewed(PendingReviewStatus` |
| 41 | `AgentPayable(Agent` |

## PensionMS.API/Interceptors/AuditSaveChangesInterceptor.cs

- التصنيف: CHANGE
- القواعد: R04,R11
- المطلوب: تدقيق حذف منطقي ومفاتيح مركبة وسياق الملف، والتقاط القيم والأفعال الحقيقية بدل الاعتماد على Update عام.

| السطر | الموضع |
|---|---|
| 58 | `BeforeSaveChanges(DbContext?` |
| 91 | `var action =` |
| 158 | `var pk =` |
| 54 | `FixupCreatedRecordIdsAsync(` |

## PensionMS.Infrastructure/Services/Audit/SnapshotService.cs

- التصنيف: CONTRACT
- القواعد: R04,R11,R12
- المطلوب: اللقطة الحالية محدودة ببيانات رأس الملف؛ لا تكفي لاستعادة المستحقين والوكلاء والوثائق أو حساب المستحقات التاريخية.

| السطر | الموضع |
|---|---|
| 24 | `CaptureAsync(` |
| 41 | `var fullState =` |

## PensionMS.Infrastructure/Services/Security/SoD/SoDGuardService.cs

- التصنيف: CHANGE
- القواعد: R05,R11
- المطلوب: سجل الأعمال الحالي يقتصر على جدول الملف؛ فحص محرري الكيانات المرتبطة ضمن الاعتماد الموحّد.

| السطر | الموضع |
|---|---|
| 21 | `ValidateAsync(` |
| 34 | `a.TableID == 2` |

## PensionMS.Infrastructure/Services/Security/SoD/SoDRules.cs

- التصنيف: CHANGE
- القواعد: R05,R11
- المطلوب: مواءمة الاعتماد الذي يفعّل مباشرة مع فصل المُدخل عن المراجع دون تجاوز الضوابط.

| السطر | الموضع |
|---|---|
| 10 | `class DocumentUploaderCannotApproveRule` |
| 31 | `class FinancialEditorCannotApproveRule` |
| 52 | `class ApproverCannotActivateRule` |

## PensionMS.Infrastructure/Repositories/UnitOfWork.cs

- التصنيف: CONTRACT
- القواعد: R04,R09,R10,R11
- المطلوب: مراجعة القفل والمعاملات المشتركة للإيقاف والاعتماد والإفراج والتشغيل الجماعي وترتيب الأقفال.

| السطر | الموضع |
|---|---|
| 449 | `LockFileReviewAsync(` |
| 459 | `LockSalaryBatchAsync(` |
| 468 | `BeginTransactionAsync(` |

## PensionMS.Infrastructure/Data/UnitOfWorkTransaction.cs

- التصنيف: CONTRACT
- القواعد: R04,R11
- المطلوب: نمط جاهز لملكية المعاملة المتداخلة؛ نشر فشل المراجعة بدل حفظ البيانات مع فشل التعليق.

| السطر | الموضع |
|---|---|
| 10 | `ExecuteAsync<T>` |

## PensionMS.Infrastructure/Repositories/SettlementRepository.cs

- التصنيف: CHANGE
- القواعد: R04,R08,R11
- المطلوب: مسار كتابة غير مباشر يعدل رتبة جميع ملفات المتقاعد وراتب مصدره؛ يحتاج سياق الملف والتعليق والنسخة المعتمدة.

| السطر | الموضع |
|---|---|
| 328 | `ApplyRetireeRankUpdatesAsync(` |
| 343 | `file.RankID =` |
| 358 | `retiree.ApprovalItem.Salary =` |

## PensionMS.Infrastructure/Services/Settlements/SettlementService.cs

- التصنيف: CHANGE
- القواعد: R04,R08,R11
- المطلوب: الاستيراد وربط المتقاعد ينشئان استحقاقاً ويعدلان رتب الملفات؛ ربط آثار التسوية بالمراجعة والإيقاف مع حفظ المعاملة.

| السطر | الموضع |
|---|---|
| 131 | `ImportExcelAsync(` |
| 496 | `_financialEntitlementService.CreateAsync` |
| 508 | `ApplyRetireeRankUpdatesAsync` |
| 860 | `LinkItemToRetireeAsync(` |

## PensionMS.Infrastructure/Repositories/Repository.cs

- التصنيف: CONTRACT
- القواعد: R04,R11
- المطلوب: الحذف الحالي Remove حقيقي؛ عدم استخدامه لحذف أعمال يجب حفظ تاريخها.

| السطر | الموضع |
|---|---|
| 57 | `Delete(T entity)` |

## PensionMS.Infrastructure/Jobs/ExpiredPledgesJob.cs

- التصنيف: CHANGE
- القواعد: R07
- المطلوب: العمل الدوري مسجل؛ يجب أن يوقف المعاش فقط، بما فيه الملف المعلق، دون إعادة حالة Suspended.

| السطر | الموضع |
|---|---|
| 42 | `ProcessExpiredPledgesAsync(` |

## PensionMS.Core/Constants/LookupConstants.cs

- التصنيف: CHANGE
- القواعد: R08,R09
- المطلوب: الثوابت Failed=3/Cancelled=4 لا تطابق Seed: استلام=3 وفشل=4 وإلغاء=5؛ يجب حسم الترميز قبل تمثيل الحجز أو الإفراج.

| السطر | الموضع |
|---|---|
| 26 | `class TransactionStatusConstants` |

## PensionMS.Infrastructure/Data/Configurations/Lookups/TransactionStatusConfiguration.cs

- التصنيف: CHANGE
- القواعد: R08,R09
- المطلوب: توافق حالات الحوالة مع الثوابت وحالة منع الصرف المطلوبة دون تغيير نتائج حوالات قديمة.

| السطر | الموضع |
|---|---|
| 17 | `builder.HasData(` |

## PensionMS.Infrastructure/Data/Configurations/System/SystemTableConfiguration.cs

- التصنيف: CHANGE
- القواعد: R04,R11
- المطلوب: المراجعة تفترض PaymentBooks=19 بينما Seed يخصصه لكشوف التسويات ولا يزرع PaymentBooks؛ التحقق من SystemTables الفعلي وإصلاح الربط.

| السطر | الموضع |
|---|---|
| 36 | `TableID = 19,` |

## PensionMS.Core/Constants/SystemTableConstants.cs

- التصنيف: CHANGE
- القواعد: R04,R11
- المطلوب: توحيد معرّفات الكيانات المدققة بدل الأرقام المتناثرة مع التوافق مع البيانات الفعلية.

| السطر | الموضع |
|---|---|
| 6 | `class SystemTableConstants` |

## PensionMS.Core/Constants/MissingDocumentStatusConstants.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05
- المطلوب: حالات النواقص تبقى مستقلة عن حالات الملف؛ تحقق من شروط التسوية والتعهد والإعفاء عند الاعتماد.

| السطر | الموضع |
|---|---|
| 3 | `class MissingDocumentStatusConstants` |

## ui/src/constants/retirement.ts

- التصنيف: CHANGE
- القواعد: R01,R06
- المطلوب: الحالات الأربع؛ إزالة اعتماد منع الملف المكرر على استثناء Closed.

| السطر | الموضع |
|---|---|
| 6 | `FILE_STATUS =` |
| 20 | `CLOSED_STATUS_ID` |

## ui/src/components/domain/retirement/FileStatusBadge.tsx

- التصنيف: CHANGE
- القواعد: R01
- المطلوب: شارات الحالات الجديدة مع الإبقاء على موقوف للمعاش خارج خريطة الملف.

| السطر | الموضع |
|---|---|
| 4 | `STATUS_VARIANTS` |

## ui/src/components/domain/retirement/StatusUpdateDialog.tsx

- التصنيف: CHANGE
- القواعد: R01,R05,R06
- المطلوب: الأرقام 3/4/6/8 ورسائل الإغلاق النهائي وإلغاء الاستحقاق؛ استبدال الإجراءات بما يطابق المراجعة والانتهاء وإعادة الاستحقاق.

| السطر | الموضع |
|---|---|
| 25 | `sensitiveStatusIDs` |
| 52 | `statusWarnings:` |

## ui/src/pages/retirement-files/FileReviewPage.tsx

- التصنيف: CHANGE
- القواعد: R02,R05
- المطلوب: مسار /files/:id/review يغير الحالة مباشرة؛ يجب أن يستخدم قرارات المراجعة، فالرفض الحالي يطلب Suspended.

| السطر | الموضع |
|---|---|
| 51 | `updateFileStatus(fileId, 2` |
| 61 | `updateFileStatus(fileId, 3` |

## ui/src/components/domain/retirement/FileReviewHeader.tsx

- التصنيف: CHANGE
- القواعد: R01,R02
- المطلوب: تمرير رقم حالة الملف إلى شارة عامة تعتمد مفاتيح نصية؛ فصل حالة الملف عن المراجعة.

| السطر | الموضع |
|---|---|
| 53 | `RecordStateBadge state={statusID` |

## ui/src/components/domain/retirement/FileReviewSidebar.tsx

- التصنيف: CHANGE
- القواعد: R01,R02
- المطلوب: مراجعة استخدام شارة عامة لحالة الملف وعرض المعاش بصورة مستقلة.

| السطر | الموضع |
|---|---|
| 6 | `RecordStateBadge` |

## ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx

- التصنيف: CHANGE
- القواعد: R01,R04,R05,R06,R07
- المطلوب: توحيد الحالات والإجراءات وصلاحيات التعديل، وإظهار سبب إيقاف المعاش وتحديث جميع البيانات بعد القرار.

| السطر | الموضع |
|---|---|
| 184 | `const statusMutation` |
| 226 | `file.statusID === 2` |
| 270 | `file?.reviewStatus === 3` |

## ui/src/pages/retirement-files/RetirementFilesPage.tsx

- التصنيف: CHANGE
- القواعد: R01,R02,R10
- المطلوب: الفلاتر والقوائم والشارات والإحالة الجماعية والحالة المعلقة مع مراجعة مرفوضة.

| السطر | الموضع |
|---|---|
| 209 | `FileStatusBadge statusId` |
| 210 | `row.reviewStatus === 1` |

## ui/src/pages/retirement-files/tabs/ReviewTab.tsx

- التصنيف: CHANGE
- القواعد: R02,R05,R07,R09
- المطلوب: توحيد الاعتماد الكلي والجزئي ومنع تغيير الحالة كبديل للمراجعة وتحديث الإيقافات والمستحقات المحجوزة.

| السطر | الموضع |
|---|---|
| 67 | `const statusMutation` |
| 86 | `const reviewMutation` |
| 108 | `const approveItemMutation` |

## ui/src/pages/review/FileReviewDetailPage.tsx

- التصنيف: CHANGE
- القواعد: R02,R05,R07,R09
- المطلوب: دورة المراجعة الحالية فقط، وفشل شروط التفعيل، وتحديث حالة الملف وإيقافه وبنود المعاش بعد القرار.

| السطر | الموضع |
|---|---|
| 384 | `const reviewMutation` |
| 399 | `const approveItemMutation` |
| 410 | `const rejectItemMutation` |

## ui/src/pages/pension/FileReviewListPage.tsx

- التصنيف: CHANGE
- القواعد: R02,R05,R10
- المطلوب: عرض المراجعات المرفوضة التي ما زالت معلقة والانتقالية وإعادة الاستحقاق، وتسمية رفض المراجعة.

| السطر | الموضع |
|---|---|
| 77 | `رفض الملف:` |
| 197 | `function FileReviewListPage` |

## ui/src/lib/retirementFileForm.ts

- التصنيف: CHANGE
- القواعد: R03,R06
- المطلوب: منع فتح ملف جديد بدلاً من إعادة استحقاق الملف نفسه؛ دالة تعتمد حالياً على استثناء Closed.

| السطر | الموضع |
|---|---|
| 271 | `findActiveFile<` |

## ui/src/components/domain/retirement/CreateFileWizard.tsx

- التصنيف: CHANGE
- القواعد: R03,R04,R06
- المطلوب: تجهيز أولي وإرسال صريح، وحماية الملف الموجود وتحديث النص والفحوص المقابلة للخلفية.

| السطر | الموضع |
|---|---|
| 1308 | `لا يمكن فتح ملف جديد لمتقاعد لديه ملف غير مغلق` |

## ui/src/constants/queryKeys.ts

- التصنيف: CHANGE
- القواعد: R04,R05,R07,R09
- المطلوب: تغطية مفاتيح الملف والمراجعة والإيقافات وكشوف الرواتب عند كل عملية أو قرار.

| السطر | الموضع |
|---|---|
| 13 | `retirement: {` |
| 173 | `salaries: {` |

## ui/src/hooks/useDocumentMutations.ts

- التصنيف: CHANGE
- القواعد: R03,R04,R11
- المطلوب: إبطال حالة الملف ومراجعته وإيقافاته ورواتبه بعد عمليات الوثائق، مع معالجة الرفع المؤجل.

| السطر | الموضع |
|---|---|
| 17 | `invalidateKeys:` |

## ui/src/hooks/queries/useFileDetailsQuery.ts

- التصنيف: CHANGE
- القواعد: R01,R02,R07
- المطلوب: عقد حالة الملف ومراجعته ووضع المعاش وسياق الملف للمتقاعد وتحديث البيانات المشتركة.

| السطر | الموضع |
|---|---|
| 6 | `interface RetirementFileDetail` |
| 84 | `FILE_DETAIL_KEY` |

## ui/src/hooks/queries/useReviewItemsQuery.ts

- التصنيف: CHANGE
- القواعد: R02
- المطلوب: تجميع بنود دورة المراجعة الحالية بدل القرارات التاريخية، وإظهار الحذف والروابط.

| السطر | الموضع |
|---|---|
| 8 | `groupBySection` |
| 37 | `getReviewDiff` |

## PensionMS.Infrastructure/Services/Reporting/DashboardService.cs

- التصنيف: CHANGE
- القواعد: R01,R02,R07
- المطلوب: إحصاءات الحالات الأربع منفصلة عن إيقاف المعاش؛ منع العد حسب نص الاسم أو ضم الجديد إلى النشط.

| السطر | الموضع |
|---|---|
| 43 | `FileStatusConstants.` |

## PensionMS.Infrastructure/Services/Reporting/DashboardService.Details.cs

- التصنيف: CHANGE
- القواعد: R01,R02,R07
- المطلوب: إحصاءات الحالات الأربع منفصلة عن إيقاف المعاش؛ منع العد حسب نص الاسم أو ضم الجديد إلى النشط.

| السطر | الموضع |
|---|---|
| 32 | `FileStatusConstants.` |

## PensionMS.Infrastructure/Services/Reporting/RetirementFileReportService.cs

- التصنيف: CHANGE
- القواعد: R01,R02,R07
- المطلوب: إحصاءات الحالات الأربع منفصلة عن إيقاف المعاش؛ منع العد حسب نص الاسم أو ضم الجديد إلى النشط.

| السطر | الموضع |
|---|---|
| 53 | `GetRetirementFileReportSummaryAsync(` |
| 65 | `ClosedCount =` |

## PensionMS.Infrastructure/Services/Reporting/ReportService.Retirement.cs

- التصنيف: CHANGE
- القواعد: R01,R02,R07
- المطلوب: إحصاءات الحالات الأربع منفصلة عن إيقاف المعاش؛ منع العد حسب نص الاسم أو ضم الجديد إلى النشط.

| السطر | الموضع |
|---|---|
| 56 | `FileStatusConstants.` |

## PensionMS.Core/Entities/RetirementFile.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class RetirementFile` |

## PensionMS.Core/Entities/FileReviewItem.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 6 | `class FileReviewItem` |

## PensionMS.Core/Entities/PensionSuspension.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class PensionSuspension` |

## PensionMS.Core/Entities/SalaryPaymentItem.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class SalaryPaymentItem` |

## PensionMS.Core/Entities/Retiree.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class Retiree` |

## PensionMS.Core/Entities/FinancialEntitlement.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class FinancialEntitlement` |

## PensionMS.Core/Entities/GuardianEntitledPerson.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R06,R07,R08,R09,R10,R11
- المطلوب: تحديد بيانات دورة المراجعة ونوع العملية والنسخة المعتمدة وسياق الملف وسبب الحجز والفترة المالية؛ لا تُعتمد إضافة أعمدة أو جداول قبل التصميم.

| السطر | الموضع |
|---|---|
| 8 | `class GuardianEntitledPerson` |

## PensionMS.Infrastructure/Data/Configurations/CoreConfiguration.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R08,R11
- المطلوب: قيود وفهارس وعلاقات الحفظ والمراجعة والإيقاف والبنود والفترات؛ مواءمة أي تغيير مع SQL اليدوي.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/Data/Configurations/CoreRelationsConfiguration.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R08,R11
- المطلوب: قيود وفهارس وعلاقات الحفظ والمراجعة والإيقاف والبنود والفترات؛ مواءمة أي تغيير مع SQL اليدوي.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/Data/Configurations/PayrollConfiguration.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R08,R11
- المطلوب: قيود وفهارس وعلاقات الحفظ والمراجعة والإيقاف والبنود والفترات؛ مواءمة أي تغيير مع SQL اليدوي.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/Data/Configurations/DocumentEntitiesConfiguration.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R08,R11
- المطلوب: قيود وفهارس وعلاقات الحفظ والمراجعة والإيقاف والبنود والفترات؛ مواءمة أي تغيير مع SQL اليدوي.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/Data/Configurations/FinalLookupsConfiguration.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R08,R11
- المطلوب: قيود وفهارس وعلاقات الحفظ والمراجعة والإيقاف والبنود والفترات؛ مواءمة أي تغيير مع SQL اليدوي.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/Data/PensionDbContext.cs

- التصنيف: CONTRACT
- القواعد: R02,R04,R07,R11
- المطلوب: تسجيل أي كيانات لاحقة والتحقق من الفلاتر والحذف والتزامن في المراجعة والترحيل.

| السطر | الموضع |
|---|---|
| 102 | `FileReviewItems =>` |
| 217 | `GLOBAL: RowVersion` |
| 294 | `HasQueryFilter(lambda)` |

## PensionMS.Infrastructure/DependencyInjection/PensionServiceCollectionExtensions.cs

- التصنيف: CONTRACT
- القواعد: R04,R05,R07,R09
- المطلوب: التسجيل اليدوي الفعلي أصبح داخل امتدادات DI؛ تسجيل أي تبعيات إضافية هنا وتحديث الموكات المقابلة.

| السطر | الموضع |
|---|---|
| 9 | `class ` |

## PensionMS.Infrastructure/DependencyInjection/PayrollServiceCollectionExtensions.cs

- التصنيف: CONTRACT
- القواعد: R04,R05,R07,R09
- المطلوب: التسجيل اليدوي الفعلي أصبح داخل امتدادات DI؛ تسجيل أي تبعيات إضافية هنا وتحديث الموكات المقابلة.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/DependencyInjection/DocumentsServiceCollectionExtensions.cs

- التصنيف: CONTRACT
- القواعد: R04,R05,R07,R09
- المطلوب: التسجيل اليدوي الفعلي أصبح داخل امتدادات DI؛ تسجيل أي تبعيات إضافية هنا وتحديث الموكات المقابلة.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## PensionMS.Infrastructure/DependencyInjection/SecurityServiceCollectionExtensions.cs

- التصنيف: CONTRACT
- القواعد: R04,R05,R07,R09
- المطلوب: التسجيل اليدوي الفعلي أصبح داخل امتدادات DI؛ تسجيل أي تبعيات إضافية هنا وتحديث الموكات المقابلة.

| السطر | الموضع |
|---|---|
| 8 | `class ` |

## PensionMS.Infrastructure/DependencyInjection/BackgroundJobsServiceCollectionExtensions.cs

- التصنيف: CONTRACT
- القواعد: R04,R05,R07,R09
- المطلوب: التسجيل اليدوي الفعلي أصبح داخل امتدادات DI؛ تسجيل أي تبعيات إضافية هنا وتحديث الموكات المقابلة.

| السطر | الموضع |
|---|---|
| 6 | `class ` |

## PensionMS.API/Controllers/RetirementFilesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 18 | `class ` |

## PensionMS.API/Controllers/FileReviewController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/FileStatusesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 11 | `class ` |

## PensionMS.API/Controllers/RetireesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 14 | `class ` |

## PensionMS.API/Controllers/EntitledPersonsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/AgentsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/GuardiansController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/PaymentBooksController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/SalaryRecipientsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/DocumentsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 13 | `class ` |

## PensionMS.API/Controllers/FinancialEntitlementsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/PensionSuspensionsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/DocumentPledgesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 13 | `class ` |

## PensionMS.API/Controllers/DocumentExemptionsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 13 | `class ` |

## PensionMS.API/Controllers/MissingDocumentsController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/FileNotesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 12 | `class ` |

## PensionMS.API/Controllers/SalaryPaymentBatchesController.cs

- التصنيف: CONTRACT
- القواعد: R03,R04,R05,R06,R07,R08,R09,R10,R11
- المطلوب: العقود والصلاحيات وسياق الملف؛ تدقيق جميع مسارات الكتابة، والاعتماد والإرسال وإعادة الاستحقاق وإحالة الجميع؛ تنفيذ السياسة في منطق الأعمال أيضاً.

| السطر | الموضع |
|---|---|
| 14 | `class ` |

## PensionMS.API/Controllers/SettlementsController.cs

- التصنيف: CONTRACT
- القواعد: R01,R04,R08,R11
- المطلوب: مدخل غير مباشر أو قائمة مرجعية/مهام؛ تحقق من سياق الملف ومعنى الحالات دون تعديل مجالات أخرى.

| السطر | الموضع |
|---|---|
| 13 | `class ` |

## PensionMS.API/Controllers/LookupsController.cs

- التصنيف: CONTRACT
- القواعد: R01,R04,R08,R11
- المطلوب: مدخل غير مباشر أو قائمة مرجعية/مهام؛ تحقق من سياق الملف ومعنى الحالات دون تعديل مجالات أخرى.

| السطر | الموضع |
|---|---|
| 11 | `class ` |

## PensionMS.API/Controllers/ComplianceController.cs

- التصنيف: CONTRACT
- القواعد: R01,R04,R08,R11
- المطلوب: مدخل غير مباشر أو قائمة مرجعية/مهام؛ تحقق من سياق الملف ومعنى الحالات دون تعديل مجالات أخرى.

| السطر | الموضع |
|---|---|
| 13 | `class ` |

## PensionMS.Core/Interfaces/IFileStatusService.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 9 | `interface IFileStatusService` |

## PensionMS.Core/Interfaces/IRetirementFileService.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 10 | `interface IRetirementFileService` |

## PensionMS.Core/Interfaces/IWorkflowEngine.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 7 | `interface IWorkflowEngine` |

## PensionMS.Core/Interfaces/IComplianceEngine.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 5 | `interface IComplianceEngine` |

## PensionMS.Core/Interfaces/ILookupService.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 5 | `interface ILookupService` |

## PensionMS.Core/Interfaces/ISettlementRepository.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 7 | `interface ISettlementRepository` |

## PensionMS.Core/Interfaces/ISettlementService.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R05,R06,R08,R10
- المطلوب: واجهة بلا استدعاء مباشر لثوابت الحالات؛ قد يلزم تغيير السياق أو نتيجة العملية مع تحديث كل المنفذين والموكات.

| السطر | الموضع |
|---|---|
| 6 | `interface ISettlementService` |

## PensionMS.Core/Entities/FileStatus.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 6 | `class ` |

## PensionMS.Core/Entities/MissingDocument.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 8 | `class ` |

## PensionMS.Core/DTOs/RetirementFiles/FileStatusDto.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 3 | `class ` |

## PensionMS.Core/DTOs/MissingDocuments/MissingDocumentDtos.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 3 | `class ` |

## PensionMS.Core/DTOs/Discrepancy/RetireeDiscrepancyDto.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 3 | `class ` |

## PensionMS.Core/DTOs/Reports/ReportDtos.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 8 | `class ` |

## PensionMS.Infrastructure/Services/Administration/LookupService.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 8 | `class ` |

## PensionMS.Infrastructure/Services/Pension/Files/RetirementFileCompleteness.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 8 | `class ` |

## PensionMS.Infrastructure/Reporting/ReportRegistry.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 5 | `class ` |

## PensionMS.Infrastructure/Reporting/Exporters/UploaderStatsReportExporter.cs

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R08,R10
- المطلوب: عقد/قائمة أو حساب اكتمال وتقارير متأثرة؛ ينبغي ألا تتحول النواقص أو نص الحالة إلى صلاحية تشغيل أو صرف ضمنية.

| السطر | الموضع |
|---|---|
| 7 | `class ` |

## ui/src/api/discrepancy.ts

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 5 | `export ` |

## ui/src/components/common/UploaderStatsView.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 52 | `export ` |

## ui/src/components/domain/print/print.types.ts

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 4 | `export ` |

## ui/src/components/domain/retirement/CorrectLifeStatusDialog.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 13 | `export ` |

## ui/src/components/domain/retirement/DocumentChecklistField.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 26 | `export ` |

## ui/src/components/domain/retirement/EntitledPersonDetails.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 24 | `export ` |

## ui/src/components/domain/retirement/ExemptionDialog.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 31 | `export ` |

## ui/src/components/domain/retirement/PersonDocCard.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 30 | `export ` |

## ui/src/components/domain/retirement/PledgeDialog.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 35 | `export ` |

## ui/src/components/domain/retirement/RecordDeathDialog.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 20 | `export ` |

## ui/src/components/domain/retirement/RetirementFileSidebar.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 43 | `export ` |

## ui/src/components/domain/retirement/sections/DocumentsSection.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 83 | `export ` |

## ui/src/lib/bulkUploadDraft.ts

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 3 | `export ` |

## ui/src/pages/pension/KashfReportsPage.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 33 | `export ` |

## ui/src/pages/print/PrintableRetirementFileReport.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 41 | `export ` |

## ui/src/pages/print/SettlementDecisionPrint.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 15 | `export ` |

## ui/src/pages/settlements/SettlementBatchDetailsPage.tsx

- التصنيف: CONTRACT
- القواعد: R01,R03,R04,R06,R08,R11
- المطلوب: مستهلك غير مباشر أو نص/عقد مرتبط بالملف؛ راجع العمليات والسياق والنسخة المعتمدة ودلالة الانتظار في العرض والطباعة.

| السطر | الموضع |
|---|---|
| 19 | `export ` |

## PensionMS.API/Program.cs

- التصنيف: CONTRACT
- القواعد: R05,R06,R10,R11
- المطلوب: صلاحيات الإجراءات الجديدة/المعاد استخدامها، خصوصاً أن إرسال المراجعة الحالي يتطلب File.Review.View.

| السطر | الموضع |
|---|---|
| 73 | `AddPensionInfrastructureServices` |
| 178 | `File.Review.Approve` |
| 177 | `File.Review.View` |

## PensionMS.API/Validators/Validators.cs

- التصنيف: CONTRACT
- القواعد: R04,R06,R11
- المطلوب: السبب وسياق الملف والإجراء وصحة التغيير دون تعليق عند الحفظ بلا تغيير فعلي.

| السطر | الموضع |
|---|---|
| 58 | `class CreateRetirementFileValidator` |
| 93 | `class UpdateRetirementFileValidator` |

## مستهلكو الواجهة وعقود المجال

كل ملف هنا مفهرس أيضاً في coverage-ledger.json. تُراجع الاستدعاءات والسياق وإبطال الاستعلامات، ولا تُفترض ضرورة تغيير كل ملف.

| الملف | أسطر الاستيراد |
|---|---|
| ui/src/components/common/RetirementFileReportViewer.tsx | 6, 7 |
| ui/src/components/domain/force/AddReferralItemDialog.tsx | 9 |
| ui/src/components/domain/investment/InvestmentDocumentsManager.tsx | 6 |
| ui/src/components/domain/print/print.utils.ts | 1, 2 |
| ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx | 4 |
| ui/src/components/domain/retirement/AgentDialog.tsx | 11 |
| ui/src/components/domain/retirement/BulkUploadDialog.tsx | 6, 7 |
| ui/src/components/domain/retirement/CreateFileWizard.tsx | 21 |
| ui/src/components/domain/retirement/DataEntryView.tsx | 12 |
| ui/src/components/domain/retirement/DocumentActions.tsx | 6 |
| ui/src/components/domain/retirement/DocumentProcessDialog.tsx | 19 |
| ui/src/components/domain/retirement/DocumentReviewPanel.tsx | 9 |
| ui/src/components/domain/retirement/EditDocumentDialog.tsx | 8, 9 |
| ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx | 10 |
| ui/src/components/domain/retirement/EntitlementAmountField.tsx | 6 |
| ui/src/components/domain/retirement/GuardianDialog.tsx | 10 |
| ui/src/components/domain/retirement/MissingDocActionDialog.tsx | 9 |
| ui/src/components/domain/retirement/PaymentBookDialog.tsx | 11 |
| ui/src/components/domain/retirement/RecipientsDialog.tsx | 8 |
| ui/src/components/domain/retirement/ReviewPanel.tsx | 3 |
| ui/src/components/domain/retirement/UploadDocumentDialog.tsx | 8, 9 |
| ui/src/components/domain/retirement/ViewAgentDialog.tsx | 4, 5 |
| ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx | 8, 9 |
| ui/src/components/domain/retirement/sections/BasicInfoSection.tsx | 5, 6 |
| ui/src/components/domain/retirement/sections/FinancialSection.tsx | 6, 7, 8 |
| ui/src/components/domain/retirement/sections/HistorySection.tsx | 4 |
| ui/src/components/domain/retirement/sections/PeopleSection.tsx | 5, 6, 7 |
| ui/src/components/domain/salaries/CreateSalaryBatchDialog.tsx | 8 |
| ui/src/components/domain/salaries/RecipientsTab.tsx | 5 |
| ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx | 8, 9 |
| ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx | 4, 5 |
| ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx | 7 |
| ui/src/components/domain/settlements/LinkRetireeDialog.tsx | 5, 6, 7, 8 |
| ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx | 8 |
| ui/src/hooks/queries/useAgentQuery.ts | 3 |
| ui/src/hooks/queries/useAgentsQuery.ts | 3 |
| ui/src/hooks/queries/useDocumentsByFileQuery.ts | 3 |
| ui/src/hooks/queries/useEntitledPersonQuery.ts | 3 |
| ui/src/hooks/queries/useEntitledPersonsQuery.ts | 3 |
| ui/src/hooks/queries/useFileDetailsQuery.ts | 3, 4 |
| ui/src/hooks/queries/useGuardianQuery.ts | 3 |
| ui/src/hooks/queries/useGuardiansQuery.ts | 3 |
| ui/src/hooks/queries/useMissingDocumentsQuery.ts | 3 |
| ui/src/hooks/queries/usePersonDocumentsQuery.ts | 3, 4 |
| ui/src/hooks/queries/useReviewItemsQuery.ts | 3 |
| ui/src/hooks/useDocumentMutations.ts | 1 |
| ui/src/hooks/useDocumentsQuery.ts | 3 |
| ui/src/hooks/useFileEntitiesQuery.ts | 3 |
| ui/src/pages/DashboardPage.tsx | 23 |
| ui/src/pages/archive/ArchivePage.tsx | 5 |
| ui/src/pages/archive/DocumentReviewArchivePage.tsx | 21, 22 |
| ui/src/pages/documents/MissingDocsCompletionPage.tsx | 15 |
| ui/src/pages/pension/AgentsPage.tsx | 6 |
| ui/src/pages/pension/AnnualBookReviewPage.tsx | 21 |
| ui/src/pages/pension/DocumentReviewPage.tsx | 17, 18 |
| ui/src/pages/pension/EntitledPersonsPage.tsx | 18 |
| ui/src/pages/pension/FileReviewListPage.tsx | 18 |
| ui/src/pages/pension/FinancialEntitlementsPage.tsx | 6 |
| ui/src/pages/pension/GuardiansPage.tsx | 7 |
| ui/src/pages/pension/MinistryReferralsPage.tsx | 28 |
| ui/src/pages/pension/MonthlySummariesPage.tsx | 6 |
| ui/src/pages/pension/PaymentCardsPage.tsx | 17, 18, 19 |
| ui/src/pages/pension/PensionStatisticsPage.tsx | 14 |
| ui/src/pages/pension/PensionSuspensionsPage.tsx | 9 |
| ui/src/pages/pension/SalaryRecipientsPage.tsx | 7 |
| ui/src/pages/pension/SalaryReconciliationPage.tsx | 5 |
| ui/src/pages/pension/SocialResearchPage.tsx | 22 |
| ui/src/pages/print/DocumentPledgePrint.tsx | 5 |
| ui/src/pages/print/MissingDocumentsPrintPage.tsx | 5, 6 |
| ui/src/pages/print/PaymentBookPrint.tsx | 7, 8 |
| ui/src/pages/print/RetirementFilePrintPage.tsx | 5, 6, 7 |
| ui/src/pages/print/SettlementDecisionPrint.tsx | 5, 6 |
| ui/src/pages/reports/ReportViewerPage.tsx | 6 |
| ui/src/pages/retirees/RetireeDetailsPage.tsx | 13 |
| ui/src/pages/retirees/RetireesPage.tsx | 7, 8 |
| ui/src/pages/retirement-files/DocumentProcessPage.tsx | 21 |
| ui/src/pages/retirement-files/FileReviewPage.tsx | 5 |
| ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx | 6 |
| ui/src/pages/retirement-files/RetirementFileReportsPage.tsx | 11, 17 |
| ui/src/pages/retirement-files/RetirementFilesPage.tsx | 8, 9 |
| ui/src/pages/retirement-files/tabs/AgentsTab.tsx | 5 |
| ui/src/pages/retirement-files/tabs/DocumentsTab.tsx | 22 |
| ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/GuardiansTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/NotesTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx | 6, 10 |
| ui/src/pages/retirement-files/tabs/ReviewTab.tsx | 5 |
| ui/src/pages/retirement-files/tabs/SalaryPaymentsTab.tsx | 6 |
| ui/src/pages/retirement-files/tabs/SettlementsTab.tsx | 6, 7 |
| ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx | 10 |
| ui/src/pages/retirement-files/tabs/SuspensionsTab.tsx | 6 |
| ui/src/pages/review/FileReviewDetailPage.tsx | 19 |
| ui/src/pages/salaries/SalaryBatchDetailsPage.tsx | 8 |
| ui/src/pages/salaries/SalaryBatchesPage.tsx | 6 |
| ui/src/pages/settlements/SettlementBatchDetailsPage.tsx | 7, 8 |
| ui/src/pages/settlements/SettlementDecisionsPage.tsx | 7, 8 |
| ui/src/pages/suspensions/SuspensionsPage.tsx | 7 |
| ui/src/utils/missing-documents.ts | 1 |
