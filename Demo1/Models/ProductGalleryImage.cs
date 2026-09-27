using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

public class ProductGalleryImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
}
