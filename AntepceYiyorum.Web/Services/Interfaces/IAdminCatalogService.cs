using AntepceYiyorum.Web.Entities;
using AntepceYiyorum.Web.ViewModels.Admin;

namespace AntepceYiyorum.Web.Services.Interfaces;

/// <summary>
/// Admin panelindeki ürün, stok, fiyat, fotoğraf ve kategori yönetim işlemlerini yürüten servis arayüzü.
/// </summary>
public interface IAdminCatalogService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminProductListViewModel> GetProductListAsync(
        string? searchTerm,
        int? categoryId,
        string? stockFilter,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateProductAsync(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateProductAsync(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> QuickUpdateProductAsync(
        AdminQuickUpdateViewModel model,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteOrDeactivateProductAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateCategoryAsync(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateCategoryAsync(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteCategoryAsync(
        int id,
        CancellationToken cancellationToken = default);
}
