using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusDAtesChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ExtraWorkRequests",
                newName: "DoneDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedDate",
                table: "ExtraWorkRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcknowledgedDate",
                table: "ExtraWorkRequests");

            migrationBuilder.RenameColumn(
                name: "DoneDate",
                table: "ExtraWorkRequests",
                newName: "UpdatedAt");
        }
    }
}
