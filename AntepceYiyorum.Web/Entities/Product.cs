using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Gaziantep yöresel ürünlerini temsil eden varlık sınıfı.
/// Eş zamanlı stok güncellemelerinde veri bütünlüğü için RowVersion (Optimistic Concurrency) içerir.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    /// <summary>SEO dostu benzersiz ürün URL anahtarı.</summary>
    [Required]
    [MaxLength(180)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Kart görünümlerinde gösterilecek kısa açıklama.</summary>
    [Required(ErrorMessage = "Kısa açıklama zorunludur.")]
    [MaxLength(300)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Ürün detay sayfasında gösterilecek kapsamlı açıklama.</summary>
    [MaxLength(2500)]
    public string? Description { get; set; }

    /// <summary>Birim satış fiyatı (TL).</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    /// <summary>Gramaj / miktar sayısal değeri (Örn: 500, 1000).</summary>
    public int WeightGrams { get; set; } = 500;

    /// <summary>Kullanıcıya gösterilecek gramaj etiketi (Örn: "500 g", "1 kg", "750 ml", "50 Adet (1 Bağ)").</summary>
    [Required]
    [MaxLength(50)]
    public string WeightLabel { get; set; } = "500 g";

    /// <summary>Mevcut stok adedi.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
    public int StockQuantity { get; set; }

    /// <summary>Stok bu değerin altına veya eşitine düştüğünde Admin panelinde "Kritik Stok" uyarısı verilir.</summary>
    public int LowStockThreshold { get; set; } = 10;

    [MaxLength(350)]
    public string? ImageUrl { get; set; }

    /// <summary>Ana sayfada öne çıkan ürün olarak gösterilsin mi?</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Ürün satışa açık mı?</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Eş zamanlı (concurrent) stok güncellemelerinde iki kullanıcının aynı son ürünü almasını
    /// engellemek için kullanılan SQL Server rowversion / timestamp alanı.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Hesaplanan Özellikler
    [NotMapped]
    public bool IsInStock => StockQuantity > 0;

    [NotMapped]
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    // İlişkiler
    public Category? Category { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
