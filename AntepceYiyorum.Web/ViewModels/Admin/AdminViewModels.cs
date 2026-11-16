using System.ComponentModel.DataAnnotations;
using AntepceYiyorum.Web.Entities;

namespace AntepceYiyorum.Web.ViewModels.Admin;

/// <summary>
/// Admin Paneli ana ekranı (Dashboard) özet istatistikleri ve kritik stok uyarıları modeli.
/// </summary>
public class AdminDashboardViewModel
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalCategories { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }

    public IReadOnlyList<Product> AlertProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Product> AllProducts { get; set; } = Array.Empty<Product>();
}

/// <summary>
/// Admin ürün yönetim tablosu ve filtreleme modeli.
/// </summary>
public class AdminProductListViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string StockFilter { get; set; } = "all"; // all, low, out

    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
}

/// <summary>
/// Admin ürün ekleme ve düzenleme formu modeli (Fotoğraf yükleme, fiyat ve stok dahil).
/// </summary>
public class AdminProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(160)]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kısa açıklama zorunludur.")]
    [MaxLength(300)]
    [Display(Name = "Kısa Açıklama (Kartlarda görünür)")]
    public string ShortDescription { get; set; } = string.Empty;

    [MaxLength(2500)]
    [Display(Name = "Detaylı Ürün Açıklaması")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Fiyat alanı zorunludur.")]
    [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    [Display(Name = "Satış Fiyatı (₺)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Gramaj sayısal değeri zorunludur.")]
    [Range(1, 100000, ErrorMessage = "Gramaj 1 ile 100000 arasında olmalıdır.")]
    [Display(Name = "Gramaj (Gram / ml)")]
    public int WeightGrams { get; set; } = 500;

    [Required(ErrorMessage = "Gramaj etiketi zorunludur.")]
    [MaxLength(50)]
    [Display(Name = "Gramaj Etiketi (Örn: 500 g, 1 kg, 500 ml)")]
    public string WeightLabel { get; set; } = "500 g";

    [Required(ErrorMessage = "Stok adedi zorunludur.")]
    [Range(0, 1000000, ErrorMessage = "Stok negatif olamaz.")]
    [Display(Name = "Mevcut Stok Adedi")]
    public int StockQuantity { get; set; } = 25;

    [Required(ErrorMessage = "Kritik stok uyarı sınırı zorunludur.")]
    [Range(1, 10000, ErrorMessage = "Kritik stok sınırı en az 1 olmalıdır.")]
    [Display(Name = "Kritik Stok Uyarı Eşiği")]
    public int LowStockThreshold { get; set; } = 10;

    [Display(Name = "Mevcut Görsel URL")]
    public string? CurrentImageUrl { get; set; }

    [Display(Name = "Yeni Ürün Fotoğrafı Yükle")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Ana Sayfada Öne Çıkan Ürün Olarak Göster")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Ürün Satışa Açık (Aktif)")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Admin tablosundan tek tıkla Fiyat, Stok ve Fotoğraf güncellemesi yapmak için kullanılan model.
/// </summary>
public class AdminQuickUpdateViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Geçerli bir fiyat giriniz.")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "Stok negatif olamaz.")]
    public int StockQuantity { get; set; }

    public IFormFile? ImageFile { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Admin kategori ekleme ve düzenleme formu modeli.
/// </summary>
public class AdminCategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MaxLength(120)]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Kategori Açıklaması")]
    public string? Description { get; set; }

    [Display(Name = "Sıralama Numarası")]
    public int DisplayOrder { get; set; } = 1;

    [Display(Name = "Kategori Aktif")]
    public bool IsActive { get; set; } = true;
}
