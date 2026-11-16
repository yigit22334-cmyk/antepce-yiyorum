using System.Text;
using AntepceYiyorum.Web.Data;
using AntepceYiyorum.Web.Data.Repositories;
using AntepceYiyorum.Web.Data.Seed;
using AntepceYiyorum.Web.Entities;
using AntepceYiyorum.Web.Services.BackgroundServices;
using AntepceYiyorum.Web.Services.Implementations;
using AntepceYiyorum.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (MS SQL Server - Code First)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. ASP.NET Core Identity Yapılandırması (Müşteri ve Admin Rolleri)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Web Arayüzü Çerez (Cookie) Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AntepceYiyorum.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// 3. Mobil REST API için JWT Bearer Yapılandırması (Anahtar User Secrets'tan okunur)
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AntepceYiyorum.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AntepceYiyorum.MobileClient";

if (!string.IsNullOrWhiteSpace(jwtKey))
{
    builder.Services.AddAuthentication()
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
}

// 4. Katmanlı Mimari Servis ve Repository Kayıtları (DI)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICatalogSummaryService, CatalogSummaryService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IAdminCatalogService, AdminCatalogService>();

// 5. Süresi Dolmuş Misafir Sepetlerini Temizleyen Arka Plan Servisi
builder.Services.AddHostedService<ExpiredGuestCartCleanupService>();

// 6. MVC ve REST API Denetleyicileri
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 7. İlk Çalıştırmada Migration ve Seed Data Uygulaması
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await DataSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı migration veya seed işlemi sırasında bir hata oluştu.");
        throw;
    }
}

// 8. HTTP İstek Hattı (Middleware Pipeline)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Admin Paneli Area Rotası
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Varsayılan Web MVC Rotası
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
