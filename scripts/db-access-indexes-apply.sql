-- Manual schema change. Run precheck and verify the target database first.
-- SQL Server Express compatible; no ONLINE assumption. Run outside an application transaction.
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51002, 'Run index apply outside an existing transaction.', 1;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL OR OBJECT_ID('dbo.EntitledPersons', 'U') IS NULL
    THROW 51003, 'Required PensionMS tables are missing.', 1;

DECLARE @indexes TABLE (TableName sysname, IndexName sysname, Keys nvarchar(500), Definition nvarchar(1000));
INSERT @indexes VALUES
('AuditLogs', 'IX_AuditLogs_Record_CreatedAt', 'TableID ASC,RecordID ASC,CreatedAt DESC',
 '([TableID], [RecordID], [CreatedAt] DESC)'),
('AuditLogs', 'IX_AuditLogs_Recent', 'CreatedAt DESC,LogID DESC',
 '([CreatedAt] DESC, [LogID] DESC) INCLUDE ([TableID], [UserID], [Action])'),
('EntitledPersons', 'IX_EntitledPersons_FileID', 'FileID ASC', '([FileID])');

DECLARE @table sysname, @index sysname, @keys nvarchar(500), @definition nvarchar(1000), @sql nvarchar(max);
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE index_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, IndexName, Keys, Definition FROM @indexes;
    OPEN index_cursor;
    FETCH NEXT FROM index_cursor INTO @table, @index, @keys, @definition;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Compare key columns and direction, not just the index name. A longer
        -- unfiltered key with this prefix also serves these predicates/orderings.
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes i
            CROSS APPLY (SELECT STUFF((SELECT ',' + c.name + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE ' ASC' END
                FROM sys.index_columns ic JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal > 0
                ORDER BY ic.key_ordinal FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') AS KeyList) k
            WHERE i.object_id = OBJECT_ID('dbo.' + @table) AND i.is_disabled = 0
              AND i.is_hypothetical = 0 AND i.has_filter = 0
              AND (k.KeyList = @keys OR LEFT(k.KeyList, LEN(@keys) + 1) = @keys + ','))
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.' + @table) AND name = @index)
                THROW 51004, 'An index name exists with an incompatible definition; review precheck.', 1;
            SET @sql = N'CREATE INDEX ' + QUOTENAME(@index) + N' ON dbo.' + QUOTENAME(@table) + N' ' + @definition;
            EXEC sys.sp_executesql @sql;
            EXEC sys.sp_addextendedproperty @name = N'PensionMS.DbAccessRemediation', @value = N'2026-10-09',
                @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = @table,
                @level2type = N'INDEX', @level2name = @index;
            PRINT N'Created ' + @index;
        END
        ELSE PRINT N'Equivalent key already exists for ' + @index;
        FETCH NEXT FROM index_cursor INTO @table, @index, @keys, @definition;
    END
    CLOSE index_cursor;
    DEALLOCATE index_cursor;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
