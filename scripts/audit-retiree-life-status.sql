-- تقرير قراءة فقط للملفات التي قد تكون تأثرت بتغيير حالة الحياة بالطريقة القديمة.
-- راجع كل نتيجة وسجل الوفاة/التصحيح قبل أي معالجة مالية أو تعديل بيانات.
SELECT
    r.RetireeID,
    r.FullName,
    f.FileID,
    f.FileNumber,
    r.IsAlive,
    r.DeathDate,
    r.HealthStatusID,
    f.StatusID AS FileStatusID,
    CASE WHEN r.IsAlive = 1 AND r.DeathDate IS NOT NULL THEN 1 ELSE 0 END AS AliveWithDeathDate,
    CASE WHEN r.IsAlive = 0 AND r.DeathDate IS NULL THEN 1 ELSE 0 END AS DeceasedWithoutDeathDate,
    CASE WHEN r.IsAlive = 1 AND r.HealthStatusID IN (4, 5) THEN 1 ELSE 0 END AS AliveWithDeceasedHealthStatus,
    CASE WHEN r.IsAlive = 0 AND (r.HealthStatusID IS NULL OR r.HealthStatusID NOT IN (4, 5)) THEN 1 ELSE 0 END AS DeceasedWithLivingHealthStatus,
    CASE WHEN r.IsAlive = 0 AND f.StatusID = 2 THEN 1 ELSE 0 END AS DeceasedWithActiveFile,
    (SELECT COUNT(*) FROM SalaryRecipients sr
     WHERE sr.FileID = f.FileID AND sr.RetireeID = r.RetireeID
       AND sr.IsActive = 1 AND sr.IsDeleted = 0) AS ActiveRetireeRecipients,
    (SELECT COUNT(*) FROM EntitledPersons ep
     WHERE ep.FileID = f.FileID AND ep.IsActive = 1 AND ep.IsDeleted = 0) AS ActiveBeneficiaries,
    (SELECT COUNT(*) FROM PaymentBooks pb
     WHERE pb.FileID = f.FileID AND pb.Status = N'فعال' AND pb.IsDeleted = 0) AS ActivePaymentBooks
FROM Retirees r
LEFT JOIN RetirementFiles f ON f.RetireeID = r.RetireeID AND f.IsDeleted = 0
WHERE r.IsDeleted = 0
  AND (
    (r.IsAlive = 1 AND (r.DeathDate IS NOT NULL OR r.HealthStatusID IN (4, 5)))
    OR (r.IsAlive = 0 AND (r.DeathDate IS NULL OR r.HealthStatusID IS NULL OR r.HealthStatusID NOT IN (4, 5)))
    OR (r.IsAlive = 0 AND f.StatusID = 2)
    OR (r.IsAlive = 0 AND EXISTS (SELECT 1 FROM SalaryRecipients sr
                                 WHERE sr.FileID = f.FileID AND sr.RetireeID = r.RetireeID
                                   AND sr.IsActive = 1 AND sr.IsDeleted = 0))
    OR (r.IsAlive = 0 AND EXISTS (SELECT 1 FROM PaymentBooks pb
                                 WHERE pb.FileID = f.FileID AND pb.Status = N'فعال' AND pb.IsDeleted = 0))
    OR (r.IsAlive = 1 AND EXISTS (SELECT 1 FROM EntitledPersons ep
                                  WHERE ep.FileID = f.FileID AND ep.IsActive = 1 AND ep.IsDeleted = 0))
  )
ORDER BY r.RetireeID, f.FileID;
