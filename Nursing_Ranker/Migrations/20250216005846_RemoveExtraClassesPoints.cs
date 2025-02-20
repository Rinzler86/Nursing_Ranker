using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nursing_Ranker.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraClassesPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraClassesPoints",
                table: "ApplicantCourses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraClassesPoints",
                table: "ApplicantCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
