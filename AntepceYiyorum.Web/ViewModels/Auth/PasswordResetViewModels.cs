using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.ViewModels.Auth;

/// <summary>
/// Şifre sıfırlama bağlantısı talep formu modeli.
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "Kayıtlı E-Posta Adresiniz")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Token ile yeni şifre belirleme formu modeli.
/// </summary>
public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre Tekrarı")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
