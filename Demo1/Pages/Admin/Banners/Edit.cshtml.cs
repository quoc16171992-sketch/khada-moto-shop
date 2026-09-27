using System.ComponentModel.DataAnnotations;
using Demo1.Data;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Admin.Banners;

[Authorize(Roles = "Admin")]
public class EditModel(ApplicationDbContext db, IImageService imageService) : PageModel
{
    [BindProperty] public BannerInputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? BannerImage { get; set; }
    public string? CurrentImagePath { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var x = await db.Banners.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound();
        CurrentImagePath = x.ImagePath; Input = new BannerInputModel { Title = x.Title, Subtitle = x.Subtitle, ButtonText = x.ButtonText, TargetUrl = x.TargetUrl, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, StartAtLocal = x.StartAtUtc?.ToLocalTime(), EndAtLocal = x.EndAtUtc?.ToLocalTime() }; return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var entity = await db.Banners.SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return NotFound(); CurrentImagePath = entity.ImagePath;
        if (!UrlSafety.IsSafeTargetUrl(Input.TargetUrl)) ModelState.AddModelError("Input.TargetUrl", "Đường dẫn đích không an toàn."); if (!ModelState.IsValid) return Page();
        try
        {
            var newPath = await imageService.SaveAsync(BannerImage, "banners", 8 * 1024 * 1024, ct); var oldPath = entity.ImagePath;
            entity.Title = Input.Title.Trim(); entity.Subtitle = Input.Subtitle?.Trim(); entity.ButtonText = Input.ButtonText?.Trim(); entity.TargetUrl = Input.TargetUrl?.Trim(); entity.DisplayOrder = Input.DisplayOrder; entity.IsActive = Input.IsActive; entity.StartAtUtc = ToUtc(Input.StartAtLocal); entity.EndAtUtc = ToUtc(Input.EndAtLocal); entity.UpdatedAtUtc = DateTime.UtcNow; if (newPath is not null) entity.ImagePath = newPath;
            await db.SaveChangesAsync(ct); if (newPath is not null) await imageService.DeleteAsync(oldPath, "banners"); TempData["SuccessMessage"] = "Đã cập nhật banner."; return RedirectToPage("Index");
        }
        catch (ValidationException ex) { ModelState.AddModelError(nameof(BannerImage), ex.Message); return Page(); }
    }
    private static DateTime? ToUtc(DateTime? value) => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Local).ToUniversalTime() : null;
}
