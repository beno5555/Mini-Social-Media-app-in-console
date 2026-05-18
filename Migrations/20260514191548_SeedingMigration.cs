using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace social_media_console_app.Migrations
{
    /// <inheritdoc />
    public partial class SeedingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_User_CommenterUserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_User_AddresseeUserId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_User_RequesterUserId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_User_ReceiverUserId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_User_SenderUserId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_User_UserId",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_User_Username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_User_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Bio", "SentAt", "DateOfBirth", "Email", "PasswordHash", "PasswordSalt", "Username" },
                values: new object[] { 2, "I am the second admin of this app", new DateTime(2026, 5, 14, 18, 40, 30, 0, DateTimeKind.Utc), new DateTime(2005, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), "secondadmin123@gmail.com", "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg=", "second_admin" });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "CommentContent", "CommenterUserId", "SentAt", "PostId" },
                values: new object[] { 1, "Nice post!", 2, new DateTime(2026, 5, 14, 18, 42, 34, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.InsertData(
                table: "Friendships",
                columns: new[] { "AddresseeUserId", "RequesterUserId", "SentAt", "FriendshipStatus" },
                values: new object[] { 1, 2, new DateTime(2026, 5, 14, 18, 43, 58, 0, DateTimeKind.Utc), "Pending" });

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments",
                column: "CommenterUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_AddresseeUserId",
                table: "Friendships",
                column: "AddresseeUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_RequesterUserId",
                table: "Friendships",
                column: "RequesterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ReceiverUserId",
                table: "Messages",
                column: "ReceiverUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderUserId",
                table: "Messages",
                column: "SenderUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommenterUserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_AddresseeUserId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_RequesterUserId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ReceiverUserId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderUserId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Comments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Friendships",
                keyColumns: new[] { "AddresseeUserId", "RequesterUserId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "User",
                newName: "IX_User_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "User",
                newName: "IX_User_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_User_CommenterUserId",
                table: "Comments",
                column: "CommenterUserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_User_AddresseeUserId",
                table: "Friendships",
                column: "AddresseeUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_User_RequesterUserId",
                table: "Friendships",
                column: "RequesterUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_User_ReceiverUserId",
                table: "Messages",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_User_SenderUserId",
                table: "Messages",
                column: "SenderUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_User_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
