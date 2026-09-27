using System.ComponentModel.DataAnnotations;
using Demo1.Data;
using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

public class SiteSettingsService(ApplicationDbContext db) : ISiteSettingsService
{
    private SiteSettings? _cached;

    public async Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        _cached = await db.SiteSettings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (_cached is not null)
        {
            return _cached;
        }

        var settings = new SiteSettings
        {
            ShopName = "Khada Moto Shop",
            FooterDescription = "Đồ bảo hộ và đồ chơi xe dành cho biker",
            SupportHours = "08:00 - 22:00"
        };
        db.SiteSettings.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        _cached = settings;
        return settings;
    }

    public async Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default)
    {
        if (!UrlSafety.IsSafeHttpUrl(settings.FacebookUrl))
        {
            throw new ValidationException("Link Facebook chỉ được dùng http hoặc https.");
        }

        var entity = await db.SiteSettings.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            entity = new SiteSettings();
            db.SiteSettings.Add(entity);
        }

        entity.ShopName = settings.ShopName.Trim();
        entity.PhoneNumber = settings.PhoneNumber?.Trim();
        entity.FacebookUrl = settings.FacebookUrl?.Trim();
        entity.Address = settings.Address?.Trim();
        entity.Email = settings.Email?.Trim();
        entity.FooterDescription = settings.FooterDescription?.Trim();
        entity.SupportHours = settings.SupportHours?.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        _cached = null;
    }
}
