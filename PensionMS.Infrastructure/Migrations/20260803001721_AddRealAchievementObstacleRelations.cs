using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRealAchievementObstacleRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "TrackingLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AchievementObstacles");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentID",
                table: "TrackingLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "TrackingLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "AchievementObstacles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_DepartmentID",
                table: "TrackingLinks",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_EmployeeID",
                table: "TrackingLinks",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_EmployeeID",
                table: "AchievementObstacles",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementObstacles_Employees_EmployeeID",
                table: "AchievementObstacles",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingLinks_Departments_DepartmentID",
                table: "TrackingLinks",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingLinks_Employees_EmployeeID",
                table: "TrackingLinks",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementObstacles_Employees_EmployeeID",
                table: "AchievementObstacles");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackingLinks_Departments_DepartmentID",
                table: "TrackingLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackingLinks_Employees_EmployeeID",
                table: "TrackingLinks");

            migrationBuilder.DropIndex(
                name: "IX_TrackingLinks_DepartmentID",
                table: "TrackingLinks");

            migrationBuilder.DropIndex(
                name: "IX_TrackingLinks_EmployeeID",
                table: "TrackingLinks");

            migrationBuilder.DropIndex(
                name: "IX_AchievementObstacles_EmployeeID",
                table: "AchievementObstacles");

            migrationBuilder.DropColumn(
                name: "DepartmentID",
                table: "TrackingLinks");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "TrackingLinks");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "AchievementObstacles");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "TrackingLinks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AchievementObstacles",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }
    }
}
