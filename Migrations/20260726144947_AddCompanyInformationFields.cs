using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyInformationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_line1",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line2",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cin_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_type",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "financial_year_end",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "financial_year_start",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gst_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hr_email",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "industry",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "legal_company_name",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "office_end_time",
                schema: "hrm",
                table: "companies",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "office_start_time",
                schema: "hrm",
                table: "companies",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pan_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pincode",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registration_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tan_number",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "working_days",
                schema: "hrm",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "year_established",
                schema: "hrm",
                table: "companies",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address_line1",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_line2",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "cin_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "company_type",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "country",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "financial_year_end",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "financial_year_start",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "gst_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "hr_email",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "industry",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "legal_company_name",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "logo_url",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "office_end_time",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "office_start_time",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "pan_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "phone_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "pincode",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "registration_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "state",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "tan_number",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "time_zone",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "website",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "working_days",
                schema: "hrm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "year_established",
                schema: "hrm",
                table: "companies");
        }
    }
}
