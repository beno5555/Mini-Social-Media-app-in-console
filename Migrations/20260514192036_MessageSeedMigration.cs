using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace social_media_console_app.Migrations
{
    /// <inheritdoc />
    public partial class MessageSeedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Friendships",
                keyColumns: new[] { "AddresseeUserId", "RequesterUserId" },
                keyValues: new object[] { 1, 2 },
                column: "FriendshipStatus",
                value: "Accepted");

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "CreatedAt", "IsRead", "MessageContent", "ReceiverUserId", "SenderUserId" },
                values: new object[] { 1, new DateTime(2026, 5, 14, 18, 47, 58, 0, DateTimeKind.Utc), false, "Hello first admin! i am the second admin", 1, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "Friendships",
                keyColumns: new[] { "AddresseeUserId", "RequesterUserId" },
                keyValues: new object[] { 1, 2 },
                column: "FriendshipStatus",
                value: "Pending");
        }
    }
}
