using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIDToJournalEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CreatedByUserID",
                table: "JournalEntries",
                column: "CreatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Users_CreatedByUserID",
                table: "JournalEntries",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Users_CreatedByUserID",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CreatedByUserID",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "JournalEntries");
        }
    }
}
