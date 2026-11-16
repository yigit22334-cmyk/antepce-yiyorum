namespace AntepceYiyorum.Web.Services.Interfaces;

/// <summary>
/// Ürün ve kategori fotoğraflarının güvenli şekilde sunucuya kaydedilmesi ve silinmesi için servis arayüzü.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Yüklenen görsel dosyasını doğrular, benzersiz bir adla wwwroot/uploads/{subFolder} altına kaydeder
    /// ve web üzerinden erişilebilir göreli URL'ini (Örn: /uploads/products/abc.jpg) döner.
    /// </summary>
    Task<(bool Success, string? FileUrl, string? ErrorMessage)> SaveImageAsync(
        IFormFile file,
        string subFolder = "products",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Daha önce yüklenmiş olan (/uploads/ ile başlayan) bir görsel dosyasını sunucudan siler.
    /// </summary>
    void DeleteImageIfUploaded(string? imageUrl);
}
