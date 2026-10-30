using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixJournalEntryTypeID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop SourceModule column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'SourceModule')
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'IX_JournalEntries_SourceModule_SourceReferenceID')
                        DROP INDEX [IX_JournalEntries_SourceModule_SourceReferenceID] ON [JournalEntries];
                    ALTER TABLE [JournalEntries] DROP COLUMN [SourceModule];
                END");

            // Add JournalEntryTypeID column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'JournalEntryTypeID')
                    ALTER TABLE [JournalEntries] ADD [JournalEntryTypeID] int NOT NULL DEFAULT 0");

            // Create JournalEntryTypes table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JournalEntryTypes')
                BEGIN
                    CREATE TABLE [JournalEntryTypes] (
                        [JournalEntryTypeID] int NOT NULL IDENTITY,
                        [TypeName] nvarchar(50) NOT NULL,
                        [TypeNameEn] nvarchar(50) NOT NULL,
                        [Code] nvarchar(20) NULL,
                        [Color] nvarchar(7) NULL,
                        [Description] nvarchar(500) NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL DEFAULT 0,
                        [DeletedAt] datetime2 NULL,
                        [DeletedByUserID] int NULL,
                        [RowVersion] rowversion NULL,
                        CONSTRAINT [PK_JournalEntryTypes] PRIMARY KEY ([JournalEntryTypeID]),
                        CONSTRAINT [FK_JournalEntryTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
                    );
                END");

            // Seed JournalEntryTypes if empty
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [JournalEntryTypes])
                BEGIN
                    SET IDENTITY_INSERT [JournalEntryTypes] ON;
                    INSERT INTO [JournalEntryTypes] ([TypeName], [TypeNameEn], [Code], [Color], [Description], [IsActive], [CreatedAt], [IsDeleted])
                    VALUES
                    (N'أرصدة افتتاحية', 'Opening Balance', 'OPN', '#7F8C8D', N'قيود بداية السنة المالية والأرصدة المرحلة', 1, GETUTCDATE(), 0),
                    (N'قيد عام', 'General Journal', 'GEN', '#34495E', N'القيود اليدوية والتسويات المحاسبية', 1, GETUTCDATE(), 0),
                    (N'مشتريات', 'Purchases', 'PUR', '#3498DB', N'قيود شراء الأصول والمخزون والخدمات', 1, GETUTCDATE(), 0),
                    (N'رواتب وأجور', 'Payroll', 'SAL', '#2980B9', N'قيود رواتب الموظفين والاستحقاقات', 1, GETUTCDATE(), 0),
                    (N'معاشات تقاعدية', 'Pensions', 'PEN', '#2ECC71', N'قيود صرف المعاشات والمستحقات التقاعدية', 1, GETUTCDATE(), 0),
                    (N'مصروفات', 'Expenses', 'EXP', '#E74C3C', N'المصروفات التشغيلية والإدارية', 1, GETUTCDATE(), 0),
                    (N'إيرادات', 'Revenues', 'REV', '#27AE60', N'الإيرادات والاشتراكات والعوائد', 1, GETUTCDATE(), 0),
                    (N'سند قبض', 'Receipt Voucher', 'REC', '#16A085', N'تحصيل النقدية والبنوك', 1, GETUTCDATE(), 0),
                    (N'سند صرف', 'Payment Voucher', 'PAY', '#8E44AD', N'عمليات الدفع والصرف', 1, GETUTCDATE(), 0),
                    (N'الأصول الثابتة', 'Fixed Assets', 'AST', '#1ABC9C', N'شراء وإضافة ونقل واستبعاد الأصول', 1, GETUTCDATE(), 0),
                    (N'إهلاك الأصول', 'Depreciation', 'DEP', '#E67E22', N'قيود الإهلاك الدوري للأصول', 1, GETUTCDATE(), 0),
                    (N'المخزون', 'Inventory', 'INV', '#16A085', N'حركات وتسويات المخزون', 1, GETUTCDATE(), 0),
                    (N'تسويات محاسبية', 'Adjustments', 'ADJ', '#F39C12', N'قيود التسويات والتصحيح', 1, GETUTCDATE(), 0),
                    (N'إقفال السنة المالية', 'Year Closing', 'CLS', '#9B59B6', N'إقفال الحسابات وترحيل النتائج', 1, GETUTCDATE(), 0),
                    (N'قيود آلية', 'System Generated', 'SYS', '#95A5A6', N'قيود ينشئها النظام تلقائياً', 1, GETUTCDATE(), 0);
                    SET IDENTITY_INSERT [JournalEntryTypes] OFF;
                END");

            // Update existing JournalEntry rows to use OPN type
            migrationBuilder.Sql(@"
                UPDATE [JournalEntries] SET [JournalEntryTypeID] = (SELECT [JournalEntryTypeID] FROM [JournalEntryTypes] WHERE [Code] = 'OPN')
                WHERE [JournalEntryTypeID] = 0");

            // Create index if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'IX_JournalEntries_JournalEntryTypeID')
                    CREATE INDEX [IX_JournalEntries_JournalEntryTypeID] ON [JournalEntries] ([JournalEntryTypeID])");

            // Create unique indexes on JournalEntryTypes if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntryTypes') AND name = 'IX_JournalEntryTypes_Code')
                    CREATE UNIQUE INDEX [IX_JournalEntryTypes_Code] ON [JournalEntryTypes] ([Code]) WHERE [Code] IS NOT NULL");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntryTypes') AND name = 'IX_JournalEntryTypes_TypeName')
                    CREATE UNIQUE INDEX [IX_JournalEntryTypes_TypeName] ON [JournalEntryTypes] ([TypeName])");

            // Create FK if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID')
                    ALTER TABLE [JournalEntries] ADD CONSTRAINT [FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID] FOREIGN KEY ([JournalEntryTypeID]) REFERENCES [JournalEntryTypes] ([JournalEntryTypeID])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID')
                    ALTER TABLE [JournalEntries] DROP CONSTRAINT [FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID]");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'IX_JournalEntries_JournalEntryTypeID')
                    DROP INDEX [IX_JournalEntries_JournalEntryTypeID] ON [JournalEntries]");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'JournalEntryTypeID')
                    ALTER TABLE [JournalEntries] DROP COLUMN [JournalEntryTypeID]");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'SourceModule')
                BEGIN
                    ALTER TABLE [JournalEntries] ADD [SourceModule] nvarchar(50) NOT NULL DEFAULT '';
                    CREATE INDEX [IX_JournalEntries_SourceModule_SourceReferenceID] ON [JournalEntries] ([SourceModule], [SourceReferenceID]);
                END");
        }
    }
}
