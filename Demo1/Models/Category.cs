using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên danh mục."), StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(120)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ gồm chữ thường, số và dấu gạch ngang.")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
