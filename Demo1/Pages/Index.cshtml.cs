using Microsoft.AspNetCore.Mvc;
using Demo1.Data;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages;

public class IndexModel(ApplicationDbContext db, IBannerService bannerService, ISiteSettingsService settingsService) : PageModel
{
    public HomeViewModel Home { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var featured = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants)
            .Where(x => x.IsActive && x.IsFeatured && x.Category.IsActive).OrderByDescending(x => x.CreatedAtUtc).Take(4).ToListAsync(cancellationToken);
        var newest = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants)
            .Where(x => x.IsActive && x.Category.IsActive).OrderByDescending(x => x.CreatedAtUtc).Take(8).ToListAsync(cancellationToken);

        Home = new HomeViewModel
        {
            Banners = await bannerService.GetActiveAsync(DateTime.UtcNow, cancellationToken),
            Categories = categories,
            FeaturedProducts = featured,
            NewProducts = newest,
            Settings = await settingsService.GetAsync(cancellationToken)
        };
    }
}
