using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Hem giriş yapmış müşterilerin hem de giriş yapmamış (misafir) ziyaretçilerin sepetini temsil eder.
/// Eski misafir sepetleri ExpiresAt tarihine göre arka plan servisiyle otomatik temizlenir.
/// </summary>
public class Cart
{
    public int Id { get; set; }

    /// <summary>Giriş yapmış kullanıcının Id'si (Misafir sepetlerinde null olur).</summary>
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>Giriş yapmamış (misafir) kullanıcı için tarayıcı çerezinde tutulan benzersiz GUID değeri.</summary>
    [MaxLength(100)]
    public string? GuestId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Misafir sepetlerinin geçerlilik süresi (Varsayılan: son güncellemeden itibaren 7 gün).
    /// Süresi dolan misafir sepetleri ExpiredGuestCartCleanupService tarafından temizlenir.
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    // İlişkiler
    public ApplicationUser? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
