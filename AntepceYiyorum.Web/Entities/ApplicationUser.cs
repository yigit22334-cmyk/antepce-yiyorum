using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AntepceYiyorum.Web.Entities;

/// <summary>
/// Uygulama kullanıcısı (Müşteri ve Admin rolleriyle ASP.NET Core Identity üzerinden yönetilir).
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Kullanıcının tam adı.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Hesap oluşturulma tarihi (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Kullanıcı hesabının aktiflik durumu.</summary>
    public bool IsActive { get; set; } = true;

    // İlişkiler (Navigation Properties)
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
