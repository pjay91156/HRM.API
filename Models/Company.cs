using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("companies", Schema = "hrm")]
public class Company : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("logo_url")]
    public string? LogoUrl { get; set; }

    [Column("legal_company_name")]
    public string? LegalCompanyName { get; set; }

    [Column("company_type")]
    public string? CompanyType { get; set; }

    [Column("industry")]
    public string? Industry { get; set; }

    [Column("year_established")]
    public int? YearEstablished { get; set; }

    [Column("gst_number")]
    public string? GstNumber { get; set; }

    [Column("pan_number")]
    public string? PanNumber { get; set; }

    [Column("tan_number")]
    public string? TanNumber { get; set; }

    [Column("cin_number")]
    public string? CinNumber { get; set; }

    [Column("registration_number")]
    public string? RegistrationNumber { get; set; }

    [Column("hr_email")]
    public string? HrEmail { get; set; }

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("website")]
    public string? Website { get; set; }

    [Column("address_line1")]
    public string? AddressLine1 { get; set; }

    [Column("address_line2")]
    public string? AddressLine2 { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("state")]
    public string? State { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("pincode")]
    public string? Pincode { get; set; }

    [Column("currency")]
    public string? Currency { get; set; }

    [Column("time_zone")]
    public string? TimeZone { get; set; }

    [Column("financial_year_start")]
    public string? FinancialYearStart { get; set; }

    [Column("financial_year_end")]
    public string? FinancialYearEnd { get; set; }

    [Column("working_days")]
    public string? WorkingDays { get; set; }

    [Column("office_start_time")]
    public TimeOnly? OfficeStartTime { get; set; }

    [Column("office_end_time")]
    public TimeOnly? OfficeEndTime { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("two_factor_mandatory")]
    public bool TwoFactorMandatory { get; set; }
}