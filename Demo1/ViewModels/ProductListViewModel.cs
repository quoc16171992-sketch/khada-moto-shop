using Demo1.Models;

namespace Demo1.ViewModels;

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public IReadOnlyList<string> Sizes { get; init; } = [];
    public IReadOnlyList<string> Colors { get; init; } = [];
    public int PageNumber { get; init; } = 1;
    public int TotalPages { get; init; }
    public int TotalItems { get; init; }
}
