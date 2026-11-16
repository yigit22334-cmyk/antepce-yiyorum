using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Müşteri siparişini temsil eder.
/// Stok, mock ödeme başarılı olup sipariş oluşturulduğunda düşülür (IsStockDeducted = true);
/// sipariş iptal edildiğinde (OrderStatus.Cancelled) stok geri eklenir (IsStockDeducted = false).
/// </summary>
public class Order
{
    public int Id { get; set; }

    /// <summary>Müşteriye gösterilen benzersiz sipariş takip numarası (Örn: ANT-20261116-8492).</summary>
    [Required]
    [MaxLength(40)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Ödeme servisinden (Mock / iyzico / PayTR) dönen işlem referans numarası.</summary>
    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>Ürünlerin toplam tutarı + kargo ücreti (TL).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingFee { get; set; }

    // Teslimat Bilgileri (Sipariş anındaki anlık kopya - adres sonradan değişse bile sipariş kaydı bozulmaz)
    [Required(ErrorMessage = "Alıcı adı soyadı zorunludur.")]
    [MaxLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [MaxLength(25)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    [MaxLength(60)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlçe zorunludur.")]
    [MaxLength(60)]
    public string ShippingDistrict { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? ShippingZipCode { get; set; }

    [MaxLength(500)]
    public string? OrderNote { get; set; }

    /// <summary>
    /// Bu sipariş için ürün stoklarının düşülüp düşülmediğini belirtir.
    /// Mock ödeme başarılı olduğunda true yapılır; sipariş iptal edildiğinde stok iade edilip false yapılır.
    /// </summary>
    public bool IsStockDeducted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // İlişkiler
    public ApplicationUser? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
