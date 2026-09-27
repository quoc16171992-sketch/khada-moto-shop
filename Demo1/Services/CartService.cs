using Demo1.Data;
using Demo1.Extensions;
using Demo1.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

public class CartService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ICartService
{
    private const string SessionKey = "KhadaShop.Cart";
    private ISession Session => httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session chưa sẵn sàng.");

    public async Task AddAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);
        var variant = await db.ProductVariants.Include(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == productVariantId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy biến thể sản phẩm.");

        if (!variant.IsActive || !variant.Product.IsActive)
        {
            throw new InvalidOperationException("Sản phẩm hiện không còn bán.");
        }

        var items = ReadItems();
        var existing = items.SingleOrDefault(x => x.ProductVariantId == productVariantId);
        var requested = quantity + (existing?.Quantity ?? 0);
        if (requested > variant.StockQuantity)
        {
            throw new InvalidOperationException("Số lượng vượt quá tồn kho.");
        }

        if (existing is null)
        {
            items.Add(new CartSessionItem { ProductVariantId = productVariantId, Quantity = quantity });
        }
        else
        {
            existing.Quantity = requested;
        }

        SaveItems(items);
    }

    public async Task UpdateAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);
        var variant = await db.ProductVariants.Include(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == productVariantId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy biến thể sản phẩm.");
        if (!variant.IsActive || !variant.Product.IsActive || quantity > variant.StockQuantity)
        {
            throw new InvalidOperationException("Số lượng hoặc sản phẩm không còn hợp lệ.");
        }

        var items = ReadItems();
        var existing = items.SingleOrDefault(x => x.ProductVariantId == productVariantId)
            ?? throw new InvalidOperationException("Sản phẩm không có trong giỏ.");
        existing.Quantity = quantity;
        SaveItems(items);
    }

    public void Remove(int productVariantId)
    {
        var items = ReadItems();
        items.RemoveAll(x => x.ProductVariantId == productVariantId);
        SaveItems(items);
    }

    public void Clear() => Session.Remove(SessionKey);
    public int GetCount() => ReadItems().Sum(x => x.Quantity);
    public IReadOnlyList<CartSessionItem> GetStoredItems() => ReadItems();

    public async Task<CartViewModel> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var stored = ReadItems();
        if (stored.Count == 0)
        {
            return new CartViewModel();
        }

        var ids = stored.Select(x => x.ProductVariantId).Distinct().ToArray();
        var variants = await db.ProductVariants.AsNoTracking().Include(x => x.Product)
            .Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var result = new List<CartItemViewModel>();

        foreach (var item in stored)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var variant))
            {
                continue;
            }

            result.Add(new CartItemViewModel
            {
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                Slug = variant.Product.Slug,
                ImagePath = variant.Product.ImagePath,
                Sku = variant.Sku,
                Size = variant.Size,
                Color = variant.Color,
                UnitPrice = variant.Product.BasePrice + variant.PriceAdjustment,
                Quantity = item.Quantity,
                StockQuantity = variant.StockQuantity,
                IsAvailable = variant.IsActive && variant.Product.IsActive && variant.StockQuantity > 0
            });
        }

        return new CartViewModel { Items = result };
    }

    public static void ValidateQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Số lượng phải từ 1 trở lên.");
        }
    }

    private List<CartSessionItem> ReadItems() => Session.GetJson<List<CartSessionItem>>(SessionKey) ?? [];
    private void SaveItems(List<CartSessionItem> items) => Session.SetJson(SessionKey, items);
}
