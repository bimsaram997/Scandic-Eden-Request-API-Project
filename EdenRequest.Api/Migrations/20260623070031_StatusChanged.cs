using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "RequestHeaders",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RequestHeaders",
                type: "text",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
