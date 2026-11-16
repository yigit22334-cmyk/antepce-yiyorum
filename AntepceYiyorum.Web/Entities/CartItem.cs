using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Sepetteki bir ürün satırını ve adedini temsil eder.
/// </summary>
public class CartItem
{
    public int Id { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, 999, ErrorMessage = "Adet en az 1 olmalıdır.")]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // İlişkiler
    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}
