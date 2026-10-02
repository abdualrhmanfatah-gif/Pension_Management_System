-- =====================================================
-- Migration: Agent ↔ Entitled Person Link
-- Date: 2026-09-22
-- لا يؤثر على البيانات الموجودة
-- =====================================================

-- ── 1. إضافة عمود EntitledID (إن لم يكن موجوداً) ──
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Agents') AND name = 'EntitledID')
BEGIN
    ALTER TABLE [Agents] ADD [EntitledID] int NULL;
END;
GO

-- ── 2. حذف الفهرس القديم (إن وُجد) ──────────────
DROP INDEX IF EXISTS [IX_Agents_FileID] ON [Agents];
GO

-- ── 3. فهرس عادي على EntitledID ──────────────────
DROP INDEX IF EXISTS [IX_Agents_EntitledID] ON [Agents];
GO
CREATE INDEX [IX_Agents_EntitledID] ON [Agents] ([EntitledID]);
GO

-- ── 4. فهرس فريد: يستبعد NULLs ───────────────────
-- NULLs = وكيل عام → يُتحقق من الخدمة فقط
-- غير NULL = وكيل مستحق → فهرس يمنع التكرار
DROP INDEX IF EXISTS [IX_Agents_FileID_EntitledID] ON [Agents];
GO
CREATE UNIQUE INDEX [IX_Agents_FileID_EntitledID]
    ON [Agents] ([FileID], [EntitledID])
    WHERE [IsDeleted] = 0 AND [IsActive] = 1 AND [EntitledID] IS NOT NULL;
GO

-- ── 5. FK إلى EntitledPersons ─────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Agents_EntitledPersons_EntitledID')
BEGIN
    ALTER TABLE [Agents]
        ADD CONSTRAINT [FK_Agents_EntitledPersons_EntitledID]
        FOREIGN KEY ([EntitledID]) REFERENCES [EntitledPersons] ([EntitledID])
        ON DELETE NO ACTION;
END;
GO

-- ── 6. فهرس SalaryPaymentItems المتعدد ───────────
DROP INDEX IF EXISTS [IX_SalaryPaymentItems_FileID_BatchID] ON [SalaryPaymentItems];
GO
DROP INDEX IF EXISTS [IX_SalaryPaymentItems_FileID_BatchID_RecipientID] ON [SalaryPaymentItems];
GO
CREATE UNIQUE INDEX [IX_SalaryPaymentItems_FileID_BatchID_RecipientID]
    ON [SalaryPaymentItems] ([FileID], [BatchID], [RecipientID])
    WHERE [IsDeleted] = 0;
GO
