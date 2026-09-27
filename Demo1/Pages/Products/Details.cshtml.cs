using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Products;

public class DetailsModel(ApplicationDbContext db, ISiteSettingsService settingsService) : PageModel
{
    public ProductDetailViewModel Detail { get; private set; } = null!;
    public SiteSettings Settings { get; private set; } = new();
    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
        => await LoadAsync(slug, cancellationToken) ? Page() : NotFound();

    private async Task<bool> LoadAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants).Include(x => x.GalleryImages)
            .SingleOrDefaultAsync(x => x.Slug == slug && x.IsActive && x.Category.IsActive, cancellationToken);
        if (product is null) return false;

        Detail = new ProductDetailViewModel
        {
            Product = product,
            Variants = product.Variants.Where(x => x.IsActive).OrderBy(x => x.Size).ThenBy(x => x.Color).ToList(),
            RelatedProducts = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants)
                .Where(x => x.IsActive && x.CategoryId == product.CategoryId && x.Id != product.Id).Take(4).ToListAsync(cancellationToken)
        };
        Settings = await settingsService.GetAsync(cancellationToken);
        return true;
    }
}
