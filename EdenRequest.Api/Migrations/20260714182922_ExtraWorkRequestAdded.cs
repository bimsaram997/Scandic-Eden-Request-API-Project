using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExtraWorkRequestAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraWorkRequestId",
                table: "ExtraWorkItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExtraWorkRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedById = table.Column<int>(type: "integer", nullable: false),
                    AssignedToId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraWorkRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraWorkRequests_Employees_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraWorkRequests_Employees_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraWorkRequests_Employees_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExtraWorkRequests_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkItems_ExtraWorkRequestId",
                table: "ExtraWorkItems",
                column: "ExtraWorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkRequests_AssignedToId",
                table: "ExtraWorkRequests",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkRequests_RequestedById",
                table: "ExtraWorkRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkRequests_RoomId",
                table: "ExtraWorkRequests",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkRequests_UpdatedById",
                table: "ExtraWorkRequests",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraWorkItems_ExtraWorkRequests_ExtraWorkRequestId",
                table: "ExtraWorkItems",
                column: "ExtraWorkRequestId",
                principalTable: "ExtraWorkRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraWorkItems_ExtraWorkRequests_ExtraWorkRequestId",
                table: "ExtraWorkItems");

            migrationBuilder.DropTable(
                name: "ExtraWorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_ExtraWorkItems_ExtraWorkRequestId",
                table: "ExtraWorkItems");

            migrationBuilder.DropColumn(
                name: "ExtraWorkRequestId",
                table: "ExtraWorkItems");
        }
    }
}
