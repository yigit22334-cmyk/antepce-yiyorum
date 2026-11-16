using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Sipariş içerisindeki her bir ürün kalemini temsil eder.
/// Ürün adı, gramajı ve birim fiyatı sipariş anındaki değerleriyle kaydedilir.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    /// <summary>Sipariş anındaki ürün adı.</summary>
    [Required]
    [MaxLength(160)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Sipariş anındaki gramaj etiketi.</summary>
    [MaxLength(50)]
    public string WeightLabel { get; set; } = string.Empty;

    /// <summary>Sipariş anındaki birim fiyat (TL).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

    /// <summary>Satır toplam tutarı (UnitPrice * Quantity).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    // İlişkiler
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
