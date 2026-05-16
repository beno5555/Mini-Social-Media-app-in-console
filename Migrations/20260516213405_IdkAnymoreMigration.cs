using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace social_media_console_app.Migrations
{
    /// <inheritdoc />
    public partial class IdkAnymoreMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments",
                column: "CommenterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments",
                column: "CommenterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
