using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice6_TestTypedExternalLabRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OutsourcedLabReferralEntityId",
                table: "Tests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tests_OutsourcedLabReferralEntityId",
                table: "Tests",
                column: "OutsourcedLabReferralEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_ReferralEntities_OutsourcedLabReferralEntityId",
                table: "Tests",
                column: "OutsourcedLabReferralEntityId",
                principalTable: "ReferralEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_ReferralEntities_OutsourcedLabReferralEntityId",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_OutsourcedLabReferralEntityId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "OutsourcedLabReferralEntityId",
                table: "Tests");
        }
    }
}
