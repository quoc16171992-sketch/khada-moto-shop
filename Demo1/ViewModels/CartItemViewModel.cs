namespace Demo1.ViewModels;

public class CartSessionItem
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class CartItemViewModel
{
    public int ProductId { get; init; }
    public int ProductVariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImagePath { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public int StockQuantity { get; init; }
    public bool IsAvailable { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
}
