/*
  توحيد حالات جميع ملفات التقاعد الحالية وإرسالها للمراجعة.
  لا يضيف جداول أو أعمدة ولا يغير تعريفات الكيانات.
  المعاينة هي الوضع الافتراضي. عند التشغيل الفعلي حدد المستخدم وضع @Apply = 1.
  الملفات المحذوفة منطقيًا تظل محفوظة ومحذوفة؛ لا يستعيدها هذا الإجراء.
  يحتفظ بسجلات المراجعة والإيقافات والمدفوعات السابقة. إعادة التشغيل لا تعيد تعليق ملف تم تحويله.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;
DECLARE @ActorUserID int = NULL;
DECLARE @RolloutID uniqueidentifier = 'C8DD9413-20E0-4C70-8202-202610200001';
DECLARE @Now datetime2(7) = SYSUTCDATETIME();

SELECT f.StatusID, s.StatusName, COUNT_BIG(*) AS FilesToReview
FROM dbo.RetirementFiles f LEFT JOIN dbo.FileStatuses s ON s.StatusID=f.StatusID
WHERE f.IsDeleted=0 AND NOT EXISTS (
    SELECT 1 FROM dbo.AuditLogs a WHERE a.TableID=2 AND a.RecordID=f.FileID
      AND a.Action=N'LifecycleRollout' AND a.CorrelationId=@RolloutID)
GROUP BY f.StatusID,s.StatusName ORDER BY f.StatusID;
SELECT COUNT_BIG(*) AS PreservedDeletedFiles FROM dbo.RetirementFiles WHERE IsDeleted=1;
IF @Apply=0 RETURN;
IF @ActorUserID IS NULL OR NOT EXISTS(SELECT 1 FROM dbo.Users WHERE UserID=@ActorUserID AND IsActive=1)
    THROW 51000,N'يجب تحديد مستخدم فعّال مسؤول عن تحويل الحالات',1;

BEGIN TRANSACTION;
DECLARE @LockResult int;
EXEC @LockResult=sys.sp_getapplock @Resource=N'PensionMS:LifecycleRollout:20261020',
    @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=30000;
IF @LockResult<0 THROW 51000,N'تعذر حجز عملية تحويل الحالات',1;

UPDATE dbo.FileStatuses SET StatusName=N'انتهى استحقاقه',Description=N'انتهت جميع الاستحقاقات المرتبطة بالملف، مع الاحتفاظ ببياناته وسجله' WHERE StatusID=4;
UPDATE dbo.FileStatuses SET StatusName=N'معلق' WHERE StatusID=6;

DECLARE @Files TABLE(FileID int PRIMARY KEY,OldStatusID int NULL,PreviousStatusID int NULL);
INSERT @Files(FileID,OldStatusID,PreviousStatusID)
SELECT f.FileID,f.StatusID,f.PreviousStatusID FROM dbo.RetirementFiles f WITH(UPDLOCK,HOLDLOCK)
WHERE f.IsDeleted=0 AND NOT EXISTS (
    SELECT 1 FROM dbo.AuditLogs a WHERE a.TableID=2 AND a.RecordID=f.FileID
      AND a.Action=N'LifecycleRollout' AND a.CorrelationId=@RolloutID);

/* نسخة أساس للمبالغ أثناء المراجعة للملفات التي لا تملك نسخة معتمدة محفوظة. */
UPDATE f SET LastReviewedFileState=(
    SELECT 1 AS Version,
      JSON_QUERY((SELECT r.* FROM dbo.Retirees r WHERE r.RetireeID=f.RetireeID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS Retiree,
      JSON_QUERY((SELECT e.* FROM dbo.FinancialEntitlements e WHERE e.RetireeID=f.RetireeID AND e.IsDeleted=0 FOR JSON PATH)) AS Entitlements,
      JSON_QUERY((SELECT p.*,
        JSON_QUERY((SELECT a.*,
            JSON_QUERY((SELECT ep.* FROM dbo.EntitledPersons ep WHERE ep.EntitledID=a.EntitledID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS EntitledPerson
          FROM dbo.Agents a WHERE a.AgentID=p.AgentID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS Agent,
        JSON_QUERY((SELECT g.* FROM dbo.Guardians g WHERE g.GuardianID=p.GuardianID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS Guardian,
        JSON_QUERY((SELECT ep.* FROM dbo.EntitledPersons ep WHERE ep.EntitledID=p.EntitledID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS EntitledPerson,
        JSON_QUERY((SELECT r.* FROM dbo.Retirees r WHERE r.RetireeID=p.RetireeID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)) AS Retiree
        FROM dbo.SalaryRecipients p WHERE p.FileID=f.FileID AND p.IsDeleted=0 FOR JSON PATH)) AS Recipients,
      JSON_QUERY((SELECT b.* FROM dbo.PaymentBooks b WHERE b.FileID=f.FileID AND b.IsDeleted=0 FOR JSON PATH)) AS Books,
      JSON_QUERY((SELECT ep.* FROM dbo.EntitledPersons ep WHERE ep.FileID=f.FileID AND ep.IsDeleted=0 FOR JSON PATH)) AS EntitledPersons,
      JSON_QUERY((SELECT g.* FROM dbo.GuardianEntitledPersons g JOIN dbo.EntitledPersons ep ON ep.EntitledID=g.EntitledID
        WHERE ep.FileID=f.FileID AND g.IsDeleted=0 FOR JSON PATH)) AS GuardianLinks
    FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)
FROM dbo.RetirementFiles f JOIN @Files q ON q.FileID=f.FileID
WHERE f.LastReviewedFileState IS NULL OR ISJSON(f.LastReviewedFileState)=0
   OR JSON_VALUE(CASE WHEN ISJSON(f.LastReviewedFileState)=1 THEN f.LastReviewedFileState ELSE N'{}' END,'$.Version') IS NULL;

/* لا يتحول إيقاف معاش قديم إلى إذن بالصرف بمجرد اعتماد المراجعة. */
INSERT dbo.PensionSuspensions(FileID,CreatedByUserID,SuspensionDate,SuspensionReason,SuspensionSource,Status,CreatedAt,IsDeleted)
SELECT q.FileID,@ActorUserID,@Now,N'إيقاف معاش محفوظ من الحالات القديمة',N'إيقاف قديم قبل توحيد الحالات',N'فعّال',@Now,0
FROM @Files q WHERE (q.OldStatusID=3 OR q.PreviousStatusID=3) AND NOT EXISTS (
  SELECT 1 FROM dbo.PensionSuspensions h WHERE h.FileID=q.FileID AND h.IsDeleted=0
    AND h.Status IN(N'فعّال',N'Active') AND h.SuspensionSource<>N'مراجعة بيانات الملف');

INSERT dbo.PensionSuspensions(FileID,CreatedByUserID,SuspensionDate,SuspensionReason,SuspensionSource,Status,CreatedAt,IsDeleted)
SELECT q.FileID,@ActorUserID,@Now,N'إيقاف الصرف حتى اعتماد مراجعة بيانات الملف',N'مراجعة بيانات الملف',N'فعّال',@Now,0
FROM @Files q WHERE NOT EXISTS (
  SELECT 1 FROM dbo.PensionSuspensions h WHERE h.FileID=q.FileID AND h.IsDeleted=0
    AND h.Status IN(N'فعّال',N'Active') AND h.SuspensionSource=N'مراجعة بيانات الملف');

UPDATE f SET PreviousStatusID=CASE WHEN q.OldStatusID=4 OR q.PreviousStatusID=4 THEN 4
                                  WHEN q.OldStatusID=6 THEN q.PreviousStatusID ELSE q.OldStatusID END,
    StatusID=6,ReviewStatus=1,ReviewRequestedAt=@Now,ReviewNotes=NULL,
    UpdatedAt=@Now,UpdatedByUserID=@ActorUserID
FROM dbo.RetirementFiles f JOIN @Files q ON q.FileID=f.FileID;

INSERT dbo.FileReviewItems(RetirementFileID,EntityType,EntityID,FieldName,NewValue,SectionName,SectionType,Status,CreatedAt)
SELECT FileID,2,FileID,N'مراجعة تحويل الحالات',N'مراجعة جميع بيانات الملف والكيانات والوثائق المرتبطة به',
    N'مراجعة بيانات الملف',N'File',0,@Now FROM @Files;
INSERT dbo.StatusTransitionLogs(TableID,ChangedByUserID,RecordID,OldStatusID,NewStatusID,Reason,TransitionDate)
SELECT 2,@ActorUserID,FileID,OldStatusID,6,N'تحويل جميع الملفات الحالية للمراجعة',@Now FROM @Files;
INSERT dbo.AuditLogs(UserID,TableID,Action,RecordID,OldValues,NewValues,CorrelationId,CreatedAt)
SELECT @ActorUserID,2,N'LifecycleRollout',FileID,
    (SELECT q.OldStatusID AS StatusID,q.PreviousStatusID FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),
    N'{"StatusID":6,"ReviewStatus":1}',@RolloutID,@Now FROM @Files q;
COMMIT;
SELECT COUNT_BIG(*) AS QueuedForReview FROM @Files;
