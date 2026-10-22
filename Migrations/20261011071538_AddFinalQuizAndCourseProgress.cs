using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalQuizAndCourseProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_module_quiz_course_module_module_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.DropIndex(
                name: "IX_module_quiz_module_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.AlterColumn<Guid>(
                name: "module_id",
                schema: "hrm",
                table: "module_quiz",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "course_id",
                schema: "hrm",
                table: "module_quiz",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "course_progress",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_progress_course_course_id",
                        column: x => x.course_id,
                        principalSchema: "hrm",
                        principalTable: "course",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_progress_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_course_id",
                schema: "hrm",
                table: "module_quiz",
                column: "course_id",
                unique: true,
                filter: "[course_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_module_id",
                schema: "hrm",
                table: "module_quiz",
                column: "module_id",
                unique: true,
                filter: "[module_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_course_progress_course_id_employee_id",
                schema: "hrm",
                table: "course_progress",
                columns: new[] { "course_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_progress_employee_id",
                schema: "hrm",
                table: "course_progress",
                column: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_module_quiz_course_course_id",
                schema: "hrm",
                table: "module_quiz",
                column: "course_id",
                principalSchema: "hrm",
                principalTable: "course",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_module_quiz_course_module_module_id",
                schema: "hrm",
                table: "module_quiz",
                column: "module_id",
                principalSchema: "hrm",
                principalTable: "course_module",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_module_quiz_course_course_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.DropForeignKey(
                name: "FK_module_quiz_course_module_module_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.DropTable(
                name: "course_progress",
                schema: "hrm");

            migrationBuilder.DropIndex(
                name: "IX_module_quiz_course_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.DropIndex(
                name: "IX_module_quiz_module_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.DropColumn(
                name: "course_id",
                schema: "hrm",
                table: "module_quiz");

            migrationBuilder.AlterColumn<Guid>(
                name: "module_id",
                schema: "hrm",
                table: "module_quiz",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_module_id",
                schema: "hrm",
                table: "module_quiz",
                column: "module_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_module_quiz_course_module_module_id",
                schema: "hrm",
                table: "module_quiz",
                column: "module_id",
                principalSchema: "hrm",
                principalTable: "course_module",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
