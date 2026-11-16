using AntepceYiyorum.Web.Data.Seed;
using AntepceYiyorum.Web.Services.Interfaces;
using AntepceYiyorum.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntepceYiyorum.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin Paneli kategori ekleme, düzenleme ve silme işlemlerini yöneten denetleyici.
/// </summary>
[Area("Admin")]
[Authorize(Roles = DataSeeder.AdminRole)]
public class CategoriesController : Controller
{
    private readonly IAdminCatalogService _adminCatalogService;

    public CategoriesController(IAdminCatalogService adminCatalogService)
    {
        _adminCatalogService = adminCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Kategori adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _adminCatalogService.CreateCategoryAsync(model, cancellationToken);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Geçersiz kategori bilgisi.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _adminCatalogService.UpdateCategoryAsync(model, cancellationToken);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _adminCatalogService.DeleteCategoryAsync(id, cancellationToken);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
