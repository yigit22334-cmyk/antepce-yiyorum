using AntepceYiyorum.Web.Data.Seed;
using AntepceYiyorum.Web.Services.Interfaces;
using AntepceYiyorum.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AntepceYiyorum.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin Paneli üzerinden ürün ekleme, düzenleme, fotoğraf yükleme ve hızlı fiyat/stok güncelleme denetleyicisi.
/// </summary>
[Area("Admin")]
[Authorize(Roles = DataSeeder.AdminRole)]
public class ProductsController : Controller
{
    private readonly IAdminCatalogService _adminCatalogService;

    public ProductsController(IAdminCatalogService adminCatalogService)
    {
        _adminCatalogService = adminCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        int? categoryId,
        string? stockFilter,
        CancellationToken cancellationToken)
    {
        var viewModel = await _adminCatalogService.GetProductListAsync(
            searchTerm,
            categoryId,
            stockFilter,
            cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
        return View(new AdminProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
            return View(model);
        }

        var result = await _adminCatalogService.CreateProductAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await _adminCatalogService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            TempData["ErrorMessage"] = "Düzenlenecek ürün bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);

        var model = new AdminProductFormViewModel
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            WeightGrams = product.WeightGrams,
            WeightLabel = product.WeightLabel,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            CurrentImageUrl = product.ImageUrl,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
            return View(model);
        }

        var result = await _adminCatalogService.UpdateProductAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Categories = await _adminCatalogService.GetAllCategoriesAsync(cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Tablo veya Dashboard üzerinden tek tıkla Fiyat, Stok, Fotoğraf ve Öne Çıkan durumunu günceller.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate(
        AdminQuickUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Lütfen geçerli bir fiyat ve stok adedi giriniz.";
            return RedirectToLocal(model.ReturnUrl);
        }

        var result = await _adminCatalogService.QuickUpdateProductAsync(model, cancellationToken);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _adminCatalogService.DeleteOrDeactivateProductAsync(id, cancellationToken);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
