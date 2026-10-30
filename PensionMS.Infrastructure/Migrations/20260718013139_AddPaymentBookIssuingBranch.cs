using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBookIssuingBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IssuingBranchID",
                table: "PaymentBooks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBooks_IssuingBranchID",
                table: "PaymentBooks",
                column: "IssuingBranchID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentBooks_PensionFundBranches_IssuingBranchID",
                table: "PaymentBooks",
                column: "IssuingBranchID",
                principalTable: "PensionFundBranches",
                principalColumn: "PensionBranchID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentBooks_PensionFundBranches_IssuingBranchID",
                table: "PaymentBooks");

            migrationBuilder.DropIndex(
                name: "IX_PaymentBooks_IssuingBranchID",
                table: "PaymentBooks");

            migrationBuilder.DropColumn(
                name: "IssuingBranchID",
                table: "PaymentBooks");
        }
    }
}
