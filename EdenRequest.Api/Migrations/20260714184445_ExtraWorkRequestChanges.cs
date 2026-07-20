using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExtraWorkRequestChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraWorkItems_ExtraWorkRequests_ExtraWorkRequestId",
                table: "ExtraWorkItems");

            migrationBuilder.DropIndex(
                name: "IX_ExtraWorkItems_ExtraWorkRequestId",
                table: "ExtraWorkItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ExtraWorkRequests");

            migrationBuilder.DropColumn(
                name: "ExtraWorkRequestId",
                table: "ExtraWorkItems");

            migrationBuilder.CreateTable(
                name: "ExtraRequestLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExtraWorkRequestId = table.Column<int>(type: "integer", nullable: false),
                    ExtraWorkItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraRequestLines_ExtraWorkItems_ExtraWorkItemId",
                        column: x => x.ExtraWorkItemId,
                        principalTable: "ExtraWorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraRequestLines_ExtraWorkRequests_ExtraWorkRequestId",
                        column: x => x.ExtraWorkRequestId,
                        principalTable: "ExtraWorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtraRequestLines_ExtraWorkItemId",
                table: "ExtraRequestLines",
                column: "ExtraWorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraRequestLines_ExtraWorkRequestId",
                table: "ExtraRequestLines",
                column: "ExtraWorkRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtraRequestLines");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ExtraWorkRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtraWorkRequestId",
                table: "ExtraWorkItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkItems_ExtraWorkRequestId",
                table: "ExtraWorkItems",
                column: "ExtraWorkRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraWorkItems_ExtraWorkRequests_ExtraWorkRequestId",
                table: "ExtraWorkItems",
                column: "ExtraWorkRequestId",
                principalTable: "ExtraWorkRequests",
                principalColumn: "Id");
        }
    }
}
