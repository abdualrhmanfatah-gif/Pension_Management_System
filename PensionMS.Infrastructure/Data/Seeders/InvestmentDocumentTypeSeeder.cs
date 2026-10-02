using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders
{
    public static class InvestmentDocumentTypeSeeder
    {
        public static async Task SeedAsync(PensionDbContext context)
        {
            // 1. Fix missing columns in DocumentTypes
            try
            {
                var fixDocumentTypesSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DocumentTypes]') AND name = 'OwnerType')
                    BEGIN ALTER TABLE [dbo].[DocumentTypes] ADD [OwnerType] INT NULL; END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DocumentTypes]') AND name = 'TypeNameEn')
                    BEGIN ALTER TABLE [dbo].[DocumentTypes] ADD [TypeNameEn] NVARCHAR(MAX) NULL; END";
                await context.Database.ExecuteSqlRawAsync(fixDocumentTypesSql);
            }
            catch { }

            // 2. Fix missing columns in Documents table (The cause of 500 error)
            try
            {
                var fixDocumentsTableSql = @"
                    -- Add missing columns
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = 'AssetValuationID')
                    BEGIN ALTER TABLE [dbo].[Documents] ADD [AssetValuationID] INT NULL; END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = 'DepreciationRecordID')
                    BEGIN ALTER TABLE [dbo].[Documents] ADD [DepreciationRecordID] INT NULL; END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = 'PaymentScheduleID')
                    BEGIN ALTER TABLE [dbo].[Documents] ADD [PaymentScheduleID] INT NULL; END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = 'OperationalExpenseID')
                    BEGIN ALTER TABLE [dbo].[Documents] ADD [OperationalExpenseID] INT NULL; END

                    -- Update Check Constraint
                    IF EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_Document_OwnerType]'))
                    BEGIN
                        ALTER TABLE [dbo].[Documents] DROP CONSTRAINT [CK_Document_OwnerType];
                    END

                    ALTER TABLE [dbo].[Documents] ADD CONSTRAINT [CK_Document_OwnerType] CHECK (
                        (OwnerType = 1  AND FileID IS NOT NULL)           OR
                        (OwnerType = 2  AND RetireeID IS NOT NULL)         OR
                        (OwnerType = 3  AND EntitledID IS NOT NULL)        OR
                        (OwnerType = 4  AND AgentID IS NOT NULL)           OR
                        (OwnerType = 5  AND GuardianID IS NOT NULL)        OR
                        (OwnerType = 6  AND PledgeID IS NOT NULL)          OR
                        (OwnerType = 7  AND ExemptionID IS NOT NULL)       OR
                        (OwnerType = 8  AND NoteID IS NOT NULL)            OR
                        (OwnerType = 9  AND SuspensionID IS NOT NULL)      OR
                        (OwnerType = 10 AND SalaryBatchID IS NOT NULL)     OR
                        (OwnerType = 11 AND MinistryBatchID IS NOT NULL)   OR
                        (OwnerType = 12 AND DecisionID IS NOT NULL)        OR
                        (OwnerType = 13 AND SettlementBatchID IS NOT NULL) OR
                        (OwnerType = 14 AND ReportID IS NOT NULL)          OR
                        (OwnerType = 15 AND InvestmentProjectID IS NOT NULL)  OR
                        (OwnerType = 16 AND InvestmentPropertyID IS NOT NULL) OR
                        (OwnerType = 17 AND PropertyUnitID IS NOT NULL)       OR
                        (OwnerType = 18 AND LeaseContractID IS NOT NULL)      OR
                        (OwnerType = 19 AND TenantID IS NOT NULL)             OR
                        (OwnerType = 20 AND InvestmentPortfolioID IS NOT NULL) OR
                        (OwnerType = 21 AND MaintenanceRequestID IS NOT NULL)  OR
                        (OwnerType = 22 AND WorkOrderID IS NOT NULL)           OR
                        (OwnerType = 25 AND AssetValuationID IS NOT NULL)      OR
                        (OwnerType = 27 AND DepreciationRecordID IS NOT NULL)  OR
                        (OwnerType = 28 AND PaymentScheduleID IS NOT NULL)     OR
                        (OwnerType = 30 AND OperationalExpenseID IS NOT NULL)  OR
                        (OwnerType = 31 AND InvestmentPlanID IS NOT NULL)      OR
                        (OwnerType = 33 AND RiskAssessmentID IS NOT NULL)      OR
                        (OwnerType = 40 AND EmployeeID IS NOT NULL)             OR
                        (OwnerType = 41 AND CorrespondenceID IS NOT NULL)
                    );";
                await context.Database.ExecuteSqlRawAsync(fixDocumentsTableSql);
            }
            catch { }

            // 3. Register Investment tables in SystemTables for Audit Logging
            try
            {
                var registerTablesSql = @"
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemTables')
                    BEGIN
                        SET IDENTITY_INSERT SystemTables ON;

                        IF NOT EXISTS (SELECT 1 FROM SystemTables WHERE TableID = 100)
                        INSERT INTO SystemTables (TableID, TableNameAr, TableNameEn, CreatedAt) VALUES (100, N'المحافظ الاستثمارية', 'InvestmentPortfolios', GETUTCDATE());
                        
                        IF NOT EXISTS (SELECT 1 FROM SystemTables WHERE TableID = 101)
                        INSERT INTO SystemTables (TableID, TableNameAr, TableNameEn, CreatedAt) VALUES (101, N'المشاريع الاستثمارية', 'InvestmentProjects', GETUTCDATE());
                        
                        IF NOT EXISTS (SELECT 1 FROM SystemTables WHERE TableID = 102)
                        INSERT INTO SystemTables (TableID, TableNameAr, TableNameEn, CreatedAt) VALUES (102, N'العقارات والأصول', 'InvestmentProperties', GETUTCDATE());
                        
                        IF NOT EXISTS (SELECT 1 FROM SystemTables WHERE TableID = 103)
                        INSERT INTO SystemTables (TableID, TableNameAr, TableNameEn, CreatedAt) VALUES (103, N'الوحدات العقارية', 'PropertyUnits', GETUTCDATE());
                        
                        SET IDENTITY_INSERT SystemTables OFF;
                    END";
                await context.Database.ExecuteSqlRawAsync(registerTablesSql);
            }
            catch { }

            // 4. Seed the Investment Document Types
            try
            {
                await context.Database.ExecuteSqlRawAsync(SeedSql);
            }
            catch { }
        }

        private const string SeedSql = @"
