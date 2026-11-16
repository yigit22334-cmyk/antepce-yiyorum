using System.Text;
using System.Text.RegularExpressions;
using AntepceYiyorum.Web.Data;
using AntepceYiyorum.Web.Entities;
using AntepceYiyorum.Web.Services.Interfaces;
using AntepceYiyorum.Web.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace AntepceYiyorum.Web.Services.Implementations;

/// <summary>
/// Admin panelindeki ürün, fotoğraf, fiyat, stok ve kategori yönetim işlemlerini gerçekleştiren servis sınıfı.
/// </summary>
public class AdminCatalogService : IAdminCatalogService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AdminCatalogService> _logger;

    public AdminCatalogService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<AdminCatalogService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var totalCategories = await _context.Categories.CountAsync(cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalOrders = await _context.Orders.CountAsync(cancellationToken);

        var alertProducts = products
            .Where(p => p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToList();

        return new AdminDashboardViewModel
        {
            TotalProducts = products.Count,
            ActiveProducts = products.Count(p => p.IsActive),
            TotalCategories = totalCategories,
            LowStockCount = products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold),
            OutOfStockCount = products.Count(p => p.StockQuantity == 0),
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            AlertProducts = alertProducts,
            AllProducts = products
        };
    }

    public async Task<AdminProductListViewModel> GetProductListAsync(
        string? searchTerm,
        int? categoryId,
        string? stockFilter,
        CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.ShortDescription.Contains(term));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var normalizedStockFilter = (stockFilter ?? "all").ToLowerInvariant();
        query = normalizedStockFilter switch
        {
            "low" => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold),
            "out" => query.Where(p => p.StockQuantity == 0),
            _ => query
        };

        var products = await query
            .OrderBy(p => p.Category!.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return new AdminProductListViewModel
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            StockFilter = normalizedStockFilter,
            Categories = categories,
            Products = products
        };
    }

    public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(bool Success, string Message)> CreateProductAsync(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        string imageUrl = "/images/products/default-product.svg";

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.ImageFile, "products", cancellationToken);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.ErrorMessage ?? "Görsel yüklenemedi.");
            }
            imageUrl = uploadResult.FileUrl!;
        }
        else if (!string.IsNullOrWhiteSpace(model.CurrentImageUrl))
        {
            imageUrl = model.CurrentImageUrl.Trim();
        }

        var slug = await GenerateUniqueProductSlugAsync(model.Name, null, cancellationToken);

        var product = new Product
        {
            CategoryId = model.CategoryId,
            Name = model.Name.Trim(),
            Slug = slug,
            ShortDescription = model.ShortDescription.Trim(),
            Description = model.Description?.Trim(),
            Price = model.Price,
            WeightGrams = model.WeightGrams,
            WeightLabel = model.WeightLabel.Trim(),
            StockQuantity = model.StockQuantity,
            LowStockThreshold = model.LowStockThreshold,
            ImageUrl = imageUrl,
            IsFeatured = model.IsFeatured,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin yeni ürün ekledi: {ProductName} (Fiyat: {Price} ₺, Stok: {Stock})",
            product.Name, product.Price, product.StockQuantity);

        return (true, $"'{product.Name}' ürünü başarıyla eklendi.");
    }

    public async Task<(bool Success, string Message)> UpdateProductAsync(
        AdminProductFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken);
        if (product == null)
        {
            return (false, "Güncellenecek ürün bulunamadı.");
        }

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.ImageFile, "products", cancellationToken);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.ErrorMessage ?? "Görsel yüklenemedi.");
            }

            _fileStorageService.DeleteImageIfUploaded(product.ImageUrl);
            product.ImageUrl = uploadResult.FileUrl!;
        }
        else if (!string.IsNullOrWhiteSpace(model.CurrentImageUrl))
        {
            product.ImageUrl = model.CurrentImageUrl.Trim();
        }

        if (!string.Equals(product.Name, model.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            product.Slug = await GenerateUniqueProductSlugAsync(model.Name, product.Id, cancellationToken);
        }

        product.CategoryId = model.CategoryId;
        product.Name = model.Name.Trim();
        product.ShortDescription = model.ShortDescription.Trim();
        product.Description = model.Description?.Trim();
        product.Price = model.Price;
        product.WeightGrams = model.WeightGrams;
        product.WeightLabel = model.WeightLabel.Trim();
        product.StockQuantity = model.StockQuantity;
        product.LowStockThreshold = model.LowStockThreshold;
        product.IsFeatured = model.IsFeatured;
        product.IsActive = model.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin ürünü güncelledi: #{Id} {Name} (Fiyat: {Price} ₺, Stok: {Stock})",
            product.Id, product.Name, product.Price, product.StockQuantity);

        return (true, $"'{product.Name}' ürününün fotoğraf, fiyat ve stok bilgileri güncellendi.");
    }

    public async Task<(bool Success, string Message)> QuickUpdateProductAsync(
        AdminQuickUpdateViewModel model,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken);
        if (product == null)
        {
            return (false, "Ürün bulunamadı.");
        }

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.ImageFile, "products", cancellationToken);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.ErrorMessage ?? "Fotoğraf yüklenirken hata oluştu.");
            }

            _fileStorageService.DeleteImageIfUploaded(product.ImageUrl);
            product.ImageUrl = uploadResult.FileUrl!;
        }

        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.IsFeatured = model.IsFeatured;
        product.IsActive = model.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin hızlı güncelleme yaptı: #{Id} {Name} -> Yeni Fiyat: {Price} ₺, Yeni Stok: {Stock}",
            product.Id, product.Name, product.Price, product.StockQuantity);

        return (true, $"'{product.Name}' güncellendi → Fiyat: {product.Price:N2} ₺ | Stok: {product.StockQuantity} adet.");
    }

    public async Task<(bool Success, string Message)> DeleteOrDeactivateProductAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.OrderItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
        {
            return (false, "Ürün bulunamadı.");
        }

        if (product.OrderItems.Any())
        {
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return (true, $"'{product.Name}' geçmiş siparişlerde yer aldığı için silinmedi, pasif (satış dışı) duruma alındı.");
        }

        _fileStorageService.DeleteImageIfUploaded(product.ImageUrl);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, $"'{product.Name}' ürünü kalıcı olarak silindi.");
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(bool Success, string Message)> CreateCategoryAsync(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var slug = await GenerateUniqueCategorySlugAsync(model.Name, null, cancellationToken);
        var category = new Category
        {
            Name = model.Name.Trim(),
            Slug = slug,
            Description = model.Description?.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, $"'{category.Name}' kategorisi başarıyla oluşturuldu.");
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(
        AdminCategoryFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id, cancellationToken);
        if (category == null)
        {
            return (false, "Kategori bulunamadı.");
        }

        if (!string.Equals(category.Name, model.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await GenerateUniqueCategorySlugAsync(model.Name, category.Id, cancellationToken);
        }

        category.Name = model.Name.Trim();
        category.Description = model.Description?.Trim();
        category.DisplayOrder = model.DisplayOrder;
        category.IsActive = model.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return (true, $"'{category.Name}' kategorisi güncellendi.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            return (false, "Kategori bulunamadı.");
        }

        if (category.Products.Any())
        {
            return (false, $"'{category.Name}' kategorisine bağlı {category.Products.Count} ürün bulunduğu için silinemez. Önce ürünleri başka kategoriye taşıyın.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, $"'{category.Name}' kategorisi silindi.");
    }

    private async Task<string> GenerateUniqueProductSlugAsync(
        string name,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugifyTurkish(name);
        var slug = baseSlug;
        var counter = 2;

        while (await _context.Products.AnyAsync(p => p.Slug == slug && p.Id != excludeId, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<string> GenerateUniqueCategorySlugAsync(
        string name,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugifyTurkish(name);
        var slug = baseSlug;
        var counter = 2;

        while (await _context.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeId, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string SlugifyTurkish(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "urun";
        }

        var sb = new StringBuilder(text.Trim().ToLowerInvariant());
        sb.Replace('ı', 'i')
          .Replace('ğ', 'g')
          .Replace('ü', 'u')
          .Replace('ş', 's')
          .Replace('ö', 'o')
          .Replace('ç', 'c');

        var normalized = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
        normalized = Regex.Replace(normalized, @"\s+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "urun" : normalized;
    }
}
