namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Siparişin operasyonel durumunu belirten numaralandırma.
/// </summary>
public enum OrderStatus
{
    /// <summary>Ödeme / onay bekleniyor.</summary>
    Pending = 0,

    /// <summary>Ödeme alındı ve sipariş onaylandı (Stok düşüldü).</summary>
    Confirmed = 1,

    /// <summary>Sipariş hazırlanıyor / paketleniyor.</summary>
    Preparing = 2,

    /// <summary>Kargoya verildi.</summary>
    Shipped = 3,

    /// <summary>Müşteriye teslim edildi.</summary>
    Delivered = 4,

    /// <summary>İptal edildi (Düşülen stok geri eklendi).</summary>
    Cancelled = 5
}