-- 1. InvestmentPortfolio = 20
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_PORT_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'وثيقة تأسيس المحفظة',           'Portfolio Establishment Document',    'INV_PORT_001', 20, 1, 0, 1,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'الخطة الاستثمارية',             'Investment Plan',                     'INV_PORT_002', 20, 1, 0, 2,  1, '.pdf,.docx,.xlsx', 20, GETUTCDATE()),
    (N'سياسة إدارة المخاطر',           'Risk Management Policy',              'INV_PORT_003', 20, 1, 0, 3,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'تقرير أداء المحفظة',            'Portfolio Performance Report',        'INV_PORT_004', 20, 0, 0, 4,  1, '.pdf,.xlsx', 20, GETUTCDATE()),
    (N'محضر اجتماع لجنة الاستثمار',    'Investment Committee Minutes',        'INV_PORT_005', 20, 0, 0, 5,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'التقرير السنوي للمحفظة',        'Annual Portfolio Report',             'INV_PORT_006', 20, 0, 0, 6,  1, '.pdf,.xlsx', 30, GETUTCDATE());
END

-- 2. InvestmentProject = 15
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_PROJ_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'دراسة الجدوى الاقتصادية',       'Feasibility Study',                   'INV_PROJ_001', 15, 1, 0, 1,  1, '.pdf,.docx', 30, GETUTCDATE()),
    (N'قرار اعتماد مجلس الإدارة',      'Board Approval Document',             'INV_PROJ_002', 15, 1, 0, 2,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'عقد المقاولة الرئيسي (معمد)',   'Main Contract (Authenticated)',       'INV_PROJ_003', 15, 1, 1, 3,  1, '.pdf,.docx', 20, GETUTCDATE()),
    (N'ترخيص البناء',                  'Building Permit',                     'INV_PROJ_004', 15, 1, 1, 4,  1, '.pdf,.jpg,.png', 10, GETUTCDATE()),
    (N'المخططات الهندسية والتصاميم (المعمدة)', 'Approved Engineering Plans',  'INV_PROJ_005', 15, 1, 0, 5,  1, '.pdf,.dwg,.jpg', 50, GETUTCDATE()),
    (N'عقد الإشراف الهندسي',           'Engineering Supervision Contract',    'INV_PROJ_006', 15, 0, 1, 6,  1, '.pdf,.docx', 20, GETUTCDATE()),
    (N'تقرير التقدم الشهري',           'Monthly Progress Report',             'INV_PROJ_007', 15, 0, 0, 7,  1, '.pdf,.xlsx', 20, GETUTCDATE()),
    (N'تقرير الأثر البيئي',            'Environmental Impact Report',         'INV_PROJ_008', 15, 0, 0, 8,  1, '.pdf,.docx', 20, GETUTCDATE()),
    (N'كفالة حسن التنفيذ / الضمان البنكي', 'Performance Bond / Bank Guarantee','INV_PROJ_009', 15, 0, 1, 9,  1, '.pdf,.jpg', 10, GETUTCDATE()),
    (N'بوليصة التأمين الشامل للمشروع', 'Comprehensive Project Insurance',    'INV_PROJ_010', 15, 0, 1, 10, 1, '.pdf', 10, GETUTCDATE()),
    (N'محضر استلام ابتدائي',           'Preliminary Acceptance Certificate',  'INV_PROJ_011', 15, 0, 0, 11, 1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'محضر استلام نهائي',             'Final Acceptance Certificate',        'INV_PROJ_012', 15, 0, 0, 12, 1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'خطة إدارة المخاطر',             'Risk Management Plan',                'INV_PROJ_013', 15, 0, 0, 13, 1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'الميزانية التفصيلية للمشروع',   'Detailed Project Budget (BOQ)',       'INV_PROJ_014', 15, 1, 0, 14, 1, '.pdf,.xlsx', 20, GETUTCDATE());
