using Demo1.Data;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Categories;
[Authorize(Roles = "Admin")]
public class EditModel(ApplicationDbContext db) : PageModel
{
    [BindProperty] public CategoryInputModel Input { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) { var x = await db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); Input = new CategoryInputModel { Name = x.Name, Slug = x.Slug, Description = x.Description, IsActive = x.IsActive }; return Page(); }
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct) { var x = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); Input.Slug = Input.Slug.Trim().ToLowerInvariant(); if (await db.Categories.AnyAsync(c => c.Slug == Input.Slug && c.Id != id, ct)) ModelState.AddModelError("Input.Slug", "Slug đã tồn tại."); if (!ModelState.IsValid) return Page(); x.Name = Input.Name.Trim(); x.Slug = Input.Slug; x.Description = Input.Description?.Trim(); x.IsActive = Input.IsActive; await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Đã cập nhật danh mục."; return RedirectToPage("Index"); }
}
