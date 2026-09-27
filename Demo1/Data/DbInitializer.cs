using Demo1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        await db.Database.MigrateAsync();

        var settings = await db.SiteSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (settings is null)
        {
            db.SiteSettings.Add(new SiteSettings
            {
                ShopName = "Khada Moto Shop",
                FooterDescription = "Đồ bảo hộ và đồ chơi xe dành cho biker",
                SupportHours = "08:00 - 22:00"
            });
        }
        else
        {
            if (settings.ShopName == "KhadaShop") settings.ShopName = "Khada Moto Shop";
            if (settings.FooterDescription == "Cửa hàng mũ bảo hiểm phong cách racing")
            {
                settings.FooterDescription = "Đồ bảo hộ và đồ chơi xe dành cho biker";
            }
        }

        var categories = await db.Categories.ToListAsync();
        _ = EnsureCategory(db, categories, "do-bao-ho", "mu-fullface", "Đồ bảo hộ", "Trang bị bảo hộ dành cho người đi mô tô và xe máy.");
        _ = EnsureCategory(db, categories, "do-choi-xe", "mu-modular", "Đồ chơi xe", "Phụ kiện và đồ chơi nâng cấp cho xe.");
        var bagCategory = EnsureCategory(db, categories, "tui-deo-biker", "phu-kien", "Túi đeo biker", "Túi đeo hông tiện dụng khi đi phố và touring.");
        await db.SaveChangesAsync();

        if (!await db.Products.AnyAsync(x => x.Slug == "tui-deo-hong-moto-den-do"))
        {
            var oldProducts = await db.Products.Include(x => x.Variants).ToListAsync();
            foreach (var oldProduct in oldProducts)
            {
                oldProduct.IsActive = false;
                oldProduct.IsFeatured = false;
                oldProduct.UpdatedAtUtc = DateTime.UtcNow;
                foreach (var variant in oldProduct.Variants) variant.IsActive = false;
            }

            var products = new[]
            {
                CreateBagProduct("Túi đeo hông moto Đen đỏ", "tui-deo-hong-moto-den-do", "Phối đen đỏ thể thao, nhiều ngăn tiện dụng.", "Đen đỏ", "KMS-TUI-DD", "/images/products/tui-den-do-1.webp", "/images/products/tui-den-do-2.webp", bagCategory.Id, true, 12, 1),
                CreateBagProduct("Túi đeo hông moto Đen", "tui-deo-hong-moto-den", "Tông đen gọn gàng, dễ phối cùng đồ bảo hộ.", "Đen", "KMS-TUI-DEN", "/images/products/tui-den-1.webp", "/images/products/tui-den-2.webp", bagCategory.Id, true, 15, 2),
                CreateBagProduct("Túi đeo hông moto Hồng", "tui-deo-hong-moto-hong", "Màu hồng nổi bật với thiết kế ôm hông chắc chắn.", "Hồng", "KMS-TUI-HONG", "/images/products/tui-hong-1.webp", "/images/products/tui-hong-2.webp", bagCategory.Id, true, 9, 3),
                CreateBagProduct("Túi đeo hông moto Xanh neon", "tui-deo-hong-moto-xanh-neon", "Xanh neon nổi bật, dễ nhận diện khi di chuyển.", "Xanh neon", "KMS-TUI-XN", "/images/products/tui-xanh-neon-1.webp", "/images/products/tui-xanh-neon-2.jpg", bagCategory.Id, true, 10, 4),
                CreateBagProduct("Túi đeo hông moto Đỏ cam", "tui-deo-hong-moto-do-cam", "Màu đỏ cam cá tính, hợp phong cách touring.", "Đỏ cam", "KMS-TUI-DC", "/images/products/tui-do-cam-1.webp", "/images/products/tui-do-cam-2.webp", bagCategory.Id, false, 11, 5),
                CreateBagProduct("Túi đeo hông moto Trắng camo", "tui-deo-hong-moto-trang-camo", "Họa tiết camo sáng hiện đại, form túi cứng cáp.", "Trắng camo", "KMS-TUI-TC", "/images/products/tui-trang-camo-1.webp", null, bagCategory.Id, false, 8, 6)
            };
            db.Products.AddRange(products);
        }

        if (!await db.Products.AnyAsync(x => x.Slug == "tui-deo-cheo-moto-led"))
        {
            var ledBag = CreateBagProduct(
                "Túi đeo chéo moto LED thông minh",
                "tui-deo-cheo-moto-led",
                "Túi đeo chéo màu đen với mặt hiển thị LED nổi bật.",
                "Đen",
                "KMS-TUI-LED-DEN",
                "/images/products/tui-led-smart-1.webp",
                "/images/products/tui-led-smart-2.webp",
                bagCategory.Id,
                true,
                8,
                0,
                690_000m,
                790_000m);
            ledBag.Description = "Túi đeo chéo moto có mặt hiển thị LED cá tính, khoang chứa nhiều ngăn và dây đeo điều chỉnh linh hoạt. Thiết kế cứng cáp, phù hợp đi phố hoặc đồng hành trong các chuyến touring.";
            ledBag.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/tui-led-smart-3.webp", DisplayOrder = 3 });
            ledBag.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/tui-led-smart-4.webp", DisplayOrder = 4 });
            ledBag.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/tui-led-smart-5.webp", DisplayOrder = 5 });
            db.Products.Add(ledBag);
        }

        if (!await db.Products.AnyAsync(x => x.Slug == "balo-moto-led-thong-minh"))
        {
            var ledBackpack = CreateBagProduct(
                "Balo moto LED thông minh",
                "balo-moto-led-thong-minh",
                "Balo moto vỏ cứng với hai mặt hiển thị LED nổi bật.",
                "Đen",
                "KMS-BALO-LED-DEN",
                "/images/products/balo-led-smart-1.webp",
                "/images/products/balo-led-smart-2.webp",
                bagCategory.Id,
                true,
                6,
                0,
                1_290_000m,
                1_490_000m);
            ledBackpack.Description = "Balo moto vỏ cứng có hai mặt hiển thị LED, kết nối Bluetooth và sử dụng nguồn pin dự phòng. Kích thước khoảng 32 × 9 × 20 cm, trọng lượng khoảng 0,8 kg; khoang chứa rộng và hệ dây đeo chắc chắn phù hợp đi phố hoặc touring.";
            ledBackpack.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/balo-led-smart-3.webp", DisplayOrder = 3 });
            ledBackpack.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/balo-led-smart-4.webp", DisplayOrder = 4 });
            ledBackpack.GalleryImages.Add(new ProductGalleryImage { ImagePath = "/images/products/balo-led-smart-5.webp", DisplayOrder = 5 });
            db.Products.Add(ledBackpack);
        }

        const string firstBannerImage = "/images/banner-rider-1.jpg";
        const string secondBannerImage = "/images/banner-rider-2.jpg";
        var existingBanners = await db.Banners.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync();
        if (existingBanners.Count == 0)
        {
            db.Banners.AddRange(
                new Banner { Title = "Bứt phá phong cách cùng Khada Moto Shop", Subtitle = "Khám phá đồ bảo hộ và phụ kiện xe đậm chất biker.", ImagePath = firstBannerImage, ButtonText = "Xem sản phẩm", TargetUrl = "/san-pham", DisplayOrder = 1, IsActive = true },
                new Banner { Title = "An tâm trên mọi hành trình", Subtitle = "Khám phá sản phẩm từ nhiều góc với thông tin rõ ràng.", ImagePath = secondBannerImage, ButtonText = "Khám phá ngay", TargetUrl = "/san-pham", DisplayOrder = 2, IsActive = true });
        }
        else
        {
            var firstBanner = existingBanners.FirstOrDefault(x => x.DisplayOrder == 1);
            var secondBanner = existingBanners.FirstOrDefault(x => x.DisplayOrder == 2);
            var thirdSeedBanner = existingBanners.FirstOrDefault(x => x.DisplayOrder == 3 && x.Title == "Tư vấn chọn size tận tâm");
            var isOriginalSeedSet = firstBanner is not null && secondBanner is not null
                && string.IsNullOrWhiteSpace(firstBanner.ImagePath)
                && string.IsNullOrWhiteSpace(secondBanner.ImagePath);

            if (isOriginalSeedSet)
            {
                firstBanner!.ImagePath = firstBannerImage;
                firstBanner.UpdatedAtUtc = DateTime.UtcNow;
                secondBanner!.ImagePath = secondBannerImage;
                secondBanner.UpdatedAtUtc = DateTime.UtcNow;
                if (thirdSeedBanner is not null)
                {
                    thirdSeedBanner.IsActive = false;
                    thirdSeedBanner.UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            if (firstBanner is not null)
            {
                if (firstBanner.Title == "Bứt phá phong cách cùng KhadaShop") firstBanner.Title = "Bứt phá phong cách cùng Khada Moto Shop";
                if (firstBanner.Subtitle == "Khám phá bộ sưu tập mũ fullface đậm chất racing.") firstBanner.Subtitle = "Khám phá đồ bảo hộ và phụ kiện xe đậm chất biker.";
            }
            if (secondBanner is not null && secondBanner.Subtitle == "Kiểm tra hàng trước khi nhận, thanh toán COD thuận tiện.")
            {
                secondBanner.Subtitle = "Khám phá sản phẩm từ nhiều góc với thông tin rõ ràng.";
            }
        }

        await db.SaveChangesAsync();
        await InitializeAdminAsync(services, configuration, logger);
    }

    private static Category EnsureCategory(ApplicationDbContext db, List<Category> categories, string slug, string legacySlug, string name, string description)
    {
        var category = categories.FirstOrDefault(x => x.Slug == slug)
            ?? categories.FirstOrDefault(x => x.Slug == legacySlug);
        if (category is null)
        {
            category = new Category();
            categories.Add(category);
            db.Categories.Add(category);
        }

        category.Name = name;
        category.Slug = slug;
        category.Description = description;
        category.IsActive = true;
        return category;
    }

    private static Product CreateBagProduct(string name, string slug, string shortDescription, string color, string sku, string imagePath, string? secondaryImagePath, int categoryId, bool featured, int stock, int displayOrder, decimal basePrice = 390_000m, decimal? compareAtPrice = 450_000m)
    {
        var product = new Product
        {
            Name = name,
            Slug = slug,
            ShortDescription = shortDescription,
            Description = $"{shortDescription} Thiết kế nhiều ngăn, dây đeo điều chỉnh linh hoạt và mặt lưng êm, phù hợp mang theo vật dụng cá nhân khi đi phố hoặc touring.",
            BasePrice = basePrice,
            CompareAtPrice = compareAtPrice,
            ImagePath = imagePath,
            SecondaryImagePath = secondaryImagePath,
            CategoryId = categoryId,
            IsActive = true,
            IsFeatured = featured,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-displayOrder)
        };

        product.Variants.Add(new ProductVariant
        {
            Size = "Freesize",
            Color = color,
            Sku = sku,
            StockQuantity = stock,
            PriceAdjustment = 0,
            IsActive = true
        });
        return product;
    }

    private static async Task InitializeAdminAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        const string roleName = "Admin";
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Không thể tạo role Admin: " + string.Join("; ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        var email = configuration["AdminSeed:Email"]?.Trim();
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Chưa cấu hình AdminSeed; bỏ qua việc tạo tài khoản quản trị.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Không thể tạo tài khoản Admin: " + string.Join("; ", createResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException("Không thể gán role Admin: " + string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
            }
        }
    }
}
