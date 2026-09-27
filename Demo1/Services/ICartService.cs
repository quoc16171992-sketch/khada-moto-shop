using Demo1.ViewModels;

namespace Demo1.Services;

public interface ICartService
{
    Task AddAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default);
    void Remove(int productVariantId);
    void Clear();
    int GetCount();
    IReadOnlyList<CartSessionItem> GetStoredItems();
    Task<CartViewModel> GetCartAsync(CancellationToken cancellationToken = default);
}
