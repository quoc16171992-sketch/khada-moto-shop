using Demo1.Data;
using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

public class BannerService(ApplicationDbContext db) : IBannerService
{
    public async Task<IReadOnlyList<Banner>> GetActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        => await db.Banners.AsNoTracking()
            .Where(x => x.IsActive
                && (!x.StartAtUtc.HasValue || x.StartAtUtc <= utcNow)
                && (!x.EndAtUtc.HasValue || x.EndAtUtc > utcNow))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
}
