-- Apply before deploying the API that exposes explicit agency scopes.
-- Existing rows remain NULL: do not infer a legal scope for historical agencies.
SET XACT_ABORT ON;
SET NOCOUNT ON;
IF COL_LENGTH('dbo.Agents', 'AgencyScope') IS NULL
    ALTER TABLE dbo.Agents ADD AgencyScope int NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('dbo.Agents') AND name = 'CK_Agents_AgencyScope')
    ALTER TABLE dbo.Agents WITH CHECK ADD CONSTRAINT CK_Agents_AgencyScope CHECK
        (AgencyScope IS NULL OR (AgencyScope IN (1, 2) AND EntitledID IS NULL)
        OR (AgencyScope = 3 AND EntitledID IS NOT NULL AND EntitledID > 0));
