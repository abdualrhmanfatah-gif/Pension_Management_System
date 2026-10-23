-- Removes only indexes tagged as created by db-access-indexes-apply.sql.
-- Equivalent pre-existing indexes and EF-created test indexes remain untouched.
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51002, 'Run rollback outside an existing transaction.', 1;
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON dbo.' + QUOTENAME(t.name) + N';'
FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.extended_properties ep ON ep.class = 7 AND ep.major_id = i.object_id AND ep.minor_id = i.index_id
WHERE t.schema_id = SCHEMA_ID('dbo') AND ep.name = N'PensionMS.DbAccessRemediation' AND CONVERT(nvarchar(20), ep.value) = N'2026-10-09'
  AND ((t.name = 'AuditLogs' AND i.name IN ('IX_AuditLogs_Record_CreatedAt', 'IX_AuditLogs_Recent'))
       OR (t.name = 'EntitledPersons' AND i.name = 'IX_EntitledPersons_FileID'));
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC sys.sp_executesql @sql;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
