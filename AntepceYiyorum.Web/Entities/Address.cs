using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Kullanıcının kayıtlı teslimat adreslerini temsil eder.
/// </summary>
public class Address
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres başlığı zorunludur (Örn: Ev, İş).")]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alıcı adı soyadı zorunludur.")]
    [MaxLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [MaxLength(25)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    [MaxLength(60)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlçe zorunludur.")]
    [MaxLength(60)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açık adres zorunludur.")]
    [MaxLength(500)]
    public string FullAddress { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? ZipCode { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // İlişkiler
    public ApplicationUser? User { get; set; }
}
