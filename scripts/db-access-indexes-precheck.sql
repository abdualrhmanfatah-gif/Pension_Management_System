-- Read-only. Run in the intended PensionMS database and retain the output before apply.
SELECT DB_NAME() AS DatabaseName, @@VERSION AS ServerVersion;
SELECT t.name AS TableName, i.name AS IndexName, i.is_unique, i.is_disabled,
       i.filter_definition, ic.key_ordinal, ic.is_descending_key, ic.is_included_column,
       c.name AS ColumnName
FROM sys.tables t
JOIN sys.indexes i ON i.object_id = t.object_id
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE t.schema_id = SCHEMA_ID('dbo') AND t.name IN ('AuditLogs', 'EntitledPersons')
ORDER BY t.name, i.index_id, ic.key_ordinal, ic.index_column_id;
SELECT OBJECT_NAME(p.object_id) AS TableName, SUM(p.rows) AS Rows
FROM sys.partitions p
WHERE p.object_id IN (OBJECT_ID('dbo.AuditLogs'), OBJECT_ID('dbo.EntitledPersons'))
  AND p.index_id IN (0, 1)
GROUP BY p.object_id;
