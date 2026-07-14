using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJECT_PRN232_.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassTransferRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassTransferRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    FromTeacherId = table.Column<int>(type: "int", nullable: false),
                    ToTeacherId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTransferRequests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTransferRequests_Users_FromTeacherId",
                        column: x => x.FromTeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTransferRequests_Users_ToTeacherId",
                        column: x => x.ToTeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransferRequests_ClassId",
                table: "ClassTransferRequests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransferRequests_FromTeacherId",
                table: "ClassTransferRequests",
                column: "FromTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransferRequests_ToTeacherId",
                table: "ClassTransferRequests",
                column: "ToTeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassTransferRequests");
        }
    }
}
