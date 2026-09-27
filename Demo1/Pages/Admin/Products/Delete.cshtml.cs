using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Products;
[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db, IImageService images) : PageModel
{
    public Product Product { get; private set; } = null!; public bool HasOrders { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) { var x = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); Product = x; HasOrders = await db.OrderItems.AnyAsync(x => x.ProductId == id, ct); return Page(); }
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct) { var x = await db.Products.Include(x => x.Variants).Include(x => x.GalleryImages).SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); if (await db.OrderItems.AnyAsync(i => i.ProductId == id, ct)) { x.IsActive = false; x.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Sản phẩm đã có trong đơn hàng nên được tắt thay vì xóa."; } else { var primaryPath = x.ImagePath; var secondaryPath = x.SecondaryImagePath; var galleryPaths = x.GalleryImages.Select(image => image.ImagePath).ToList(); db.Products.Remove(x); await db.SaveChangesAsync(ct); await images.DeleteAsync(primaryPath, "products"); await images.DeleteAsync(secondaryPath, "products"); foreach (var galleryPath in galleryPaths) await images.DeleteAsync(galleryPath, "products"); TempData["SuccessMessage"] = "Đã xóa sản phẩm."; } return RedirectToPage("Index"); }
}
