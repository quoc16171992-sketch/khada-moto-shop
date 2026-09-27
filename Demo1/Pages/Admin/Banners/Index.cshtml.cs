using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Admin.Banners;

[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<Banner> Banners { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken cancellationToken) => Banners = await db.Banners.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(cancellationToken);
}