END

-- 3. InvestmentProperty = 16
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_PROP_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'بصيرة الملكية (الصك العقاري)',  'Ownership Deed (Title Deed)',         'INV_PROP_001', 16, 1, 0, 1,  1, '.pdf,.jpg,.png', 10, GETUTCDATE()),
    (N'الصورة الرئيسية للعقار',        'Main Property Photo',                 'INV_PROP_002', 16, 1, 0, 2,  1, '.jpg,.png,.webp', 5, GETUTCDATE()),
    (N'ألبوم صور العقار',              'Property Photo Album',                'INV_PROP_003', 16, 0, 0, 3,  1, '.jpg,.png,.webp,.zip', 50, GETUTCDATE()),
    (N'شهادة تثمين (تقييم) العقار',    'Property Valuation Certificate',      'INV_PROP_004', 16, 1, 1, 4,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'وثيقة الوقف / عقارات الدولة',   'Waqf/State Property Document',        'INV_PROP_005', 16, 0, 0, 5,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'الاسقاط الجوي (المسطح الموقعي)','Aerial Survey (Site Plan)',            'INV_PROP_006', 16, 1, 0, 6,  1, '.pdf,.jpg,.png', 20, GETUTCDATE()),
    (N'مسودة عقد الشراء / التنازل',    'Purchase/Transfer Draft',             'INV_PROP_007', 16, 1, 0, 7,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'كروت الخدمات والمرافق',         'Utility Service Cards',               'INV_PROP_008', 16, 0, 0, 8,  1, '.pdf,.jpg,.png', 10, GETUTCDATE()),
    (N'تقرير الفحص الفني',             'Technical Inspection Report',         'INV_PROP_009', 16, 0, 0, 9,  1, '.pdf,.docx', 20, GETUTCDATE());
