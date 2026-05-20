using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace social_media_console_app.Migrations
{
    /// <inheritdoc />
    public partial class ResetMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments");

            // migrationBuilder.DeleteData(
            //     table: "Comments",
            //     keyColumn: "Id",
            //     keyValue: 1);
            //
            // migrationBuilder.DeleteData(
            //     table: "Friendships",
            //     keyColumns: new[] { "AddresseeUserId", "RequesterUserId" },
            //     keyValues: new object[] { 1, 2 });
            //
            // migrationBuilder.DeleteData(
            //     table: "Messages",
            //     keyColumn: "Id",
            //     keyValue: 1);
            //
            // migrationBuilder.DeleteData(
            //     table: "Posts",
            //     keyColumn: "Id",
            //     keyValue: 1);
            //
            // migrationBuilder.DeleteData(
            //     table: "Users",
            //     keyColumn: "Id",
            //     keyValue: 2);
            //
            // migrationBuilder.DeleteData(
            //     table: "Users",
            //     keyColumn: "Id",
            //     keyValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Bio", "CreatedAt", "DateOfBirth", "Email", "PasswordHash", "PasswordSalt", "Username" },
                values: new object[,]
                {
                    { 1, "I am admin of this app", new DateTime(2026, 5, 14, 18, 29, 30, 0, DateTimeKind.Utc), new DateTime(2000, 4, 17, 0, 0, 0, 0, DateTimeKind.Utc), "admin123@gmail.com", "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg=", "first_admin" },
                    { 2, "I am the second admin of this app", new DateTime(2026, 5, 14, 18, 40, 30, 0, DateTimeKind.Utc), new DateTime(2005, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), "secondadmin123@gmail.com", "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg=", "second_admin" }
                });

            migrationBuilder.InsertData(
                table: "Friendships",
                columns: new[] { "AddresseeUserId", "RequesterUserId", "CreatedAt", "FriendshipStatus" },
                values: new object[] { 1, 2, new DateTime(2026, 5, 14, 18, 43, 58, 0, DateTimeKind.Utc), "Accepted" });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "CreatedAt", "IsRead", "MessageContent", "ReceiverUserId", "SenderUserId" },
                values: new object[] { 1, new DateTime(2026, 5, 14, 18, 47, 58, 0, DateTimeKind.Utc), false, "Hello first admin! i am the second admin", 1, 2 });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "CreatedAt", "PostContent", "PostTitle", "UserId" },
                values: new object[] { 1, new DateTime(2026, 5, 14, 18, 30, 58, 0, DateTimeKind.Utc), "this is initial admin post", "Initial admin post", 1 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "CommentContent", "CommenterUserId", "CreatedAt", "PostId" },
                values: new object[] { 1, "Nice post!", 2, new DateTime(2026, 5, 14, 18, 42, 34, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
