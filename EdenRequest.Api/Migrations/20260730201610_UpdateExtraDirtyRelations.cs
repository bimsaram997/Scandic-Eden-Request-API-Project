using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExtraDirtyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports",
                column: "ReportedById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports",
                column: "ReportedById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