END

-- 4. PropertyUnit = 17
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_UNIT_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'المخطط الهندسي للوحدة',         'Unit Layout Plan',                    'INV_UNIT_001', 17, 1, 0, 1,  1, '.pdf,.jpg,.dwg', 20, GETUTCDATE()),
    (N'الصورة الرئيسية للوحدة',        'Main Unit Photo',                     'INV_UNIT_002', 17, 0, 0, 2,  1, '.jpg,.png,.webp', 5, GETUTCDATE()),
    (N'ألبوم صور الوحدة',              'Unit Photo Album',                    'INV_UNIT_003', 17, 0, 0, 3,  1, '.jpg,.png,.webp,.zip', 50, GETUTCDATE()),
    (N'محضر تسليم الوحدة',             'Unit Handover Certificate',           'INV_UNIT_004', 17, 0, 0, 4,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'محضر استلام الوحدة',            'Unit Receipt Certificate',            'INV_UNIT_005', 17, 0, 0, 5,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'تقرير حالة الوحدة',             'Unit Condition Report',               'INV_UNIT_006', 17, 0, 0, 6,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'كشف التجهيزات والمرفقات',       'Fixtures & Fittings Schedule',        'INV_UNIT_007', 17, 0, 0, 7,  1, '.pdf,.xlsx', 10, GETUTCDATE());
END

-- 5. Tenant = 19
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_TNNT_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'صورة البطاقة الشخصية (الرقم الوطني)', 'National ID Copy',              'INV_TNNT_001', 19, 1, 1, 1,  1, '.pdf,.jpg,.png', 5, GETUTCDATE()),
    (N'السجل التجاري',                 'Commercial Registration',             'INV_TNNT_002', 19, 0, 1, 2,  1, '.pdf,.jpg', 5, GETUTCDATE()),
    (N'البطاقة الضريبية + الرقم الضريبي', 'Tax Card + Tax Number',            'INV_TNNT_003', 19, 0, 1, 3,  1, '.pdf,.jpg', 5, GETUTCDATE()),
    (N'البطاقة التأمينية / الترخيص',   'Insurance Card / License',            'INV_TNNT_004', 19, 0, 1, 4,  1, '.pdf,.jpg', 5, GETUTCDATE()),
    (N'براءة ذمة زكوية / ضريبية',      'Zakat/Tax Clearance',                 'INV_TNNT_005', 19, 0, 1, 5,  1, '.pdf,.jpg', 5, GETUTCDATE()),
    (N'توكيل شرعي / تفويض',            'Legal Power of Attorney',             'INV_TNNT_006', 19, 0, 1, 6,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'ضمانة تجارية / كفالة حضورية وغرامية', 'Commercial Guarantee/Surety',   'INV_TNNT_007', 19, 1, 0, 7,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'صورة جواز السفر (مرفق بالإقامة)', 'Passport Copy (with Residence)',    'INV_TNNT_008', 19, 0, 1, 8,  1, '.pdf,.jpg,.png', 5, GETUTCDATE());
END

