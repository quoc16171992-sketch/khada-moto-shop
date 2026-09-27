using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Categories;
[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db) : PageModel
{
    public Category Category { get; private set; } = null!; public int ProductCount { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) { var x = await db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); Category = x; ProductCount = await db.Products.CountAsync(x => x.CategoryId == id, ct); return Page(); }
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct) { var x = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); if (await db.Products.AnyAsync(p => p.CategoryId == id, ct)) { TempData["ErrorMessage"] = "Không thể xóa danh mục vì vẫn còn sản phẩm. Hãy tắt danh mục hoặc chuyển sản phẩm trước."; return RedirectToPage("Index"); } db.Categories.Remove(x); await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Đã xóa danh mục."; return RedirectToPage("Index"); }
}
