namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Siparişin ödeme durumunu belirten numaralandırma.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Ödeme henüz tamamlanmadı.</summary>
    Pending = 0,

    /// <summary>Ödeme başarıyla alındı.</summary>
    Paid = 1,

    /// <summary>Ödeme başarısız oldu.</summary>
    Failed = 2,

    /// <summary>Ödeme iade edildi.</summary>
    Refunded = 3
}
