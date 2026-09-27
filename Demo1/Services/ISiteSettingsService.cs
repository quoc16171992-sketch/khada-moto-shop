using Demo1.Models;

namespace Demo1.Services;

public interface ISiteSettingsService
{
    Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default);
}
