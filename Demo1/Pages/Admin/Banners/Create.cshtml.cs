using System.ComponentModel.DataAnnotations;
using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Admin.Banners;

[Authorize(Roles = "Admin")]
public class CreateModel(ApplicationDbContext db, IImageService imageService) : PageModel
{
    [BindProperty] public BannerInputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? BannerImage { get; set; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!UrlSafety.IsSafeTargetUrl(Input.TargetUrl)) ModelState.AddModelError("Input.TargetUrl", "Đường dẫn đích không an toàn.");
        if (!ModelState.IsValid) return Page();
        try
        {
            var imagePath = await imageService.SaveAsync(BannerImage, "banners", 8 * 1024 * 1024, cancellationToken);
            db.Banners.Add(new Banner { Title = Input.Title.Trim(), Subtitle = Input.Subtitle?.Trim(), ButtonText = Input.ButtonText?.Trim(), TargetUrl = Input.TargetUrl?.Trim(), DisplayOrder = Input.DisplayOrder, IsActive = Input.IsActive, StartAtUtc = ToUtc(Input.StartAtLocal), EndAtUtc = ToUtc(Input.EndAtLocal), ImagePath = imagePath });
            await db.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Đã thêm banner.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex) { ModelState.AddModelError(nameof(BannerImage), ex.Message); return Page(); }
    }
    private static DateTime? ToUtc(DateTime? value) => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Local).ToUniversalTime() : null;
}
