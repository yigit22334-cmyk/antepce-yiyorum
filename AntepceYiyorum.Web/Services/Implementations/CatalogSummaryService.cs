using AntepceYiyorum.Web.Data.Repositories;
using AntepceYiyorum.Web.DTOs;
using AntepceYiyorum.Web.Entities;
using AntepceYiyorum.Web.Services.Interfaces;
using AntepceYiyorum.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AntepceYiyorum.Web.Services.Implementations;

/// <summary>
/// Repository katmanını kullanarak ana sayfa ve API özet verilerini üreten servis sınıfı.
/// </summary>
public class CatalogSummaryService : ICatalogSummaryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Product> _productRepository;

    public CatalogSummaryService(
        IRepository<Category> categoryRepository,
        IRepository<Product> productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<HomeIndexViewModel> GetHomeSummaryAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.Query()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Products.Where(p => p.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var featuredProducts = await _productRepository.Query()
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsFeatured)
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .Take(6)
            .ToListAsync(cancellationToken);

        var totalActiveProducts = await _productRepository.Query()
            .CountAsync(p => p.IsActive, cancellationToken);

        return new HomeIndexViewModel
        {
            TotalCategories = categories.Count,
            TotalActiveProducts = totalActiveProducts,
            Categories = categories,
            FeaturedProducts = featuredProducts
        };
    }

    public async Task<SystemStatusDto> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        var categoryCount = await _categoryRepository.Query().CountAsync(c => c.IsActive, cancellationToken);
        var productCount = await _productRepository.Query().CountAsync(p => p.IsActive, cancellationToken);

        return new SystemStatusDto
        {
            CategoryCount = categoryCount,
            ActiveProductCount = productCount,
            ServerTimeUtc = DateTime.UtcNow
        };
    }
}
