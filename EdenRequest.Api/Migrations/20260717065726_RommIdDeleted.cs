using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdenRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class RommIdDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraWorkRequests_Rooms_RoomId",
                table: "ExtraWorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_ExtraWorkRequests_RoomId",
                table: "ExtraWorkRequests");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ExtraWorkRequests");

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "ExtraWorkRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "ExtraWorkRequests");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "ExtraWorkRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExtraWorkRequests_RoomId",
                table: "ExtraWorkRequests",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraWorkRequests_Rooms_RoomId",
                table: "ExtraWorkRequests",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
