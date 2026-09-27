using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Checkout;

public class IndexModel(ICartService cartService, IOrderService orderService) : PageModel
{
    [BindProperty] public CheckoutInputModel Input { get; set; } = new();
    public CartViewModel Cart { get; private set; } = new();
    public decimal ShippingFee { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCartAsync(cancellationToken);
        return Cart.Items.Count == 0 ? RedirectToPage("/Cart/Index") : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadCartAsync(cancellationToken);
        if (Cart.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng đang trống.");
        }
        if (!Cart.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng có sản phẩm không còn hợp lệ hoặc vượt tồn kho.");
        }
        if (!ModelState.IsValid) return Page();

        try
        {
            var order = await orderService.CreateAsync(Input, cancellationToken);
            TempData["CompletedOrderCode"] = order.OrderCode;
            TempData["CompletedPhoneNumber"] = order.PhoneNumber;
            return RedirectToPage("/Checkout/Success");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCartAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadCartAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);
        ShippingFee = orderService.CalculateShippingFee(Cart.Subtotal);
    }
}
