using Demo1.Models;

namespace Demo1.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Banner> Banners { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public IReadOnlyList<Product> FeaturedProducts { get; init; } = [];
    public IReadOnlyList<Product> NewProducts { get; init; } = [];
    public SiteSettings Settings { get; init; } = new();
}
