using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Yöresel ürün kategorilerini temsil eder (Örn: Antep Fıstığı ve Kuruyemiş, Salça ve Kurutmalık).
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>SEO dostu URL anahtarı (Örn: antep-fistigi-ve-kuruyemis).</summary>
    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    /// <summary>Listeleme sırası.</summary>
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // İlişkiler
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
