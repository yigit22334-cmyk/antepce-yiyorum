using AntepceYiyorum.Web.DTOs;
using AntepceYiyorum.Web.ViewModels;

namespace AntepceYiyorum.Web.Services.Interfaces;

/// <summary>
/// Ana sayfa ve sistem durumu özet bilgilerini sağlayan servis arayüzü.
/// </summary>
public interface ICatalogSummaryService
{
    Task<HomeIndexViewModel> GetHomeSummaryAsync(CancellationToken cancellationToken = default);
    Task<SystemStatusDto> GetSystemStatusAsync(CancellationToken cancellationToken = default);
}
