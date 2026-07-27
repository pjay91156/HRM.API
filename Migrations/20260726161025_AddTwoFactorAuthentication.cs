using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "two_factor_enabled",
                schema: "hrm",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "two_factor_mandatory",
                schema: "hrm",
                table: "companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "login_otp",
                schema: "hrm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code_hash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_otp", x => x.id);
                    table.ForeignKey(
                        name: "FK_login_otp_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "hrm",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_login_otp_user_id",
                schema: "hrm",
                table: "login_otp",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_otp",
                schema: "hrm");

            migrationBuilder.DropColumn(
                name: "two_factor_enabled",
                schema: "hrm",
                table: "users");

            migrationBuilder.DropColumn(
                name: "two_factor_mandatory",
                schema: "hrm",
                table: "companies");
        }
    }
}
