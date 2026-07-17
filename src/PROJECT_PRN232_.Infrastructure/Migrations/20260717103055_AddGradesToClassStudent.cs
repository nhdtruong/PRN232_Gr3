using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJECT_PRN232_.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGradesToClassStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalComment",
                table: "ClassStudents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalScore",
                table: "ClassStudents",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MidtermComment",
                table: "ClassStudents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MidtermScore",
                table: "ClassStudents",
                type: "decimal(4,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalComment",
                table: "ClassStudents");

            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "ClassStudents");

            migrationBuilder.DropColumn(
                name: "MidtermComment",
                table: "ClassStudents");

            migrationBuilder.DropColumn(
                name: "MidtermScore",
                table: "ClassStudents");
        }
    }
}
