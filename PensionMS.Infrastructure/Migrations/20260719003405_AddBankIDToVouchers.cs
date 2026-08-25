using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBankIDToVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankID",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_BankID",
                table: "Vouchers",
                column: "BankID");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Banks_BankID",
                table: "Vouchers",
                column: "BankID",
                principalTable: "Banks",
                principalColumn: "BankID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Banks_BankID",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_BankID",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "BankID",
                table: "Vouchers");
        }
    }
}
