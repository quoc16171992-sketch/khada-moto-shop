using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages.Admin.Settings;

[Authorize(Roles = "Admin")]
public class IndexModel(ISiteSettingsService service) : PageModel
{
    public SiteSettings Settings { get; private set; } = new();
    public async Task OnGetAsync(CancellationToken cancellationToken) => Settings = await service.GetAsync(cancellationToken);
}
