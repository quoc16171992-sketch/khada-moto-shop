using System.ComponentModel.DataAnnotations;
using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Products;
[Authorize(Roles = "Admin")]
public class CreateModel(ApplicationDbContext db, IImageService images) : PageModel, IProductAdminPageModel
{
    [BindProperty] public ProductInputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? ProductImage { get; set; }
    [BindProperty] public IFormFile? ProductSecondaryImage { get; set; }
    [BindProperty] public IFormFile[] ProductGalleryImages { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => await LoadCategories(ct);
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.Slug = Input.Slug.Trim().ToLowerInvariant();
        if (await db.Products.AnyAsync(x => x.Slug == Input.Slug, ct)) ModelState.AddModelError("Input.Slug", "Slug đã tồn tại.");
        if (!await db.Categories.AnyAsync(x => x.Id == Input.CategoryId, ct)) ModelState.AddModelError("Input.CategoryId", "Danh mục không hợp lệ.");
        if (!ModelState.IsValid) { await LoadCategories(ct); return Page(); }

        string? primaryPath = null;
        string? secondaryPath = null;
        var galleryPaths = new List<string>();
        try
        {
            primaryPath = await images.SaveAsync(ProductImage, "products", 5 * 1024 * 1024, ct);
            secondaryPath = await images.SaveAsync(ProductSecondaryImage, "products", 5 * 1024 * 1024, ct);
            foreach (var galleryImage in ProductGalleryImages)
            {
                var path = await images.SaveAsync(galleryImage, "products", 5 * 1024 * 1024, ct);
                if (path is not null) galleryPaths.Add(path);
            }
            var product = new Product { Name = Input.Name.Trim(), Slug = Input.Slug, ShortDescription = Input.ShortDescription.Trim(), Description = Input.Description.Trim(), BasePrice = Input.BasePrice, CompareAtPrice = Input.CompareAtPrice, CategoryId = Input.CategoryId, IsActive = Input.IsActive, IsFeatured = Input.IsFeatured, ImagePath = primaryPath, SecondaryImagePath = secondaryPath };
            for (var index = 0; index < galleryPaths.Count; index++)
            {
                product.GalleryImages.Add(new ProductGalleryImage { ImagePath = galleryPaths[index], DisplayOrder = index + 3 });
            }
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);
            TempData["SuccessMessage"] = "Đã thêm sản phẩm. Hãy tạo các phiên bản và tồn kho.";
            return RedirectToPage("Variants", new { id = product.Id });
        }
        catch (ValidationException ex)
        {
            await images.DeleteAsync(primaryPath, "products");
            await images.DeleteAsync(secondaryPath, "products");
            foreach (var galleryPath in galleryPaths) await images.DeleteAsync(galleryPath, "products");
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategories(ct);
            return Page();
        }
    }
    private async Task LoadCategories(CancellationToken ct) => Categories = await db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
}
