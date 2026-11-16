using AntepceYiyorum.Web.Entities;

namespace AntepceYiyorum.Web.ViewModels;

/// <summary>
/// Ana sayfa için kategori ve öne çıkan ürün özetlerini taşıyan ViewModel.
/// </summary>
public class HomeIndexViewModel
{
    public int TotalCategories { get; set; }
    public int TotalActiveProducts { get; set; }
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Product> FeaturedProducts { get; set; } = Array.Empty<Product>();
}
