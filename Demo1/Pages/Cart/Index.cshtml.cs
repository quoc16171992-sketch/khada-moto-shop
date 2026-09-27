using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Cart;

public class IndexModel(ICartService cartService) : PageModel
{
    public CartViewModel Cart { get; private set; } = new();
    public async Task OnGetAsync(CancellationToken cancellationToken) => Cart = await cartService.GetCartAsync(cancellationToken);

    public async Task<IActionResult> OnPostUpdateAsync(int productVariantId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await cartService.UpdateAsync(productVariantId, quantity, cancellationToken);
            TempData["SuccessMessage"] = "Đã cập nhật giỏ hàng.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int productVariantId)
    {
        cartService.Remove(productVariantId);
        TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ.";
        return RedirectToPage();
    }
}
