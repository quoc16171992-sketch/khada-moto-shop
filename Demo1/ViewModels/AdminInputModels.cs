using System.ComponentModel.DataAnnotations;
namespace Demo1.ViewModels;
public interface IProductAdminPageModel
{
    ProductInputModel Input { get; }
    IReadOnlyList<Demo1.Models.Category> Categories { get; }
    IFormFile? ProductImage { get; }
    IFormFile? ProductSecondaryImage { get; }
    IFormFile[] ProductGalleryImages { get; }
}
public class BannerInputModel : IValidatableObject
{
    [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
    [StringLength(500)] public string? Subtitle { get; set; }
    [StringLength(50)] public string? ButtonText { get; set; }
    [StringLength(1000)] public string? TargetUrl { get; set; }
    [Range(0, int.MaxValue)] public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    [Display(Name = "Bắt đầu (giờ địa phương)")] public DateTime? StartAtLocal { get; set; }
    [Display(Name = "Kết thúc (giờ địa phương)")] public DateTime? EndAtLocal { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) { if (StartAtLocal.HasValue && EndAtLocal.HasValue && EndAtLocal <= StartAtLocal) yield return new ValidationResult("Thời gian kết thúc phải sau thời gian bắt đầu.", [nameof(EndAtLocal)]); }
}
public class CategoryInputModel
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(120), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] public string Slug { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
public class ProductInputModel : IValidatableObject
{
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(180), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] public string Slug { get; set; } = string.Empty;
    [StringLength(300)] public string ShortDescription { get; set; } = string.Empty;
    [StringLength(5000)] public string Description { get; set; } = string.Empty;
    [Range(typeof(decimal), "1", "999999999")] public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) { if (CompareAtPrice.HasValue && CompareAtPrice < BasePrice) yield return new ValidationResult("Giá cũ phải lớn hơn hoặc bằng giá bán.", [nameof(CompareAtPrice)]); }
}
public class VariantInputModel
{
    [Required, StringLength(20)] public string Size { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Color { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Sku { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int StockQuantity { get; set; }
    public decimal PriceAdjustment { get; set; }
    public bool IsActive { get; set; } = true;
}
public class SiteSettingsInputModel
{
    [Required, StringLength(100)] public string ShopName { get; set; } = "Khada Moto Shop";
    [Phone, StringLength(30)] public string? PhoneNumber { get; set; }
    [StringLength(1000)] public string? FacebookUrl { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    [EmailAddress, StringLength(200)] public string? Email { get; set; }
    [StringLength(500)] public string? FooterDescription { get; set; }
    [StringLength(100)] public string? SupportHours { get; set; }
}
