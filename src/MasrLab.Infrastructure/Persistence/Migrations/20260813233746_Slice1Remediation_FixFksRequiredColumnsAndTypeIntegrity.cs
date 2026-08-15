using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice1Remediation_FixFksRequiredColumnsAndTypeIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SourceTestComponentId",
                table: "VisitTestResultItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cultures_VisitTestResultItems_VisitTestResultItemId",
                table: "Cultures",
                column: "VisitTestResultItemId",
                principalTable: "VisitTestResultItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReferenceValues_TestComponents_TestComponentId",
                table: "ReferenceValues",
                column: "TestComponentId",
                principalTable: "TestComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_VisitTestResultItems_VisitTestResultItemId",
                table: "TestResults",
                column: "VisitTestResultItemId",
                principalTable: "VisitTestResultItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitTestResultItems_TestComponents_SourceTestComponentId",
                table: "VisitTestResultItems",
                column: "SourceTestComponentId",
                principalTable: "TestComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cultures_VisitTestResultItems_VisitTestResultItemId",
                table: "Cultures");

            migrationBuilder.DropForeignKey(
                name: "FK_ReferenceValues_TestComponents_TestComponentId",
                table: "ReferenceValues");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_VisitTestResultItems_VisitTestResultItemId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitTestResultItems_TestComponents_SourceTestComponentId",
                table: "VisitTestResultItems");

            migrationBuilder.AlterColumn<int>(
                name: "SourceTestComponentId",
                table: "VisitTestResultItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
