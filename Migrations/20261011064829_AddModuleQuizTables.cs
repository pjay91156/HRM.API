using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleQuizTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "module_progress",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    learning_completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    quiz_passed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_progress_course_module_module_id",
                        column: x => x.module_id,
                        principalSchema: "hrm",
                        principalTable: "course_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_module_progress_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "module_quiz",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    passing_percentage = table.Column<int>(type: "int", nullable: false),
                    max_attempts = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_course_module_module_id",
                        column: x => x.module_id,
                        principalSchema: "hrm",
                        principalTable: "course_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_quiz_attempt",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attempt_number = table.Column<int>(type: "int", nullable: false),
                    total_questions = table.Column<int>(type: "int", nullable: false),
                    correct_answers = table.Column<int>(type: "int", nullable: false),
                    score_percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    passing_percentage = table.Column<int>(type: "int", nullable: false),
                    is_passed = table.Column<bool>(type: "bit", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz_attempt", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_attempt_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_module_quiz_attempt_module_quiz_quiz_id",
                        column: x => x.quiz_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_quiz_question",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    is_multiple_answer = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz_question", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_question_module_quiz_quiz_id",
                        column: x => x.quiz_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_quiz_answer",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    is_multiple_answer = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz_answer", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_answer_module_quiz_attempt_attempt_id",
                        column: x => x.attempt_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz_attempt",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_module_quiz_answer_module_quiz_question_question_id",
                        column: x => x.question_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz_question",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "module_quiz_option",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_option_module_quiz_question_question_id",
                        column: x => x.question_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_quiz_answer_option",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    answer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    is_selected = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_quiz_answer_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_quiz_answer_option_module_quiz_answer_answer_id",
                        column: x => x.answer_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz_answer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_module_quiz_answer_option_module_quiz_option_option_id",
                        column: x => x.option_id,
                        principalSchema: "hrm",
                        principalTable: "module_quiz_option",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_module_progress_employee_id",
                schema: "hrm",
                table: "module_progress",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_progress_module_id_employee_id",
                schema: "hrm",
                table: "module_progress",
                columns: new[] { "module_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_module_id",
                schema: "hrm",
                table: "module_quiz",
                column: "module_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_answer_attempt_id_question_id",
                schema: "hrm",
                table: "module_quiz_answer",
                columns: new[] { "attempt_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_answer_question_id",
                schema: "hrm",
                table: "module_quiz_answer",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_answer_option_answer_id_option_id",
                schema: "hrm",
                table: "module_quiz_answer_option",
                columns: new[] { "answer_id", "option_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_answer_option_option_id",
                schema: "hrm",
                table: "module_quiz_answer_option",
                column: "option_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_attempt_employee_id",
                schema: "hrm",
                table: "module_quiz_attempt",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_attempt_quiz_id_employee_id_attempt_number",
                schema: "hrm",
                table: "module_quiz_attempt",
                columns: new[] { "quiz_id", "employee_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_option_question_id_sort_order",
                schema: "hrm",
                table: "module_quiz_option",
                columns: new[] { "question_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_module_quiz_question_quiz_id_sort_order",
                schema: "hrm",
                table: "module_quiz_question",
                columns: new[] { "quiz_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "module_progress",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz_answer_option",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz_answer",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz_option",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz_attempt",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz_question",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "module_quiz",
                schema: "hrm");
        }
    }
}
