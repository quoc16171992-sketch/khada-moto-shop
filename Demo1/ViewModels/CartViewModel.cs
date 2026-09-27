namespace Demo1.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; init; } = [];
    public decimal Subtotal => Items.Sum(x => x.LineTotal);
    public int TotalQuantity => Items.Sum(x => x.Quantity);
    public bool IsValid => Items.Count > 0 && Items.All(x => x.IsAvailable && x.Quantity <= x.StockQuantity);
}
