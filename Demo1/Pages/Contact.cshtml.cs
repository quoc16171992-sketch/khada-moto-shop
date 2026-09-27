using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo1.Pages;

public class ContactModel(ISiteSettingsService settingsService) : PageModel
{
    public SiteSettings Settings { get; private set; } = new();
    public async Task OnGetAsync(CancellationToken cancellationToken) => Settings = await settingsService.GetAsync(cancellationToken);
}
