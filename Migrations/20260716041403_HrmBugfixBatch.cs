using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class HrmBugfixBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "hrm",
                table: "users",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "hrm",
                table: "users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "hrm",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                schema: "hrm",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                schema: "hrm",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "hrm",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "hrm",
                table: "users",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "hrm",
                table: "users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "company_id",
                schema: "hrm",
                table: "users",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "hrm",
                table: "users",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "hrm",
                table: "employee",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                schema: "hrm",
                table: "employee",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "joining_date",
                schema: "hrm",
                table: "employee",
                type: "date",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "employee",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "employee_code",
                schema: "hrm",
                table: "employee",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "designation_id",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "department_id",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "hrm",
                table: "employee",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "manager_id",
                schema: "hrm",
                table: "employee",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "hrm",
                table: "designation",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "hrm",
                table: "designation",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "designation",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "designation_name",
                schema: "hrm",
                table: "designation",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "department_id",
                schema: "hrm",
                table: "designation",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "hrm",
                table: "designation",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "hrm",
                table: "designation",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "hrm",
                table: "designation",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "hrm",
                table: "department",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "hrm",
                table: "department",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "department",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "hrm",
                table: "department",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "hrm",
                table: "department",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "hrm",
                table: "department",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "company_id",
                schema: "hrm",
                table: "department",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "hrm",
                table: "department",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "hrm",
                table: "companies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "hrm",
                table: "companies",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "companies",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "hrm",
                table: "companies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "hrm",
                table: "companies",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "company_name",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "hrm",
                table: "companies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "attendance",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_working_hours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leave_request",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    leave_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    leave_duration = table.Column<int>(type: "int", nullable: false),
                    half_day_period = table.Column<int>(type: "int", nullable: true),
                    total_days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    approved_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approver_comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leave_type",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    leave_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    leave_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    default_days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    link = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "hrm",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "performance_cycle",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cycle_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    review_period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    review_period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    employee_review_start = table.Column<DateOnly>(type: "date", nullable: false),
                    employee_review_end = table.Column<DateOnly>(type: "date", nullable: false),
                    manager_review_start = table.Column<DateOnly>(type: "date", nullable: false),
                    manager_review_end = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_cycle", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "performance_rating",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    rating_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_rating", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "performance_template",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    template_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    department_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_template", x => x.id);
                    table.ForeignKey(
                        name: "FK_performance_template_department_department_id",
                        column: x => x.department_id,
                        principalSchema: "hrm",
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_token_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "hrm",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendance_session",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attendance_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_number = table.Column<int>(type: "int", nullable: false),
                    check_in_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check_out_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    working_hours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_session_attendance_attendance_id",
                        column: x => x.attendance_id,
                        principalSchema: "hrm",
                        principalTable: "attendance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_performance_review",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    performance_cycle_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    manager_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    submitted_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    manager_reviewed_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    overall_employee_comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    overall_manager_comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    overall_score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_performance_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_performance_review_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_performance_review_employee_manager_id",
                        column: x => x.manager_id,
                        principalSchema: "hrm",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_performance_review_performance_cycle_performance_cycle_id",
                        column: x => x.performance_cycle_id,
                        principalSchema: "hrm",
                        principalTable: "performance_cycle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "performance_category",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    performance_template_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    weightage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_performance_category_performance_template_performance_template_id",
                        column: x => x.performance_template_id,
                        principalSchema: "hrm",
                        principalTable: "performance_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendance_regularization",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attendance_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attendance_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    after_check_in = table.Column<TimeOnly>(type: "time", nullable: true),
                    after_check_out = table.Column<TimeOnly>(type: "time", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_regularization", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_regularization_attendance_attendance_id",
                        column: x => x.attendance_id,
                        principalSchema: "hrm",
                        principalTable: "attendance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attendance_regularization_attendance_session_attendance_session_id",
                        column: x => x.attendance_session_id,
                        principalSchema: "hrm",
                        principalTable: "attendance_session",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_attendance_regularization_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "hrm",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attendance_regularization_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "hrm",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "performance_skill",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    performance_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    skill_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    weightage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_skill", x => x.id);
                    table.ForeignKey(
                        name: "FK_performance_skill_performance_category_performance_category_id",
                        column: x => x.performance_category_id,
                        principalSchema: "hrm",
                        principalTable: "performance_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_performance_skill_review",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_performance_review_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    performance_skill_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    employee_comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    manager_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    manager_comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_performance_skill_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_performance_skill_review_employee_performance_review_employee_performance_review_id",
                        column: x => x.employee_performance_review_id,
                        principalSchema: "hrm",
                        principalTable: "employee_performance_review",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_performance_skill_review_performance_skill_performance_skill_id",
                        column: x => x.performance_skill_id,
                        principalSchema: "hrm",
                        principalTable: "performance_skill",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_manager_id",
                schema: "hrm",
                table: "employee",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_employee_id",
                schema: "hrm",
                table: "attendance",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_regularization_attendance_id",
                schema: "hrm",
                table: "attendance_regularization",
                column: "attendance_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_regularization_attendance_session_id",
                schema: "hrm",
                table: "attendance_regularization",
                column: "attendance_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_regularization_created_by",
                schema: "hrm",
                table: "attendance_regularization",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_regularization_updated_by",
                schema: "hrm",
                table: "attendance_regularization",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_session_attendance_id",
                schema: "hrm",
                table: "attendance_session",
                column: "attendance_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_performance_review_employee_id",
                schema: "hrm",
                table: "employee_performance_review",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_performance_review_manager_id",
                schema: "hrm",
                table: "employee_performance_review",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_performance_review_performance_cycle_id",
                schema: "hrm",
                table: "employee_performance_review",
                column: "performance_cycle_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_performance_skill_review_employee_performance_review_id",
                schema: "hrm",
                table: "employee_performance_skill_review",
                column: "employee_performance_review_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_performance_skill_review_performance_skill_id",
                schema: "hrm",
                table: "employee_performance_skill_review",
                column: "performance_skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id",
                schema: "hrm",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_performance_category_performance_template_id",
                schema: "hrm",
                table: "performance_category",
                column: "performance_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_performance_skill_performance_category_id",
                schema: "hrm",
                table: "performance_skill",
                column: "performance_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_performance_template_department_id",
                schema: "hrm",
                table: "performance_template",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                schema: "hrm",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_employee_manager_id",
                schema: "hrm",
                table: "employee",
                column: "manager_id",
                principalSchema: "hrm",
                principalTable: "employee",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_employee_manager_id",
                schema: "hrm",
                table: "employee");

            migrationBuilder.DropTable(
                name: "attendance_regularization",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "employee_performance_skill_review",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "leave_request",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "leave_type",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "notification",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "performance_rating",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "attendance_session",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "employee_performance_review",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "performance_skill",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "attendance",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "performance_cycle",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "performance_category",
                schema: "hrm");

            migrationBuilder.DropTable(
                name: "performance_template",
                schema: "hrm");

            migrationBuilder.DropIndex(
                name: "IX_employee_manager_id",
                schema: "hrm",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "manager_id",
                schema: "hrm",
                table: "employee");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "updated_at",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_at",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "company_id",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "hrm",
                table: "users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "updated_at",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "joining_date",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "employee",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "employee_code",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "designation_id",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "department_id",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_at",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "hrm",
                table: "employee",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "updated_at",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "designation",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "designation_name",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "department_id",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_at",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "hrm",
                table: "designation",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "updated_at",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "department",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "department_name",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_at",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "company_id",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "hrm",
                table: "department",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "updated_at",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "hrm",
                table: "companies",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "created_at",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "company_name",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "hrm",
                table: "companies",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
