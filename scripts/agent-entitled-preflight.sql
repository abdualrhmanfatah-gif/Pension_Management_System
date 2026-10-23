SET NOCOUNT ON;

SELECT t.name AS TableName, c.name AS ColumnName, TYPE_NAME(c.user_type_id) AS DataType
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.name IN ('Agents', 'SalaryRecipients', 'SalaryPaymentItems')
  AND c.name IN ('FileID', 'EntitledID', 'AgentID', 'RecipientID', 'BatchID');

SELECT OBJECT_NAME(i.object_id) AS TableName, i.name, i.is_unique, i.filter_definition
FROM sys.indexes i
WHERE i.object_id IN (OBJECT_ID('dbo.Agents'), OBJECT_ID('dbo.SalaryPaymentItems'));

IF COL_LENGTH('dbo.Agents', 'EntitledID') IS NOT NULL
EXEC(N'
SELECT FileID, EntitledID, COUNT(*) AS ActiveAgents
FROM dbo.Agents WHERE IsActive = 1 AND IsDeleted = 0
GROUP BY FileID, EntitledID HAVING COUNT(*) > 1;

SELECT a.AgentID, a.FileID, a.EntitledID, e.FileID AS EntitledFileID
FROM dbo.Agents a LEFT JOIN dbo.EntitledPersons e ON e.EntitledID = a.EntitledID
WHERE a.IsDeleted = 0 AND a.EntitledID IS NOT NULL
AND (e.EntitledID IS NULL OR e.FileID <> a.FileID OR e.IsDeleted = 1 OR e.IsDeceased = 1);

SELECT a.AgentID, g.GuardianID, a.EntitledID
FROM dbo.Agents a JOIN dbo.GuardianEntitledPersons g ON a.EntitledID = g.EntitledID
WHERE a.IsActive = 1 AND a.IsDeleted = 0 AND g.IsActive = 1;

SELECT r.FileID, CASE WHEN r.AgentID IS NOT NULL THEN a.EntitledID ELSE r.EntitledID END AS EffectiveEntitledID,
COUNT(*) AS ActiveRecipients
FROM dbo.SalaryRecipients r LEFT JOIN dbo.Agents a ON a.AgentID = r.AgentID
WHERE r.IsActive = 1 AND r.IsDeleted = 0
GROUP BY r.FileID, CASE WHEN r.AgentID IS NOT NULL THEN a.EntitledID ELSE r.EntitledID END
HAVING COUNT(*) > 1;

SELECT r.RecipientID, r.FileID, r.AgentID, r.SharePercent
FROM dbo.SalaryRecipients r JOIN dbo.Agents a ON a.AgentID = r.AgentID
WHERE r.IsActive = 1 AND r.IsDeleted = 0
AND (a.IsActive = 0 OR a.IsDeleted = 1 OR a.FileID <> r.FileID
OR (a.EntitledID IS NOT NULL AND (r.SharePercent IS NULL OR r.SharePercent <= 0)));
');
