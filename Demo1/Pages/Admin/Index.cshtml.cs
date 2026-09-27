using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin;
[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public int ProductCount { get; private set; } public int VariantCount { get; private set; } public int BannerCount { get; private set; } public int OrderCount { get; private set; } public int PendingOrderCount { get; private set; } public int LowStockCount { get; private set; } public int ActiveBannerCount { get; private set; } public decimal CompletedRevenue { get; private set; }
    public async Task OnGetAsync(CancellationToken ct) { var now = DateTime.UtcNow; ProductCount = await db.Products.CountAsync(ct); VariantCount = await db.ProductVariants.CountAsync(ct); BannerCount = await db.Banners.CountAsync(ct); OrderCount = await db.Orders.CountAsync(ct); PendingOrderCount = await db.Orders.CountAsync(x => x.Status == OrderStatus.Pending, ct); LowStockCount = await db.ProductVariants.CountAsync(x => x.IsActive && x.StockQuantity < 5, ct); ActiveBannerCount = await db.Banners.CountAsync(x => x.IsActive && (!x.StartAtUtc.HasValue || x.StartAtUtc <= now) && (!x.EndAtUtc.HasValue || x.EndAtUtc > now), ct); CompletedRevenue = await db.Orders.Where(x => x.Status == OrderStatus.Completed).SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0; }
}
