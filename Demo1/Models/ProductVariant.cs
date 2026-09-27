using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo1.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(20)]
    public string Size { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm.")]
    public int StockQuantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAdjustment { get; set; }

    public bool IsActive { get; set; } = true;

    [NotMapped]
    public decimal EffectivePrice => Product.BasePrice + PriceAdjustment;
}
