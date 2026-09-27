using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Products;
[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    public IReadOnlyList<Product> Products { get; private set; } = [];
    public IReadOnlyList<Category> Categories { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) { var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants).AsQueryable(); if (!string.IsNullOrWhiteSpace(Search)) { var term = Search.Trim(); query = query.Where(x => EF.Functions.Like(x.Name, $"%{term}%")); } if (CategoryId.HasValue) query = query.Where(x => x.CategoryId == CategoryId); Products = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct); Categories = await db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct); }
}
