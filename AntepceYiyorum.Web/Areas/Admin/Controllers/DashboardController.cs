using AntepceYiyorum.Web.Data.Seed;
using AntepceYiyorum.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntepceYiyorum.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin Paneli ana özet ekranını (Dashboard), stok uyarılarını ve hızlı yönetim kartlarını yöneten denetleyici.
/// </summary>
[Area("Admin")]
[Authorize(Roles = DataSeeder.AdminRole)]
public class DashboardController : Controller
{
    private readonly IAdminCatalogService _adminCatalogService;

    public DashboardController(IAdminCatalogService adminCatalogService)
    {
        _adminCatalogService = adminCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _adminCatalogService.GetDashboardAsync(cancellationToken);
        return View(viewModel);
    }
}
