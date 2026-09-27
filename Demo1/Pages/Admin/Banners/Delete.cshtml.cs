using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Banners;
[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db, IImageService images) : PageModel
{
    public Banner Banner { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) { var x = await db.Banners.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); Banner = x; return Page(); }
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct) { var x = await db.Banners.SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound(); db.Banners.Remove(x); await db.SaveChangesAsync(ct); await images.DeleteAsync(x.ImagePath, "banners"); TempData["SuccessMessage"] = "Đã xóa banner."; return RedirectToPage("Index"); }
}
