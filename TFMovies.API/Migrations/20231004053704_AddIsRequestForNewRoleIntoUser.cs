using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TFMovies.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRequestForNewRoleIntoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequestForNewRole",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRequestForNewRole",
                table: "Users");
        }
    }
}
