using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface ICompanyDocumentService
{
    Task<ApiResponse<List<CompanyDocumentResponse>>> GetAllAsync();

    Task<ApiResponse<CompanyDocumentResponse>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> CreateAsync(CreateCompanyDocumentDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateCompanyDocumentDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);

    Task<ApiResponse<CompanyDocumentDownloadInfo>> GetDownloadInfoAsync(Guid id);
}

public class CompanyDocumentService : ICompanyDocumentService
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 1000;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    // Stored outside wwwroot so files are never reachable via static file hosting;
    // downloads must go through the authorized download endpoint.
    private const string StorageRootFolder = "UploadedFiles";
    private const string StorageSubFolder = "company-documents";

    private static readonly string[] AllowedExtensions =
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png"
    };

    private readonly ICompanyDocumentRepository _companyDocumentRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CompanyDocumentService(
        ICompanyDocumentRepository companyDocumentRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _companyDocumentRepository = companyDocumentRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    private static CompanyDocumentResponse ToResponse(CompanyDocument document)
    {
        return new CompanyDocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            DocumentType = (int)document.DocumentType,
            DocumentTypeName = document.DocumentType.ToString(),
            FileName = document.FileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            IsActive = document.IsActive,
            CreatedAt = document.CreatedAt
        };
    }

    private string GetStorageFolder()
    {
        var folder = Path.Combine(_webHostEnvironment.ContentRootPath, StorageRootFolder, StorageSubFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public async Task<ApiResponse<List<CompanyDocumentResponse>>> GetAllAsync()
    {
        try
        {
            var documents = await _companyDocumentRepository.GetAllAsync();

            return new ApiResponse<List<CompanyDocumentResponse>>
            {
                Success = true,
                Message = "Company documents retrieved successfully.",
                Data = documents.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<CompanyDocumentResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving company documents.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CompanyDocumentResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var document = await _companyDocumentRepository.GetByIdAsync(id);

            if (document == null)
            {
                return new ApiResponse<CompanyDocumentResponse>
                {
                    Success = false,
                    Message = "Company document not found."
                };
            }

            return new ApiResponse<CompanyDocumentResponse>
            {
                Success = true,
                Message = "Company document retrieved successfully.",
                Data = ToResponse(document)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyDocumentResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the company document.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateCompanyDocumentDto request, Guid userId)
    {
        try
        {
            var titleValidation = ValidateTitle(request.Title, out var trimmedTitle);

            if (titleValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = titleValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description, out var trimmedDescription);

            if (descriptionValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = descriptionValidation };
            }

            var documentTypeValidation = ValidateDocumentType(request.DocumentType);

            if (documentTypeValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = documentTypeValidation };
            }

            var fileValidation = ValidateFile(request.File, out var extension);

            if (fileValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = fileValidation };
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(GetStorageFolder(), storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var document = new CompanyDocument
            {
                Id = Guid.NewGuid(),
                Title = trimmedTitle,
                Description = trimmedDescription,
                DocumentType = request.DocumentType,
                FileName = request.File.FileName,
                FilePath = storedFileName,
                FileExtension = extension,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _companyDocumentRepository.AddAsync(document);

            await _companyDocumentRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Company document uploaded successfully.",
                Data = document.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while uploading the company document.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateCompanyDocumentDto request, Guid userId)
    {
        try
        {
            var document = await _companyDocumentRepository.GetByIdAsync(request.Id);

            if (document == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Company document not found."
                };
            }

            var titleValidation = ValidateTitle(request.Title, out var trimmedTitle);

            if (titleValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = titleValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description, out var trimmedDescription);

            if (descriptionValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = descriptionValidation };
            }

            var documentTypeValidation = ValidateDocumentType(request.DocumentType);

            if (documentTypeValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = documentTypeValidation };
            }

            string? newStoredFileName = null;
            string? newExtension = null;

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File, out newExtension);

                if (fileValidation != null)
                {
                    return new ApiResponse<bool> { Success = false, Message = fileValidation };
                }

                newStoredFileName = $"{Guid.NewGuid()}{newExtension}";
                var filePath = Path.Combine(GetStorageFolder(), newStoredFileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }
            }

            document.Title = trimmedTitle;
            document.Description = trimmedDescription;
            document.DocumentType = request.DocumentType;
            document.UpdatedAt = DateTime.UtcNow;
            document.UpdatedBy = userId;

            if (newStoredFileName != null && request.File != null)
            {
                var oldFilePath = Path.Combine(GetStorageFolder(), document.FilePath);

                document.FileName = request.File.FileName;
                document.FilePath = newStoredFileName;
                document.FileExtension = newExtension!;
                document.ContentType = request.File.ContentType;
                document.FileSize = request.File.Length;

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            _companyDocumentRepository.Update(document);

            await _companyDocumentRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Company document updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the company document.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var document = await _companyDocumentRepository.GetByIdAsync(id);

            if (document == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Company document not found."
                };
            }

            document.IsActive = false;
            document.UpdatedAt = DateTime.UtcNow;
            document.UpdatedBy = userId;

            _companyDocumentRepository.Update(document);

            await _companyDocumentRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Company document deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the company document.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CompanyDocumentDownloadInfo>> GetDownloadInfoAsync(Guid id)
    {
        try
        {
            var document = await _companyDocumentRepository.GetByIdAsync(id);

            if (document == null)
            {
                return new ApiResponse<CompanyDocumentDownloadInfo>
                {
                    Success = false,
                    Message = "Company document not found."
                };
            }

            var absolutePath = Path.Combine(GetStorageFolder(), document.FilePath);

            if (!File.Exists(absolutePath))
            {
                return new ApiResponse<CompanyDocumentDownloadInfo>
                {
                    Success = false,
                    Message = "The document file could not be found on the server."
                };
            }

            return new ApiResponse<CompanyDocumentDownloadInfo>
            {
                Success = true,
                Message = "Document ready for download.",
                Data = new CompanyDocumentDownloadInfo
                {
                    AbsolutePath = absolutePath,
                    ContentType = document.ContentType,
                    FileName = document.FileName
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyDocumentDownloadInfo>
            {
                Success = false,
                Message = "An error occurred while preparing the document for download.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateTitle(string? title, out string trimmedTitle)
    {
        trimmedTitle = title?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedTitle))
        {
            return "Title is required and cannot contain only spaces.";
        }

        if (trimmedTitle.Length > MaxTitleLength)
        {
            return $"Title cannot exceed {MaxTitleLength} characters.";
        }

        return null;
    }

    private static string? ValidateDescription(string? description, out string? trimmedDescription)
    {
        trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedDescription != null && trimmedDescription.Length > MaxDescriptionLength)
        {
            return $"Description cannot exceed {MaxDescriptionLength} characters.";
        }

        return null;
    }

    private static string? ValidateDocumentType(DocumentType documentType)
    {
        if (!Enum.IsDefined(typeof(DocumentType), documentType))
        {
            return "Please select a valid document category.";
        }

        return null;
    }

    private static string? ValidateFile(IFormFile? file, out string extension)
    {
        extension = string.Empty;

        if (file == null || file.Length == 0)
        {
            return "Please choose a file to upload.";
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return "File must be 10 MB or smaller.";
        }

        extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return "Only PDF, Word, Excel, PowerPoint, JPG and PNG files are allowed.";
        }

        return null;
    }
}
