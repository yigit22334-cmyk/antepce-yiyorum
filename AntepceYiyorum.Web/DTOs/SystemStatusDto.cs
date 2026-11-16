namespace AntepceYiyorum.Web.DTOs;

/// <summary>
/// Mobil (Android) ve Web istemcileri için sistem ve katalog durumunu bildiren JSON yanıt modeli.
/// </summary>
public class SystemStatusDto
{
    public string Application { get; set; } = "Antepçe Yiyorum E-Ticaret Platformu";
    public string Status { get; set; } = "Healthy";
    public int CategoryCount { get; set; }
    public int ActiveProductCount { get; set; }
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
}
