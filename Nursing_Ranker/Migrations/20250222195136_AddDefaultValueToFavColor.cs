using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nursing_Ranker.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultValueToFavColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FavColor",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "white");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavColor",
                table: "Users");
        }
    }
}
