using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccount356OperationalAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة حساب "مخصصات تشغيلية" (356) تحت باب المصروفات الجارية التحويلية (35)
            // للتسجيل المحاسبي لتخصيص الميزانية التشغيلية من الخزينة
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = N'356')
                BEGIN
                    DECLARE @ParentID INT = (SELECT AccountID FROM ChartOfAccounts WHERE AccountCode = N'35');

                    INSERT INTO ChartOfAccounts (
                        AccountCode,
                        AccountName,
                        AccountNature,
                        AccountLevel,
                        BookNumber,
                        ParentAccountID,
                        IsPostable,
                        IsActive,
                        IsDeleted,
                        Description,
                        CreatedAt
                    ) VALUES (
                        N'356',
                        N'مخصصات تشغيلية',
                        N'Debit',
                        3,
                        3,
                        @ParentID,
                        1,
                        1,
                        0,
                        N'تخصيصات للميزانية التشغيلية',
                        SYSUTCDATETIME()
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ChartOfAccounts WHERE AccountCode = N'356'
            ");
        }
    }
}
