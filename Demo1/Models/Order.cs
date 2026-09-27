using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo1.Models;

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string OrderCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Note { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public bool StockRestored { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