-- 6. LeaseContract = 18
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_LEAS_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'عقد الإيجار الموقّع والمعمد',   'Signed & Authenticated Lease',        'INV_LEAS_001', 18, 1, 1, 1,  1, '.pdf,.docx', 20, GETUTCDATE()),
    (N'ملحق / تعديل العقد',            'Contract Amendment',                  'INV_LEAS_002', 18, 0, 1, 2,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'إيصالات الدفع / سندات القبض (السند المالي)', 'Payment Receipts',       'INV_LEAS_003', 18, 0, 0, 3,  1, '.pdf,.jpg', 5, GETUTCDATE()),
    (N'شيك ضمان / استمارة الكفيل',     'Guarantee Check / Surety Form',       'INV_LEAS_004', 18, 1, 1, 4,  1, '.pdf,.jpg', 10, GETUTCDATE()),
    (N'خطاب تجديد العقد',              'Contract Renewal Letter',             'INV_LEAS_005', 18, 0, 0, 5,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'إشعار إخلاء / فسخ العقد',       'Eviction/Termination Notice',         'INV_LEAS_006', 18, 0, 0, 6,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'محضر معاينة عند التأجير',       'Pre-Lease Inspection Report',         'INV_LEAS_007', 18, 0, 0, 7,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'مخالصة / محضر إخلاء وتسليم',    'Clearance / Handover Certificate',    'INV_LEAS_008', 18, 1, 0, 8,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'شهادة تأمين المستأجر',          'Tenant Insurance Certificate',        'INV_LEAS_009', 18, 0, 1, 9,  1, '.pdf', 10, GETUTCDATE());
END

-- 7. MaintenanceRequest = 21
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_MAINT_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'صور الأضرار / العطل',           'Damage/Defect Photos',                'INV_MAINT_001', 21, 0, 0, 1, 1, '.jpg,.png,.webp,.zip', 50, GETUTCDATE()),
    (N'تقرير الفني / المعاين',         'Technical Inspection Report',         'INV_MAINT_002', 21, 0, 0, 2, 1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'عرض سعر المقاول',               'Contractor Price Quote',              'INV_MAINT_003', 21, 0, 0, 3, 1, '.pdf,.xlsx', 10, GETUTCDATE()),
    (N'أمر تكليف المقاول',             'Contractor Assignment Order',         'INV_MAINT_004', 21, 0, 0, 4, 1, '.pdf,.docx', 10, GETUTCDATE());
END

-- 8. WorkOrder = 22
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_WO_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'فاتورة المقاول',                'Contractor Invoice',                  'INV_WO_001', 22, 1, 0, 1, 1, '.pdf,.jpg', 10, GETUTCDATE()),
    (N'شهادة إنجاز / محضر استلام',     'Completion Certificate',              'INV_WO_002', 22, 1, 0, 2, 1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'صور بعد الإصلاح',               'Post-Repair Photos',                  'INV_WO_003', 22, 0, 0, 3, 1, '.jpg,.png,.webp,.zip', 50, GETUTCDATE()),
    (N'كفالة أعمال الصيانة',           'Maintenance Warranty',                'INV_WO_004', 22, 0, 1, 4, 1, '.pdf,.docx', 10, GETUTCDATE());
END

-- 10. PropertyValuation = 25
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_VAL_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'تقرير تثمين عقاري معتمد',           'Certified Valuation Report',          'INV_VAL_001', 25, 1, 1, 1,  1, '.pdf,.docx', 20, GETUTCDATE()),
    (N'شهادة القيمة السوقية',             'Market Value Certificate',            'INV_VAL_002', 25, 1, 0, 2,  1, '.pdf,.jpg', 10, GETUTCDATE()),
    (N'صور معاينة التقييم',               'Valuation Inspection Photos',         'INV_VAL_003', 25, 0, 0, 3,  1, '.jpg,.png,.zip', 50, GETUTCDATE()),
    (N'اعتماد المقيم / الشركة',           'Valuer Accreditation',                'INV_VAL_004', 25, 0, 1, 4,  1, '.pdf,.jpg', 5, GETUTCDATE());
END

-- 11. PropertyDepreciation = 27
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_DEPR_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'محضر إهلاك سنوي معمد',           'Annual Depreciation Minutes',         'INV_DEPR_001', 27, 1, 0, 1,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'تقرير محاسبي مؤيد للإهلاك',      'Accounting Supporting Report',        'INV_DEPR_002', 27, 0, 0, 2,  1, '.pdf,.xlsx', 20, GETUTCDATE()),
    (N'قرار إهلاك إداري',               'Administrative Depreciation Order',   'INV_DEPR_003', 27, 0, 0, 3,  1, '.pdf,.docx', 10, GETUTCDATE());
