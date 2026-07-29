using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInExtraDirty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExtraDirtyReports_ReportedById",
                table: "ExtraDirtyReports",
                column: "ReportedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports",
                column: "ReportedById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraDirtyReports_Employees_ReportedById",
                table: "ExtraDirtyReports");

            migrationBuilder.DropIndex(
                name: "IX_ExtraDirtyReports_ReportedById",
                table: "ExtraDirtyReports");
        }
    }
}
