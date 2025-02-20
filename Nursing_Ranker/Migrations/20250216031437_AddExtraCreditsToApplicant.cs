using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nursing_Ranker.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraCreditsToApplicant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraCredits",
                table: "Applicants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraCredits",
                table: "Applicants");
        }
    }
}
