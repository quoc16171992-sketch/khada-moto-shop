using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo1.Models;

public class Product : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm."), StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(180)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ gồm chữ thường, số và dấu gạch ngang.")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(300)]
    public string ShortDescription { get; set; } = string.Empty;

    [StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "999999999", ErrorMessage = "Giá bán phải lớn hơn 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CompareAtPrice { get; set; }

    [StringLength(500)]
    public string? ImagePath { get; set; }

    [StringLength(500)]
    public string? SecondaryImagePath { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductGalleryImage> GalleryImages { get; set; } = new List<ProductGalleryImage>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CompareAtPrice.HasValue && CompareAtPrice.Value < BasePrice)
        {
            yield return new ValidationResult("Giá cũ phải lớn hơn hoặc bằng giá bán.", [nameof(CompareAtPrice)]);
        }
    }
}
