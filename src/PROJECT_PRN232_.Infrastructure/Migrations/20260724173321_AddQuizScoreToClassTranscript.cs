using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJECT_PRN232_.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizScoreToClassTranscript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuizComment",
                table: "ClassTranscript",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuizScore",
                table: "ClassTranscript",
                type: "decimal(4,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizComment",
                table: "ClassTranscript");

            migrationBuilder.DropColumn(
                name: "QuizScore",
                table: "ClassTranscript");
        }
    }
}
