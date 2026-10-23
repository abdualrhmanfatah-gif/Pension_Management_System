SET XACT_ABORT ON;
SET NOCOUNT ON;
IF COL_LENGTH('dbo.Agents', 'EntitledID') IS NULL
    ALTER TABLE dbo.Agents ADD EntitledID int NULL;
IF COL_LENGTH('dbo.Agents', 'AgencyScope') IS NULL
    ALTER TABLE dbo.Agents ADD AgencyScope int NULL;
GO

BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('dbo.Agents') AND name = 'CK_Agents_AgencyScope')
        ALTER TABLE dbo.Agents WITH CHECK ADD CONSTRAINT CK_Agents_AgencyScope CHECK
            (AgencyScope IS NULL OR (AgencyScope IN (1, 2) AND EntitledID IS NULL)
            OR (AgencyScope = 3 AND EntitledID IS NOT NULL AND EntitledID > 0));
    IF EXISTS (SELECT 1 FROM dbo.Agents WHERE IsActive = 1 AND IsDeleted = 0
        GROUP BY FileID, EntitledID HAVING COUNT(*) > 1)
        THROW 51001, N'توجد وكالات نشطة مكررة. راجع تقرير الفحص قبل تطبيق القيود.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Agents a JOIN dbo.EntitledPersons e ON e.EntitledID = a.EntitledID
        WHERE a.FileID <> e.FileID)
        THROW 51002, N'توجد وكالة مرتبطة بمستحق من ملف آخر.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Agents_EntitledPersons_EntitledID')
        ALTER TABLE dbo.Agents ADD CONSTRAINT FK_Agents_EntitledPersons_EntitledID
        FOREIGN KEY (EntitledID) REFERENCES dbo.EntitledPersons(EntitledID);

    -- Replace only legacy unique indexes whose complete key is FileID.
    DECLARE @index sysname;
    DECLARE old_agent_indexes CURSOR LOCAL FAST_FORWARD FOR
        SELECT i.name FROM sys.indexes i
        WHERE i.object_id = OBJECT_ID('dbo.Agents') AND i.is_unique = 1
          AND i.is_primary_key = 0 AND i.is_unique_constraint = 0 AND i.filter_definition IS NULL
          AND (SELECT COUNT(*) FROM sys.index_columns c WHERE c.object_id = i.object_id AND c.index_id = i.index_id AND c.key_ordinal > 0) = 1
          AND EXISTS (SELECT 1 FROM sys.index_columns c JOIN sys.columns col ON col.object_id = c.object_id AND col.column_id = c.column_id
            WHERE c.object_id = i.object_id AND c.index_id = i.index_id AND c.key_ordinal = 1 AND col.name = 'FileID');
    OPEN old_agent_indexes;
    FETCH NEXT FROM old_agent_indexes INTO @index;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC(N'DROP INDEX ' + QUOTENAME(@index) + N' ON dbo.Agents');
        FETCH NEXT FROM old_agent_indexes INTO @index;
    END;
    CLOSE old_agent_indexes;
    DEALLOCATE old_agent_indexes;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Agents') AND name = 'UX_Agents_ActiveGeneral')
        DROP INDEX UX_Agents_ActiveGeneral ON dbo.Agents;
    CREATE UNIQUE INDEX UX_Agents_ActiveGeneral ON dbo.Agents(FileID)
        WHERE IsActive = 1 AND IsDeleted = 0 AND EntitledID IS NULL;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Agents') AND name = 'IX_Agents_FileID_EntitledID')
        DROP INDEX IX_Agents_FileID_EntitledID ON dbo.Agents;
    CREATE UNIQUE INDEX IX_Agents_FileID_EntitledID ON dbo.Agents(FileID, EntitledID)
        WHERE IsActive = 1 AND IsDeleted = 0 AND EntitledID IS NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.SalaryPaymentItems') AND name = 'IX_SalaryPaymentItems_FileID_BatchID')
        DROP INDEX IX_SalaryPaymentItems_FileID_BatchID ON dbo.SalaryPaymentItems;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.SalaryPaymentItems') AND name = 'IX_SalaryPaymentItems_FileID_BatchID_RecipientID')
        DROP INDEX IX_SalaryPaymentItems_FileID_BatchID_RecipientID ON dbo.SalaryPaymentItems;
    CREATE UNIQUE INDEX IX_SalaryPaymentItems_FileID_BatchID_RecipientID
        ON dbo.SalaryPaymentItems(FileID, BatchID, RecipientID) WHERE IsDeleted = 0;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;

-- Rollback of code does not require deleting EntitledID or financial history.
-- Keep both agency indexes. Restore a previous payment-item index only after
-- proving that there are no multi-recipient batches; never remove their rows.
