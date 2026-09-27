using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Demo1.Pages.Admin.Categories;
[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<Category> Categories { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => Categories = await db.Categories.AsNoTracking().Include(x => x.Products).OrderBy(x => x.Name).ToListAsync(ct);
}
