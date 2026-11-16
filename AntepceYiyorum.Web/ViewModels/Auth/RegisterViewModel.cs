using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.ViewModels.Auth;

/// <summary>
/// Yeni müşteri kayıt formu modeli.
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MaxLength(100)]
    [Display(Name = "Adınız")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MaxLength(100)]
    [Display(Name = "Soyadınız")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-Posta Adresi")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [MaxLength(25)]
    [Display(Name = "Telefon Numarası")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
    [Display(Name = "Şifre Tekrarı")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
