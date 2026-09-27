using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Orders;
[Authorize(Roles = "Admin")]
public class DetailsModel(ApplicationDbContext db, IOrderService orderService) : PageModel
{
    public Order Order { get; private set; } = null!; [BindProperty] public OrderStatus NewStatus { get; set; }
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct) => await LoadAsync(id, ct) ? Page() : NotFound();
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct) { if (!Enum.IsDefined(NewStatus)) { TempData["ErrorMessage"] = "Trạng thái không hợp lệ."; return RedirectToPage(new { id }); } if (!await orderService.UpdateStatusAsync(id, NewStatus, ct)) { TempData["ErrorMessage"] = "Không thể cập nhật đơn hàng. Đơn đã hủy không thể mở lại."; } else { TempData["SuccessMessage"] = NewStatus == OrderStatus.Cancelled ? "Đã hủy đơn và hoàn tồn kho đúng một lần." : "Đã cập nhật trạng thái đơn hàng."; } return RedirectToPage(new { id }); }
    private async Task<bool> LoadAsync(int id, CancellationToken ct) { var order = await db.Orders.AsNoTracking().Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct); if (order is null) return false; Order = order; NewStatus = order.Status; return true; }
}
