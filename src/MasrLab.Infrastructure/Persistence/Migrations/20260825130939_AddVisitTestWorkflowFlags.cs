using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitTestWorkflowFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "VisitTests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinishedByUserId",
                table: "VisitTests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExportMarked",
                table: "VisitTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "VisitTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                table: "VisitTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "VisitTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintedAt",
                table: "VisitTests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrintedByUserId",
                table: "VisitTests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "VisitTests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "VisitTests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "FinishedByUserId",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "IsExportMarked",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "IsPrinted",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "PrintedAt",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "PrintedByUserId",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "VisitTests");
        }
    }
}
