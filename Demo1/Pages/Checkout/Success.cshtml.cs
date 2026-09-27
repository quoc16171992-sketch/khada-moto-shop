using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Checkout;

public class SuccessModel : PageModel
{
    [TempData] public string? CompletedOrderCode { get; set; }
    [TempData] public string? CompletedPhoneNumber { get; set; }
    public IActionResult OnGet() => string.IsNullOrWhiteSpace(CompletedOrderCode) ? RedirectToPage("/Index") : Page();
}
