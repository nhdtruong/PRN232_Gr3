using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJECT_PRN232_.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assessments_StudentId",
                table: "Assessments");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentId_LessonId",
                table: "Assessments",
                columns: new[] { "StudentId", "LessonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assessments_StudentId_LessonId",
                table: "Assessments");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentId",
                table: "Assessments",
                column: "StudentId");
        }
    }
}
