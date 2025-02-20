using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nursing_Ranker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WSCCGPA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NursingGPA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MissingRequirements = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantCourses",
                columns: table => new
                {
                    ApplicantCourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<int>(type: "int", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    RequiresRetake = table.Column<bool>(type: "bit", nullable: false),
                    PointsAwarded = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantCourses", x => x.ApplicantCourseId);
                    table.ForeignKey(
                        name: "FK_ApplicantCourses_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantRequirements",
                columns: table => new
                {
                    RequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<int>(type: "int", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TestDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantRequirements", x => x.RequirementId);
                    table.ForeignKey(
                        name: "FK_ApplicantRequirements_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantCourses_ApplicantId",
                table: "ApplicantCourses",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantRequirements_ApplicantId",
                table: "ApplicantRequirements",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantCourses");

            migrationBuilder.DropTable(
                name: "ApplicantRequirements");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Applicants");
        }
    }
}
