public class CompanyInformationResponse
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? LegalCompanyName { get; set; }

    public string? CompanyType { get; set; }

    public string? Industry { get; set; }

    public int? YearEstablished { get; set; }

    public string? GstNumber { get; set; }

    public string? PanNumber { get; set; }

    public string? TanNumber { get; set; }

    public string? CinNumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? HrEmail { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Website { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public string? Pincode { get; set; }

    public string? Currency { get; set; }

    public string? TimeZone { get; set; }

    public string? FinancialYearStart { get; set; }

    public string? FinancialYearEnd { get; set; }

    public List<string> WorkingDays { get; set; } = new();

    public string? OfficeStartTime { get; set; }

    public string? OfficeEndTime { get; set; }

    public string? Description { get; set; }

    public bool TwoFactorMandatory { get; set; }
}
