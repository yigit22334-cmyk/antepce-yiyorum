using AntepceYiyorum.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AntepceYiyorum.Web.Services.BackgroundServices;

/// <summary>
/// Süresi dolmuş (ExpiresAt &lt; UtcNow) giriş yapmamış (misafir) kullanıcı sepetlerini
/// veritabanından periyodik olarak temizleyen arka plan servisi (BackgroundService).
/// </summary>
public class ExpiredGuestCartCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredGuestCartCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public ExpiredGuestCartCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredGuestCartCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Eski misafir sepetlerini temizleme servisi başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredGuestCartsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Süresi dolmuş misafir sepetleri temizlenirken bir hata oluştu.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredGuestCartsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        var expiredCarts = await dbContext.Carts
            .Where(c => c.UserId == null && c.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expiredCarts.Count > 0)
        {
            dbContext.Carts.RemoveRange(expiredCarts);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "{Count} adet süresi dolmuş misafir sepeti veritabanından temizlendi.",
                expiredCarts.Count);
        }
    }
}
