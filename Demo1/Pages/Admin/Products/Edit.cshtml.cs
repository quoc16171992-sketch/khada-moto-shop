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
public class EditModel(ApplicationDbContext db, IImageService images) : PageModel, IProductAdminPageModel
{
    [BindProperty] public ProductInputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? ProductImage { get; set; }
    [BindProperty] public IFormFile? ProductSecondaryImage { get; set; }
    [BindProperty] public IFormFile[] ProductGalleryImages { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; private set; } = [];
    public string? CurrentImagePath { get; private set; }
    public string? CurrentSecondaryImagePath { get; private set; }
    public IReadOnlyList<string> CurrentGalleryImagePaths { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var x = await db.Products.AsNoTracking().Include(x => x.GalleryImages).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (x is null) return NotFound();
        Input = new ProductInputModel { Name = x.Name, Slug = x.Slug, ShortDescription = x.ShortDescription, Description = x.Description, BasePrice = x.BasePrice, CompareAtPrice = x.CompareAtPrice, CategoryId = x.CategoryId, IsActive = x.IsActive, IsFeatured = x.IsFeatured };
        SetCurrentImages(x);
        await LoadCategories(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var x = await db.Products.Include(x => x.GalleryImages).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (x is null) return NotFound();
        SetCurrentImages(x);
        Input.Slug = Input.Slug.Trim().ToLowerInvariant();
        if (await db.Products.AnyAsync(p => p.Slug == Input.Slug && p.Id != id, ct)) ModelState.AddModelError("Input.Slug", "Slug đã tồn tại.");
        if (!await db.Categories.AnyAsync(c => c.Id == Input.CategoryId, ct)) ModelState.AddModelError("Input.CategoryId", "Danh mục không hợp lệ.");
        if (!ModelState.IsValid) { await LoadCategories(ct); return Page(); }

        string? newPrimaryPath = null;
        string? newSecondaryPath = null;
        var newGalleryPaths = new List<string>();
        try
        {
            newPrimaryPath = await images.SaveAsync(ProductImage, "products", 5 * 1024 * 1024, ct);
            newSecondaryPath = await images.SaveAsync(ProductSecondaryImage, "products", 5 * 1024 * 1024, ct);
            foreach (var galleryImage in ProductGalleryImages)
            {
                var path = await images.SaveAsync(galleryImage, "products", 5 * 1024 * 1024, ct);
                if (path is not null) newGalleryPaths.Add(path);
            }
            var oldPrimaryPath = x.ImagePath;
            var oldSecondaryPath = x.SecondaryImagePath;
            var oldGalleryPaths = x.GalleryImages.Select(image => image.ImagePath).ToList();
            x.Name = Input.Name.Trim(); x.Slug = Input.Slug; x.ShortDescription = Input.ShortDescription.Trim(); x.Description = Input.Description.Trim(); x.BasePrice = Input.BasePrice; x.CompareAtPrice = Input.CompareAtPrice; x.CategoryId = Input.CategoryId; x.IsActive = Input.IsActive; x.IsFeatured = Input.IsFeatured; x.UpdatedAtUtc = DateTime.UtcNow;
            if (newPrimaryPath is not null) x.ImagePath = newPrimaryPath;
            if (newSecondaryPath is not null) x.SecondaryImagePath = newSecondaryPath;
            if (newGalleryPaths.Count > 0)
            {
                db.ProductGalleryImages.RemoveRange(x.GalleryImages);
                for (var index = 0; index < newGalleryPaths.Count; index++)
                {
                    x.GalleryImages.Add(new ProductGalleryImage { ImagePath = newGalleryPaths[index], DisplayOrder = index + 3 });
                }
            }
            await db.SaveChangesAsync(ct);
            if (newPrimaryPath is not null) await images.DeleteAsync(oldPrimaryPath, "products");
            if (newSecondaryPath is not null) await images.DeleteAsync(oldSecondaryPath, "products");
            if (newGalleryPaths.Count > 0)
            {
                foreach (var oldGalleryPath in oldGalleryPaths) await images.DeleteAsync(oldGalleryPath, "products");
            }
            TempData["SuccessMessage"] = "Đã cập nhật sản phẩm.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            await images.DeleteAsync(newPrimaryPath, "products");
            await images.DeleteAsync(newSecondaryPath, "products");
            foreach (var galleryPath in newGalleryPaths) await images.DeleteAsync(galleryPath, "products");
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategories(ct);
            return Page();
        }
    }

    private void SetCurrentImages(Product product)
    {
        CurrentImagePath = product.ImagePath;
        CurrentSecondaryImagePath = product.SecondaryImagePath;
        CurrentGalleryImagePaths = product.GalleryImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath).ToList();
    }
    private async Task LoadCategories(CancellationToken ct) => Categories = await db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
}
