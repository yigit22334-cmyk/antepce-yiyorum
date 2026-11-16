using System.Diagnostics;
using AntepceYiyorum.Web.Models;
using AntepceYiyorum.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AntepceYiyorum.Web.Controllers;

/// <summary>
/// Ana sayfa ve genel bilgilendirme sayfalarını yöneten MVC denetleyicisi.
/// </summary>
public class HomeController : Controller
{
    private readonly ICatalogSummaryService _catalogSummaryService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ICatalogSummaryService catalogSummaryService,
        ILogger<HomeController> logger)
    {
        _catalogSummaryService = catalogSummaryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _catalogSummaryService.GetHomeSummaryAsync(cancellationToken);
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