END

-- 12. LeasePayment = 28
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_PAY_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'سند قبض إيجار',                 'Rental Receipt',                      'INV_PAY_001', 28, 1, 0, 1,  1, '.pdf,.jpg,.png', 5, GETUTCDATE()),
    (N'صورة الشيك / حوالة',            'Check/Transfer Copy',                 'INV_PAY_002', 28, 0, 0, 2,  1, '.pdf,.jpg,.png', 5, GETUTCDATE()),
    (N'إشعار إيداع بنكي',              'Bank Deposit Advice',                 'INV_PAY_003', 28, 0, 0, 3,  1, '.pdf,.jpg,.png', 5, GETUTCDATE());
END

-- 13. OperationalExpense = 30
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'OPS_EXP_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'أمر الصرف الموقع والمعمد',      'Signed Financial Order',              'OPS_EXP_001', 30, 1, 0, 1,  1, '.pdf,.jpg,.png', 5, GETUTCDATE()),
    (N'الأوليات والمستندات الداعمة',   'Supporting Documents',                'OPS_EXP_002', 30, 0, 0, 2,  1, '.pdf,.zip', 20, GETUTCDATE()),
    (N'فاتورة المورد/المستفيد',        'Supplier/Beneficiary Invoice',        'OPS_EXP_003', 30, 0, 0, 3,  1, '.pdf,.jpg,.png', 5, GETUTCDATE()),
    (N'محضر اللجنة/الموافقة الإدارية', 'Committee Minutes/Approval',          'OPS_EXP_004', 30, 0, 0, 4,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'كشف استلام المواد/الخدمات',     'Receipt Notes/Delivery',              'OPS_EXP_005', 30, 0, 0, 5,  1, '.pdf,.xlsx', 10, GETUTCDATE());
END
-- 14. InvestmentPlan = 31
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_PLAN_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'الميزانية التفصيلية للسنة',     'Detailed Annual Budget',              'INV_PLAN_001', 31, 1, 0, 1,  1, '.pdf,.xlsx', 20, GETUTCDATE()),
    (N'تقرير الإنفاق الفعلي المعتمد',  'Approved Actual Spending Report',     'INV_PLAN_002', 31, 0, 0, 2,  1, '.pdf,.xlsx', 20, GETUTCDATE()),
    (N'الاعتماد الرسمي ومحضر الجلسة',  'Official Approval & Minutes',         'INV_PLAN_003', 31, 1, 0, 3,  1, '.pdf,.docx', 10, GETUTCDATE());
END

-- 15. RiskAssessment = 33
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE SystemCode = 'INV_RISK_001')
BEGIN
    INSERT INTO DocumentTypes (TypeName, TypeNameEn, SystemCode, OwnerType, IsRequired, HasExpiry, SortOrder, IsActive, AllowedExtensions, MaxFileSize, CreatedAt)
    VALUES
    (N'تقرير تقييم المخاطر المعتمد',   'Approved Risk Assessment Report',     'INV_RISK_001', 33, 1, 0, 1,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'خطة المعالجة والتخفيف (MMP)',   'Mitigation & Management Plan',        'INV_RISK_002', 33, 1, 0, 2,  1, '.pdf,.xlsx,.docx', 10, GETUTCDATE()),
    (N'توصية / موافقة لجنة المخاطر',   'Risk Committee Recommendation',       'INV_RISK_003', 33, 0, 0, 3,  1, '.pdf,.docx', 10, GETUTCDATE()),
    (N'ملاحق فنية وتقارير استشارية',   'Technical Annexes/Consultant Reports','INV_RISK_004', 33, 0, 0, 4,  1, '.pdf,.docx,.zip', 30, GETUTCDATE());
END
";
    }
}
