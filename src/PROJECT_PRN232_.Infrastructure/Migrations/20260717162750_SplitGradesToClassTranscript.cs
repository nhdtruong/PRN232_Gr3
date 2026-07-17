using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJECT_PRN232_.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitGradesToClassTranscript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessments");

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

            migrationBuilder.CreateTable(
                name: "ClassTranscript",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    MidTermScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    MidTermComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    FinalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AverageDailyScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    FinalScoreTotal = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    ClassId1 = table.Column<int>(type: "int", nullable: true),
                    StudentId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTranscript", x => new { x.ClassId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_ClassTranscript_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTranscript_Classes_ClassId1",
                        column: x => x.ClassId1,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassTranscript_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTranscript_Students_StudentId1",
                        column: x => x.StudentId1,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailyAssessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAssessed = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyAssessment_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyAssessment_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTranscript_ClassId1",
                table: "ClassTranscript",
                column: "ClassId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTranscript_StudentId",
                table: "ClassTranscript",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTranscript_StudentId1",
                table: "ClassTranscript",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAssessment_LessonId",
                table: "DailyAssessment",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAssessment_StudentId_LessonId",
                table: "DailyAssessment",
                columns: new[] { "StudentId", "LessonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassTranscript");

            migrationBuilder.DropTable(
                name: "DailyAssessment");

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

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    DateAssessed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    TeacherComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_LessonId",
                table: "Assessments",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentId_LessonId",
                table: "Assessments",
                columns: new[] { "StudentId", "LessonId" },
                unique: true);
        }
    }
}
