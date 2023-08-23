using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TFMovies.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToUserSecretToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSecretTokens_UserId",
                table: "UserSecretTokens");

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "UserSecretTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "JwtRefreshToken",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "UX_UserSecretToken_UserId_TokenType",
                table: "UserSecretTokens",
                columns: new[] { "UserId", "TokenType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_UserSecretToken_UserId_TokenType",
                table: "UserSecretTokens");

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "UserSecretTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "JwtRefreshToken",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSecretTokens_UserId",
                table: "UserSecretTokens",
                column: "UserId");
        }
    }
}
