using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Orders;
[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public OrderStatus? Status { get; set; }
    public IReadOnlyList<Order> Orders { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) { var query = db.Orders.AsNoTracking().AsQueryable(); if (!string.IsNullOrWhiteSpace(Search)) { var term = Search.Trim(); query = query.Where(x => EF.Functions.Like(x.OrderCode, $"%{term}%") || EF.Functions.Like(x.PhoneNumber, $"%{term}%")); } if (Status.HasValue) query = query.Where(x => x.Status == Status); Orders = await query.OrderByDescending(x => x.CreatedAtUtc).Take(200).ToListAsync(ct); }
}
