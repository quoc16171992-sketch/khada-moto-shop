using Demo1.Data;
using Demo1.Models;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Categories;
[Authorize(Roles = "Admin")]
public class CreateModel(ApplicationDbContext db) : PageModel
{
    [BindProperty] public CategoryInputModel Input { get; set; } = new();
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(CancellationToken ct) { Input.Slug = Input.Slug.Trim().ToLowerInvariant(); if (await db.Categories.AnyAsync(x => x.Slug == Input.Slug, ct)) ModelState.AddModelError("Input.Slug", "Slug đã tồn tại."); if (!ModelState.IsValid) return Page(); db.Categories.Add(new Category { Name = Input.Name.Trim(), Slug = Input.Slug, Description = Input.Description?.Trim(), IsActive = Input.IsActive }); await db.SaveChangesAsync(ct); TempData["SuccessMessage"] = "Đã thêm danh mục."; return RedirectToPage("Index"); }
}
