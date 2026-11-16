using AntepceYiyorum.Web.DTOs;
using AntepceYiyorum.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AntepceYiyorum.Web.Controllers.Api;

/// <summary>
/// Web ön yüzü ve ileride geliştirilecek Java Android mobil uygulaması için
/// API sağlık ve veritabanı durumunu JSON olarak dönen REST API denetleyicisi.
/// </summary>
[ApiController]
[Route("api/v1/health")]
public class HealthApiController : ControllerBase
{
    private readonly ICatalogSummaryService _catalogSummaryService;

    public HealthApiController(ICatalogSummaryService catalogSummaryService)
    {
        _catalogSummaryService = catalogSummaryService;
    }

    [HttpGet]
    public async Task<ActionResult<SystemStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _catalogSummaryService.GetSystemStatusAsync(cancellationToken);
        return Ok(status);
    }
}
