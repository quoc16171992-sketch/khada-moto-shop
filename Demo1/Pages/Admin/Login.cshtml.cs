using System.ComponentModel.DataAnnotations;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Demo1.Pages.Admin;
[AllowAnonymous]
public class LoginModel(SignInManager<ApplicationUser> signInManager, ISiteSettingsService settingsService) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public string ShopName { get; private set; } = "Khada Moto Shop";
    public async Task<IActionResult> OnGetAsync(string? returnUrl) { ShopName = (await settingsService.GetAsync()).ShopName; if (User.IsInRole("Admin")) return RedirectToPage("/Admin/Index"); ViewData["ReturnUrl"] = returnUrl; return Page(); }
    public async Task<IActionResult> OnPostAsync(string? returnUrl) { ShopName = (await settingsService.GetAsync()).ShopName; if (!ModelState.IsValid) return Page(); var result = await signInManager.PasswordSignInAsync(Input.Email.Trim(), Input.Password, Input.RememberMe, true); if (result.Succeeded) return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl) ? "/Admin" : returnUrl); ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Tài khoản tạm khóa do đăng nhập sai nhiều lần." : "Email hoặc mật khẩu không đúng."); return Page(); }
    public class LoginInput { [Required, EmailAddress] public string Email { get; set; } = string.Empty; [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty; [Display(Name = "Ghi nhớ đăng nhập")] public bool RememberMe { get; set; } }
}
