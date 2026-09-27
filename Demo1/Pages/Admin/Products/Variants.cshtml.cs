using Demo1.Data;
using Demo1.Models;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Admin.Products;

[Authorize(Roles = "Admin")]
public class VariantsModel(ApplicationDbContext db) : PageModel
{
    public Product Product { get; private set; } = null!;
    public IReadOnlyList<ProductVariant> Variants { get; private set; } = [];
    [BindProperty] public VariantInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) => await LoadAsync(id, ct) ? Page() : NotFound();

    public async Task<IActionResult> OnPostAddAsync(int id, CancellationToken ct)
    {
        Normalize(); await ValidateUniqueAsync(id, null, ct);
        if (!ModelState.IsValid) { if (!await LoadAsync(id, ct)) return NotFound(); return Page(); }
        db.ProductVariants.Add(new ProductVariant { ProductId = id, Size = Input.Size, Color = Input.Color, Sku = Input.Sku, StockQuantity = Input.StockQuantity, PriceAdjustment = Input.PriceAdjustment, IsActive = Input.IsActive });
        await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Đã thêm biến thể."; return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, int variantId, CancellationToken ct)
    {
        var variant = await db.ProductVariants.SingleOrDefaultAsync(x => x.Id == variantId && x.ProductId == id, ct); if (variant is null) return NotFound();
        Normalize(); await ValidateUniqueAsync(id, variantId, ct);
        if (!ModelState.IsValid) { if (!await LoadAsync(id, ct)) return NotFound(); return Page(); }
        variant.Size = Input.Size; variant.Color = Input.Color; variant.Sku = Input.Sku; variant.StockQuantity = Input.StockQuantity; variant.PriceAdjustment = Input.PriceAdjustment; variant.IsActive = Input.IsActive; await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Đã cập nhật biến thể."; return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int variantId, CancellationToken ct)
    {
        var variant = await db.ProductVariants.SingleOrDefaultAsync(x => x.Id == variantId && x.ProductId == id, ct); if (variant is null) return NotFound();
        if (await db.OrderItems.AnyAsync(x => x.ProductVariantId == variantId, ct)) { variant.IsActive = false; variant.StockQuantity = 0; TempData["SuccessMessage"] = "Biến thể đã có trong đơn hàng nên được tắt thay vì xóa."; } else { db.ProductVariants.Remove(variant); TempData["SuccessMessage"] = "Đã xóa biến thể."; }
        await db.SaveChangesAsync(ct); return RedirectToPage(new { id });
    }

    private async Task<bool> LoadAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (product is null) return false; Product = product; Variants = await db.ProductVariants.AsNoTracking().Where(x => x.ProductId == id).OrderBy(x => x.Size).ThenBy(x => x.Color).ToListAsync(ct); return true;
    }
    private void Normalize() { Input.Size = Input.Size.Trim().ToUpperInvariant(); Input.Color = Input.Color.Trim(); Input.Sku = Input.Sku.Trim().ToUpperInvariant(); }
    private async Task ValidateUniqueAsync(int productId, int? variantId, CancellationToken ct)
    {
        if (await db.ProductVariants.AnyAsync(x => x.Sku == Input.Sku && x.Id != variantId, ct)) ModelState.AddModelError("Input.Sku", "SKU đã tồn tại.");
        if (await db.ProductVariants.AnyAsync(x => x.ProductId == productId && x.Size == Input.Size && x.Color == Input.Color && x.Id != variantId, ct)) ModelState.AddModelError(string.Empty, "Biến thể cùng size và màu đã tồn tại.");
    }
}
