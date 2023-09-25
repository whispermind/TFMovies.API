using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TFMovies.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangePostTagKeyFromCompositeToSingle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PostTags",
                table: "PostTags");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "PostTags",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql("UPDATE PostTags SET Id = NEWID() WHERE Id IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "PostTags",
                type: "nvarchar(450)",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostTags",
                table: "PostTags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PostTags_PostId_TagId",
                table: "PostTags",
                columns: new[] { "PostId", "TagId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PostTags",
                table: "PostTags");

            migrationBuilder.DropIndex(
                name: "IX_PostTags_PostId_TagId",
                table: "PostTags");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PostTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostTags",
                table: "PostTags",
                columns: new[] { "PostId", "TagId" });
        }
    }
}
