using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HRM.API.Services;

public interface ICompanyService
{
    Task<ApiResponse<CompanyInformationResponse>> GetCompanyInformationAsync(Guid companyId);

    Task<ApiResponse<CompanyInformationResponse>> UpdateCompanyInformationAsync(
        Guid companyId,
        UpdateCompanyInformationDto request,
        Guid userId);

    Task<ApiResponse<CompanyInformationResponse>> UpdateCompanyLogoAsync(
        Guid companyId,
        IFormFile file,
        Guid userId);
}

public class CompanyService : ICompanyService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly ICompanyRepository _companyRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CompanyService(ICompanyRepository companyRepository, IWebHostEnvironment webHostEnvironment)
    {
        _companyRepository = companyRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    private static CompanyInformationResponse ToResponse(Company company)
    {
        return new CompanyInformationResponse
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            LogoUrl = company.LogoUrl,
            LegalCompanyName = company.LegalCompanyName,
            CompanyType = company.CompanyType,
            Industry = company.Industry,
            YearEstablished = company.YearEstablished,
            GstNumber = company.GstNumber,
            PanNumber = company.PanNumber,
            TanNumber = company.TanNumber,
            CinNumber = company.CinNumber,
            RegistrationNumber = company.RegistrationNumber,
            Email = company.Email,
            HrEmail = company.HrEmail,
            PhoneNumber = company.PhoneNumber,
            Website = company.Website,
            AddressLine1 = company.AddressLine1,
            AddressLine2 = company.AddressLine2,
            Country = company.Country,
            State = company.State,
            City = company.City,
            Pincode = company.Pincode,
            Currency = company.Currency,
            TimeZone = company.TimeZone,
            FinancialYearStart = company.FinancialYearStart,
            FinancialYearEnd = company.FinancialYearEnd,
            WorkingDays = string.IsNullOrWhiteSpace(company.WorkingDays)
                ? new List<string>()
                : company.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            OfficeStartTime = company.OfficeStartTime?.ToString("HH:mm"),
            OfficeEndTime = company.OfficeEndTime?.ToString("HH:mm"),
            Description = company.Description,
            TwoFactorMandatory = company.TwoFactorMandatory
        };
    }

    public async Task<ApiResponse<CompanyInformationResponse>> GetCompanyInformationAsync(Guid companyId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
            {
                return new ApiResponse<CompanyInformationResponse>
                {
                    Success = false,
                    Message = "Company not found."
                };
            }

            return new ApiResponse<CompanyInformationResponse>
            {
                Success = true,
                Message = "Company information retrieved successfully.",
                Data = ToResponse(company)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyInformationResponse>
            {
                Success = false,
                Message = "Error while retrieving company information.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CompanyInformationResponse>> UpdateCompanyInformationAsync(
        Guid companyId,
        UpdateCompanyInformationDto request,
        Guid userId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
            {
                return new ApiResponse<CompanyInformationResponse>
                {
                    Success = false,
                    Message = "Company not found."
                };
            }

            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return new ApiResponse<CompanyInformationResponse>
                {
                    Success = false,
                    Message = "Company name is required."
                };
            }

            company.CompanyName = request.CompanyName.Trim();
            company.LegalCompanyName = request.LegalCompanyName;
            company.CompanyType = request.CompanyType;
            company.Industry = request.Industry;
            company.YearEstablished = request.YearEstablished;
            company.GstNumber = request.GstNumber;
            company.PanNumber = request.PanNumber;
            company.TanNumber = request.TanNumber;
            company.CinNumber = request.CinNumber;
            company.RegistrationNumber = request.RegistrationNumber;
            company.Email = request.Email;
            company.HrEmail = request.HrEmail;
            company.PhoneNumber = request.PhoneNumber;
            company.Website = request.Website;
            company.AddressLine1 = request.AddressLine1;
            company.AddressLine2 = request.AddressLine2;
            company.Country = request.Country;
            company.State = request.State;
            company.City = request.City;
            company.Pincode = request.Pincode;
            company.Currency = request.Currency;
            company.TimeZone = request.TimeZone;
            company.FinancialYearStart = request.FinancialYearStart;
            company.FinancialYearEnd = request.FinancialYearEnd;
            company.WorkingDays = request.WorkingDays == null
                ? null
                : string.Join(",", request.WorkingDays);
            company.OfficeStartTime = string.IsNullOrWhiteSpace(request.OfficeStartTime)
                ? null
                : TimeOnly.Parse(request.OfficeStartTime);
            company.OfficeEndTime = string.IsNullOrWhiteSpace(request.OfficeEndTime)
                ? null
                : TimeOnly.Parse(request.OfficeEndTime);
            company.Description = request.Description;
            company.TwoFactorMandatory = request.TwoFactorMandatory;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = userId;

            await _companyRepository.SaveChangesAsync();

            return new ApiResponse<CompanyInformationResponse>
            {
                Success = true,
                Message = "Company information updated successfully.",
                Data = ToResponse(company)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyInformationResponse>
            {
                Success = false,
                Message = "Error while updating company information.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CompanyInformationResponse>> UpdateCompanyLogoAsync(
        Guid companyId,
        IFormFile file,
        Guid userId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<CompanyInformationResponse> { Success = false, Message = "Please choose a logo to upload." };
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return new ApiResponse<CompanyInformationResponse> { Success = false, Message = "Logo must be 2 MB or smaller." };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return new ApiResponse<CompanyInformationResponse> { Success = false, Message = "Only JPG, PNG, SVG, and WEBP images are allowed." };
            }

            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
            {
                return new ApiResponse<CompanyInformationResponse> { Success = false, Message = "Company not found." };
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", "company-logos");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var existingFile in Directory.GetFiles(uploadsFolder, $"{companyId}.*"))
            {
                File.Delete(existingFile);
            }

            var fileName = $"{companyId}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            company.LogoUrl = $"/uploads/company-logos/{fileName}";
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = userId;

            await _companyRepository.SaveChangesAsync();

            return new ApiResponse<CompanyInformationResponse>
            {
                Success = true,
                Message = "Company logo updated successfully.",
                Data = ToResponse(company)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyInformationResponse>
            {
                Success = false,
                Message = "Error while updating company logo.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
