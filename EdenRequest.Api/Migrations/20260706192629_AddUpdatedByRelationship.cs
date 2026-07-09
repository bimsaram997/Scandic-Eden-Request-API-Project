using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedByRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "RequestHeaders",
                newName: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RequestHeaders_UpdatedById",
                table: "RequestHeaders",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestHeaders_Employees_UpdatedById",
                table: "RequestHeaders",
                column: "UpdatedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHeaders_Employees_UpdatedById",
                table: "RequestHeaders");

            migrationBuilder.DropIndex(
                name: "IX_RequestHeaders_UpdatedById",
                table: "RequestHeaders");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "RequestHeaders",
                newName: "UpdatedBy");
        }
    }
}
