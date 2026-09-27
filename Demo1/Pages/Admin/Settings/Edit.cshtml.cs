using System.ComponentModel.DataAnnotations;
using Demo1.Models;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Admin.Settings;

[Authorize(Roles = "Admin")]
public class EditModel(ISiteSettingsService service) : PageModel
{
    [BindProperty] public SiteSettingsInputModel Input { get; set; } = new();
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var x = await service.GetAsync(cancellationToken);
        Input = new SiteSettingsInputModel { ShopName = x.ShopName, PhoneNumber = x.PhoneNumber, FacebookUrl = x.FacebookUrl, Address = x.Address, Email = x.Email, FooterDescription = x.FooterDescription, SupportHours = x.SupportHours };
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!UrlSafety.IsSafeHttpUrl(Input.FacebookUrl)) ModelState.AddModelError("Input.FacebookUrl", "Link Facebook chỉ được dùng http hoặc https.");
        if (!ModelState.IsValid) return Page();
        try
        {
            await service.UpdateAsync(new SiteSettings { ShopName = Input.ShopName, PhoneNumber = Input.PhoneNumber, FacebookUrl = Input.FacebookUrl, Address = Input.Address, Email = Input.Email, FooterDescription = Input.FooterDescription, SupportHours = Input.SupportHours }, cancellationToken);
            TempData["SuccessMessage"] = "Đã cập nhật thông tin cửa hàng.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex) { ModelState.AddModelError(string.Empty, ex.Message); return Page(); }
    }
}
