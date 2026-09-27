using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.OrderTracking;

public class IndexModel : PageModel
{
    [BindProperty] public OrderTrackingInputModel Input { get; set; } = new();
    public void OnGet() { }
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        return RedirectToPage("/OrderTracking/Details", new { orderCode = Input.OrderCode.Trim(), phoneNumber = Input.PhoneNumber.Trim() });
    }
}
