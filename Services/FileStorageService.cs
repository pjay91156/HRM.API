namespace HRM.API.Services;

// Private file storage under the folder configured in appsettings.json (FileStorage:RootPath).
// Callers only ever deal in paths relative to that root.
public interface IFileStorageService
{
    Task SaveAsync(IFormFile file, string relativePath);

    Stream? OpenRead(string? relativePath);

    void Delete(string? relativePath);
}

public class FileStorageService : IFileStorageService
{
    private const string StorageRootPathKey = "FileStorage:RootPath";

    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SaveAsync(IFormFile file, string relativePath)
    {
        var absolutePath = Resolve(relativePath)
            ?? throw new InvalidOperationException("The file path is outside the storage folder.");

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        try
        {
            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
        }
        catch
        {
            // Don't leave a partial file behind when the upload is interrupted.
            Delete(relativePath);
            throw;
        }
    }

    public Stream? OpenRead(string? relativePath)
    {
        var absolutePath = Resolve(relativePath);

        if (absolutePath == null || !File.Exists(absolutePath))
        {
            return null;
        }

        // Delete sharing lets a file be replaced while someone is still watching the old one.
        return new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 64 * 1024,
            useAsync: true);
    }

    public void Delete(string? relativePath)
    {
        try
        {
            var absolutePath = Resolve(relativePath);

            if (absolutePath != null && File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
        catch (Exception ex)
        {
            // Cleanup is best effort; a leftover file must not fail the request.
            _logger.LogWarning(ex, "Could not delete stored file {RelativePath}", relativePath);
        }
    }

    // Returns the absolute path for a relative path, or null when the value
    // is empty or would resolve outside the storage root.
    private string? Resolve(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var root = GetRoot();
        var absolutePath = Path.GetFullPath(Path.Combine(root, relativePath));

        if (!absolutePath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return absolutePath;
    }

    private string GetRoot()
    {
        var rootPath = _configuration[StorageRootPathKey];

        if (string.IsNullOrWhiteSpace(rootPath) || !Path.IsPathFullyQualified(rootPath))
        {
            throw new InvalidOperationException(
                $"'{StorageRootPathKey}' must be set to an absolute folder path in appsettings.json.");
        }

        return Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
