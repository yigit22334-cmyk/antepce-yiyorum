using AntepceYiyorum.Web.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AntepceYiyorum.Web.Data.Seed;

/// <summary>
/// Uygulama ilk kez çalıştığında veritabanı migration'larını uygular,
/// Admin/Customer rollerini, User Secrets'tan okunan Admin kullanıcısını
/// ve Gaziantep yöresel kategori/ürün başlangıç verilerini (Seed Data) oluşturur.
/// </summary>
public static class DataSeeder
{
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // 1. Bekleyen Migration'ları Uygula
        await context.Database.MigrateAsync();

        // 2. Rolleri Oluştur (Admin ve Customer)
        string[] roles = { AdminRole, CustomerRole };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("'{Role}' rolü oluşturuldu.", roleName);
            }
        }

        // 3. Admin Kullanıcısını User Secrets'tan Okuyarak Oluştur (Şifre koda gömülmez!)
        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@antepceyiyorum.com";
        var adminPassword = configuration["SeedAdmin:Password"];
        var adminFirstName = configuration["SeedAdmin:FirstName"] ?? "Antepçe";
        var adminLastName = configuration["SeedAdmin:LastName"] ?? "Yönetici";

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "SeedAdmin:Password User Secrets içinde tanımlanmamış! " +
                "Admin kullanıcısının oluşturulması için 'dotnet user-secrets set \"SeedAdmin:Password\" \"GucluSifre123!\"' komutunu çalıştırın.");
        }
        else
        {
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                    logger.LogInformation("Admin kullanıcısı ({Email}) başarıyla oluşturuldu.", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Admin kullanıcısı oluşturulamadı: {Errors}", errors);
                }
            }
        }

        // 4. Kategoriler ve Yöresel Ürünler Seed Verisi
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new()
                {
                    Name = "Antep Fıstığı ve Kuruyemiş",
                    Slug = "antep-fistigi-ve-kuruyemis",
                    Description = "Boz fıstık, kavrulmuş tuzlu Antep fıstığı, kelebek ceviz içi ve doğal çam fıstığı çeşitleri.",
                    ImageUrl = "/images/categories/fistik-kuruyemis.svg",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new()
                {
                    Name = "Yöresel Tatlılar ve Ezme",
                    Slug = "yoresel-tatlilar-ve-ezme",
                    Description = "Taş değirmende çekilmiş fıstık ezmesi, fıstıklı muska, pekmezli sucuk ve çifte kavrulmuş lokumlar.",
                    ImageUrl = "/images/categories/tatlilar-ezme.svg",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new()
                {
                    Name = "Salça, Kurutmalık ve Turşu",
                    Slug = "salca-kurutmalik-ve-tursu",
                    Description = "Antep güneşi altında kurutulmuş doğal salçalar, dolmalık kurutmalıklar ve ev yapımı turşular.",
                    ImageUrl = "/images/categories/salca-kurutmalik.svg",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new()
                {
                    Name = "Baharat ve Ekşiler",
                    Slug = "baharat-ve-eksiler",
                    Description = "Zeytinyağlı ipek pul biber, zahter, sumak, odun ateşinde kaynatılmış hakiki nar ve vişne ekşisi.",
                    ImageUrl = "/images/categories/baharat-eksiler.svg",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new()
                {
                    Name = "Geleneksel Antep Yemeklikleri",
                    Slug = "geleneksel-antep-yemeklikleri",
                    Description = "Bayram sofralarının baş tacı el yapımı Antep yuvalaması ve geleneksel yaprak köfte.",
                    ImageUrl = "/images/categories/antep-yemeklik.svg",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new()
                {
                    Name = "Kahvaltılık, Çay ve Kahve",
                    Slug = "kahvaltilik-cay-ve-kahve",
                    Description = "Yabani fıstıktan menengiç kahvesi, zahter çayı ve geleneksel Antep reçel çeşitleri.",
                    ImageUrl = "/images/categories/kahvaltilik-kahve.svg",
                    DisplayOrder = 6,
                    IsActive = true
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var catFistik = categories.First(c => c.Slug == "antep-fistigi-ve-kuruyemis").Id;
            var catTatli = categories.First(c => c.Slug == "yoresel-tatlilar-ve-ezme").Id;
            var catSalca = categories.First(c => c.Slug == "salca-kurutmalik-ve-tursu").Id;
            var catBaharat = categories.First(c => c.Slug == "baharat-ve-eksiler").Id;
            var catYemek = categories.First(c => c.Slug == "geleneksel-antep-yemeklikleri").Id;
            var catKahvalti = categories.First(c => c.Slug == "kahvaltilik-cay-ve-kahve").Id;

            var products = new List<Product>
            {
                // 1. Kategori: Antep Fıstığı ve Kuruyemiş
                new()
                {
                    CategoryId = catFistik,
                    Name = "Birinci Sınıf İç Boz Antep Fıstığı",
                    Slug = "birinci-sinif-ic-boz-antep-fistigi-500g",
                    ShortDescription = "Erken hasat zümrüt yeşili, baklavalık ve pastalık birinci kalite iç boz fıstık.",
                    Description = "Gaziantep'in Barak ovası ve Nizip bahçelerinden Ağustos ayında erken hasat edilen gerçek boz iç fıstık. Yoğun aroması ve zümrüt yeşili rengiyle tatlılarınızda ve sofralarınızda eşsiz lezzet sunar.",
                    Price = 690.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 35,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/boz-fistik.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catFistik,
                    Name = "Kavrulmuş Tuzlu Antep Fıstığı (Ana Çıtlak)",
                    Slug = "kavrulmus-tuzlu-antep-fistigi-ana-citlak-500g",
                    ShortDescription = "İri taneli, doğal ağaçta çıtlamış ve taş fırında özenle kavrulmuş tuzlu Antep fıstığı.",
                    Description = "Hiçbir yapay işlem görmeden dalında doğal olarak çıtlayan iri taneli Antep fıstıklarının geleneksel usulde kavrulmasıyla hazırlanmıştır.",
                    Price = 420.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 60,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/tuzlu-fistik.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catFistik,
                    Name = "Yerli Kelebek Ceviz İçi",
                    Slug = "yerli-kelebek-ceviz-ici-500g",
                    ShortDescription = "Yeni mahsul, ince kabuklu cevizlerden ayıklanmış açık renkli kelebek ceviz içi.",
                    Description = "Güneydoğu Anadolu yaylalarından toplanan yeni mahsul doğal ceviz içi. Kahvaltılarda, tatlılarda ve ara öğünlerde taze tüketim için idealdir.",
                    Price = 285.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 45,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/ceviz-ici.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catFistik,
                    Name = "Doğal Çam Fıstığı (Dolmalık & Helvalık)",
                    Slug = "dogal-cam-fistigi-250g",
                    ShortDescription = "İç pilav, zeytinyağlı dolma, yuvalama ve irmik helvası için birinci kalite çam fıstığı.",
                    Description = "Özenle seçilmiş dolgun taneli doğal çam fıstığı. Gaziantep mutfağının etli ve zeytinyağlı yemeklerine karakteristik lezzetini katar.",
                    Price = 640.00m,
                    WeightGrams = 250,
                    WeightLabel = "250 g",
                    StockQuantity = 22,
                    LowStockThreshold = 8,
                    ImageUrl = "/images/products/cam-fistigi.jpg",
                    IsFeatured = false,
                    IsActive = true
                },

                // 2. Kategori: Yöresel Tatlılar ve Ezme
                new()
                {
                    CategoryId = catTatli,
                    Name = "Taş Değirmen Antep Fıstık Ezmesi (%80 Fıstık)",
                    Slug = "tas-degirmen-antep-fistik-ezmesi-350g",
                    ShortDescription = "Boz Antep fıstığından taş değirmende çekilmiş, koruyucu ve glikoz içermeyen yoğun fıstık ezmesi.",
                    Description = "%80 oranında birinci sınıf boz Antep fıstığı ve pancar şekeri ile geleneksel taş değirmenlerde ezilerek üretilmiştir. Palm yağı, koruyucu ve renklendirici içermez.",
                    Price = 365.00m,
                    WeightGrams = 350,
                    WeightLabel = "350 g",
                    StockQuantity = 50,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/fistik-ezmesi.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catTatli,
                    Name = "Antep Fıstıklı Muska Pestil",
                    Slug = "antep-fistikli-muska-pestil-500g",
                    ShortDescription = "İnce üzüm pestili içerisine bol Antep fıstığı ezmesi sarılarak hazırlanan geleneksel muska.",
                    Description = "Güneşte kurutulmuş doğal üzüm pestilinin içerisine fıstıklı özel harç doldurularak üçgen muska formunda sarılmasıyla üretilen enerji deposu yöresel tatlı.",
                    Price = 340.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 40,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/fistikli-muska.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catTatli,
                    Name = "Fıstıklı Pekmezli Sucuk (Şire Sucuğu)",
                    Slug = "fistikli-pekmezli-sucuk-500g",
                    ShortDescription = "İpe dizilmiş taze Antep fıstıklarının doğal üzüm pekmezi peltesine batırılmasıyla yapılan şire sucuğu.",
                    Description = "Gaziantep bağlarının kehribar üzümlerinden elde edilen şire (pekmez) peltesine batırılarak güneşte kurutulan bol Antep fıstıklı geleneksel sucuk.",
                    Price = 380.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 38,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/fistikli-sucuk.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catTatli,
                    Name = "Cevizli Pekmezli Sucuk (Köme)",
                    Slug = "cevizli-pekmezli-sucuk-500g",
                    ShortDescription = "Kelebek ceviz içlerinin üzüm pekmezi şiresiyle kaplanıp kurutulduğu doğal pekmezli sucuk.",
                    Description = "Yerli keviz içlerinin ipe dizilip odun ateşinde kaynatılan doğal üzüm pekmezi bulamacına batırılmasıyla hazırlanan katkısız kış tatlısı.",
                    Price = 275.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 45,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/pekmezli-sucuk.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catTatli,
                    Name = "Çifte Kavrulmuş Bol Antep Fıstıklı Lokum",
                    Slug = "cifte-kavrulmus-bol-antep-fistikli-lokum-500g",
                    ShortDescription = "İçi ve dışı tüm boz Antep fıstıklarıyla bezeli, pancar şekerli geleneksel lokum.",
                    Description = "Glikoz şurubu kullanılmadan %100 pancar şekeri ve bol Antep fıstığı ile bakır kazanlarda ağır ağır pişirilmiş çifte kavrulmuş lokum.",
                    Price = 320.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 55,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/fistikli-lokum.jpg",
                    IsFeatured = false,
                    IsActive = true
                },

                // 3. Kategori: Salça, Kurutmalık ve Turşu
                new()
                {
                    CategoryId = catSalca,
                    Name = "Antep Güneş Kurusu Karışık Biber & Domates Salçası",
                    Slug = "antep-gunes-kurusu-salca-1000g",
                    ShortDescription = "Gaziantep güneşi altında tepsilerde kıvam almış, katkısız ev yapımı biber ve domates salçası.",
                    Description = "Etli kapya biberlerin ve tarla domateslerinin çekilip Antep güneşi altında günlerce karıştırılarak suyunu çekmesiyle elde edilen yoğun kıvamlı doğal salça.",
                    Price = 245.00m,
                    WeightGrams = 1000,
                    WeightLabel = "1 kg",
                    StockQuantity = 70,
                    LowStockThreshold = 12,
                    ImageUrl = "/images/products/antep-salca.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catSalca,
                    Name = "Geleneksel Antep Kurutmalık Seti (Patlıcan & Biber)",
                    Slug = "geleneksel-antep-kurutmalik-seti-50-adet",
                    ShortDescription = "Oğuzeli'nin ince kabuklu patlıcan ve tatlı biberlerinden ipe dizilerek güneşte kurutulmuş dolmalık set.",
                    Description = "25 adet kurutulmuş dolmalık patlıcan ve 25 adet kurutulmuş dolmalık kırmızı biberden oluşan (toplam 50 adet / 1 bağ) hakiki Gaziantep kurutmalık seti.",
                    Price = 210.00m,
                    WeightGrams = 300,
                    WeightLabel = "50 Adet (1 Bağ)",
                    StockQuantity = 48,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/kurutmalik.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catSalca,
                    Name = "Ev Yapımı Antep Karışık Zahterli Turşu",
                    Slug = "ev-yapimi-antep-karisik-tursu-1000g",
                    ShortDescription = "Kaya tuzu, sarımsak ve taze dağ kekiği (zahter) ile kurulmuş kütür kütür ev turşusu.",
                    Description = "Acur, badem salatalık, kelek, yeşil domates ve Antep biberinin doğal üzüm sirkesi ve kaya tuzu ile fermente edilmesiyle hazırlanan probiyotik yöresel turşu.",
                    Price = 175.00m,
                    WeightGrams = 1000,
                    WeightLabel = "1 kg",
                    StockQuantity = 32,
                    LowStockThreshold = 8,
                    ImageUrl = "/images/products/antep-tursu.jpg",
                    IsFeatured = false,
                    IsActive = true
                },

                // 4. Kategori: Baharat ve Ekşiler
                new()
                {
                    CategoryId = catBaharat,
                    Name = "Zeytinyağlı Hakiki Antep İpek Pul Biber",
                    Slug = "zeytinyagli-antep-ipek-pul-biber-500g",
                    ShortDescription = "İslahiye ve Nurdağı biberlerinden taş dibekte dövülmüş, sızma zeytinyağı ile harmanlanmış ipek pul biber.",
                    Description = "Güneşte kurutulan tatlı-acı dengeli kırmızı biberlerin tohumlarından ayrılıp taş dibeklerde ipek dokusunda dövülmesi ve saf zeytinyağı ile nemlendirilmesiyle üretilmiştir.",
                    Price = 230.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 65,
                    LowStockThreshold = 15,
                    ImageUrl = "/images/products/ipek-pul-biber.jpg",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catBaharat,
                    Name = "Odun Ateşinde Kaynatılmış %100 Hakiki Nar Ekşisi",
                    Slug = "odun-atesinde-hakiki-nar-eksisi-500ml",
                    ShortDescription = "Oğuzeli hicaz narlarından glikoz ve sitrik asit eklenmeden bakır kazanlarda kaynatılmış saf nar ekşisi.",
                    Description = "Sadece sıkılmış nar suyunun odun ateşinde saatlerce kaynatılarak pekmez kıvamına getirilmesiyle üretilmiştir. Nar ekşili sos değil, %100 saf nar ekşisidir.",
                    Price = 345.00m,
                    WeightGrams = 650,
                    WeightLabel = "500 ml",
                    StockQuantity = 30,
                    LowStockThreshold = 8,
                    ImageUrl = "/images/products/nar-eksisi.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catBaharat,
                    Name = "Geleneksel Doğal Vişne Ekşisi",
                    Slug = "geleneksel-dogal-visne-eksisi-500ml",
                    ShortDescription = "Mayhoş Antep yemekleri (vişne kebabı, sarma) ve salatalar için katkısız yoğun vişne ekşisi.",
                    Description = "Yayla vişnelerinin çekirdeklerinden ayrılıp geleneksel usulde koyulaştırılmasıyla elde edilen, salatalara ve et yemeklerine benzersiz aroma veren doğal vişne ekşisi.",
                    Price = 310.00m,
                    WeightGrams = 650,
                    WeightLabel = "500 ml",
                    StockQuantity = 24,
                    LowStockThreshold = 8,
                    ImageUrl = "/images/products/visne-eksisi.jpg",
                    IsFeatured = false,
                    IsActive = true
                },

                // 5. Kategori: Geleneksel Antep Yemeklikleri
                new()
                {
                    CategoryId = catYemek,
                    Name = "El Yapımı Antep Yuvalama (Dondurulmuş / Vakumlu)",
                    Slug = "el-yapimi-antep-yuvalama-500g",
                    ShortDescription = "Pirinç unu, yağsız dana kıyması ve irmikle tek tek elde yuvarlanmış bayramlık Antep yuvalaması.",
                    Description = "Gaziantep kadın kooperatifleri tarafından geleneksel tarifle tek tek elde yuvarlanıp buharda mühürlenmiş, nohut ve süzme yoğurtla pişirmeye hazır vakumlu Antep yuvalaması.",
                    Price = 350.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 20,
                    LowStockThreshold = 5,
                    ImageUrl = "/images/products/antep-yuvalama.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catYemek,
                    Name = "Geleneksel Antep Yaprak Köfte (Simit Köftesi Harcı)",
                    Slug = "geleneksel-antep-yaprak-kofte-500g",
                    ShortDescription = "İnce esmer köftelik bulgur (simit), reyhan, kimyon ve ipek biberle yoğrulup şekillendirilmiş yaprak köfte.",
                    Description = "Gaziantep'in ünlü ince simit bulguru ve yöresel baharatlarıyla taş havanda kıvam alıp yaprak formunda incelikle şekillendirilmiş, haşlama veya sotelemeye hazır yöresel köfte.",
                    Price = 240.00m,
                    WeightGrams = 500,
                    WeightLabel = "500 g",
                    StockQuantity = 25,
                    LowStockThreshold = 6,
                    ImageUrl = "/images/products/yaprak-kofte.jpg",
                    IsFeatured = false,
                    IsActive = true
                },

                // 6. Kategori: Kahvaltılık, Çay ve Kahve
                new()
                {
                    CategoryId = catKahvalti,
                    Name = "Tahmis Usulü Menengiç Kahvesi & Zahter Çayı Seti",
                    Slug = "menengic-kahvesi-ve-zahter-cayi-seti-400g",
                    ShortDescription = "Yabani fıstık (çitlembik) özlü kavrulmuş menengiç kahvesi ve doğal dağ zahteri çayı ikili seti.",
                    Description = "Sütle veya suyla pişirilen kafeinsiz doğal menengiç kahvesi (250 g) ve Gaziantep'in tarihi kahvehanelerinin vazgeçilmezi sindirim dostu zahter çayı (150 g) bir arada.",
                    Price = 225.00m,
                    WeightGrams = 400,
                    WeightLabel = "400 g (İkili Set)",
                    StockQuantity = 42,
                    LowStockThreshold = 10,
                    ImageUrl = "/images/products/menengic-kahve.jpg",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catKahvalti,
                    Name = "Ev Yapımı Antep Ceviz & Patlıcan Reçeli",
                    Slug = "ev-yapimi-antep-recel-cesitleri-680g",
                    ShortDescription = "Kireç kaymağında kıtırlaştırılmış taze yeşil ceviz ve bebek patlıcan reçeli.",
                    Description = "Gaziantep kahvaltı sofralarının asırlık geleneği; karanfil ve tarçın aromalı şerbet içinde pişirilmiş kıtır bütün ceviz ve bebek patlıcan reçeli.",
                    Price = 195.00m,
                    WeightGrams = 680,
                    WeightLabel = "680 g",
                    StockQuantity = 36,
                    LowStockThreshold = 8,
                    ImageUrl = "/images/products/antep-recel.jpg",
                    IsFeatured = false,
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Seed Data tamamlandı: {CatCount} kategori ve {ProdCount} yöresel ürün eklendi.",
                categories.Count,
                products.Count);
        }
        else
        {
            // Mevcut veritabanında eski .svg uzantılı varsayılan ürün görselleri varsa .jpg fotoğraflarına güncelle
            var svgProducts = await context.Products
                .Where(p => p.ImageUrl != null && p.ImageUrl.StartsWith("/images/products/") && p.ImageUrl.EndsWith(".svg"))
                .ToListAsync();

            if (svgProducts.Count > 0)
            {
                foreach (var prod in svgProducts)
                {
                    prod.ImageUrl = prod.ImageUrl![..^4] + ".jpg";
                }
                await context.SaveChangesAsync();
                logger.LogInformation("{Count} ürünün görseli yeni JPG fotoğraflarıyla güncellendi.", svgProducts.Count);
            }
        }
    }
}
