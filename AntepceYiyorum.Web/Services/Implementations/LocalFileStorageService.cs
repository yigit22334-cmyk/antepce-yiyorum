using AntepceYiyorum.Web.Services.Interfaces;

namespace AntepceYiyorum.Web.Services.Implementations;

/// <summary>
/// Ürün fotoğraflarını wwwroot/uploads klasörü altında güvenli şekilde saklayan servis sınıfı.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<(bool Success, string? FileUrl, string? ErrorMessage)> SaveImageAsync(
        IFormFile file,
        string subFolder = "products",
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return (false, null, "Yüklenecek dosya seçilmedi veya dosya boş.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, null, "Görsel boyutu en fazla 5 MB olabilir.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, null, "Sadece .jpg, .jpeg, .png, .webp, .gif veya .svg uzantılı görseller yüklenebilir.");
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(uploadsFolder, safeFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/{subFolder}/{safeFileName}";
        _logger.LogInformation("Yeni görsel yüklendi: {RelativeUrl}", relativeUrl);

        return (true, relativeUrl, null);
    }

    public void DeleteImageIfUploaded(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("Eski yüklenen görsel silindi: {ImageUrl}", imageUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Eski görsel silinirken hata oluştu: {ImageUrl}", imageUrl);
        }
    }
}
