using System.ComponentModel.DataAnnotations;

namespace AntepceYiyorum.Web.DTOs.Auth;

/// <summary>
/// Mobil (Android) uygulama ve API istemcileri için JWT giriş isteği modeli.
/// </summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Başarılı API girişinde dönen JWT token ve kullanıcı bilgileri modeli.
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
