using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Demo1.Pages.Admin;
[Authorize(Roles = "Admin")]
public class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel { public async Task<IActionResult> OnPostAsync() { await signInManager.SignOutAsync(); return RedirectToPage("/Index"); } }
