using Demo1.Models;

namespace Demo1.ViewModels;

public class ProductDetailViewModel
{
    public Product Product { get; init; } = null!;
    public IReadOnlyList<ProductVariant> Variants { get; init; } = [];
    public IReadOnlyList<Product> RelatedProducts { get; init; } = [];
}
