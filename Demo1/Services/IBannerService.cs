using Demo1.Models;

namespace Demo1.Services;

public interface IBannerService
{
    Task<IReadOnlyList<Banner>> GetActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}
