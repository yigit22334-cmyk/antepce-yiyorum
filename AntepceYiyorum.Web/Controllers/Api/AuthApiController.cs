using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AntepceYiyorum.Web.DTOs.Auth;
using AntepceYiyorum.Web.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AntepceYiyorum.Web.Controllers.Api;

/// <summary>
/// İleride geliştirilecek Java Android mobil uygulaması ve dış API istemcileri için
/// JWT (JSON Web Token) üreten kimlik doğrulama REST API denetleyicisi.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthApiController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Geçersiz giriş bilgileri."
            });
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "E-posta veya şifre hatalı."
            });
        }

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Sunucu JWT anahtarı yapılandırılmamış."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "AntepceYiyorum.Api",
            audience: _configuration["Jwt:Audience"] ?? "AntepceYiyorum.MobileClient",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "Giriş başarılı.",
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles.ToList()
        });
    }
}
