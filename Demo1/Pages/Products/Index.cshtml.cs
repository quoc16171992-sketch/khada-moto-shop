using Demo1.Data;
using Demo1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Products;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    private const int PageSize = 12;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Size { get; set; }
    [BindProperty(SupportsGet = true)] public string? Color { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? MinPrice { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? MaxPrice { get; set; }
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "newest";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public ProductListViewModel Result { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        PageNumber = Math.Max(1, PageNumber);
        var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants)
            .Where(x => x.IsActive && x.Category.IsActive);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{term}%"));
        }
        if (!string.IsNullOrWhiteSpace(Category)) query = query.Where(x => x.Category.Slug == Category);
        if (!string.IsNullOrWhiteSpace(Size)) query = query.Where(x => x.Variants.Any(v => v.IsActive && v.Size == Size));
        if (!string.IsNullOrWhiteSpace(Color)) query = query.Where(x => x.Variants.Any(v => v.IsActive && v.Color == Color));
        if (MinPrice.HasValue) query = query.Where(x => x.BasePrice >= MinPrice.Value);
        if (MaxPrice.HasValue) query = query.Where(x => x.BasePrice <= MaxPrice.Value);

        query = Sort switch
        {
            "price-asc" => query.OrderBy(x => x.BasePrice),
            "price-desc" => query.OrderByDescending(x => x.BasePrice),
            _ => query.OrderByDescending(x => x.CreatedAtUtc)
        };

        var total = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (totalPages > 0) PageNumber = Math.Min(PageNumber, totalPages);

        Result = new ProductListViewModel
        {
            Products = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken),
            Categories = await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken),
            Sizes = await db.ProductVariants.AsNoTracking().Where(x => x.IsActive && x.Product.IsActive).Select(x => x.Size).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
            Colors = await db.ProductVariants.AsNoTracking().Where(x => x.IsActive && x.Product.IsActive).Select(x => x.Color).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
            PageNumber = PageNumber,
            TotalPages = totalPages,
            TotalItems = total
        };
    }
}
