using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

public class SiteSettings
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Tên cửa hàng")]
    public string ShopName { get; set; } = "Khada Moto Shop";

    [Phone, StringLength(30)]
    [Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }

    [StringLength(1000)]
    [Display(Name = "Đường dẫn Facebook")]
    public string? FacebookUrl { get; set; }

    [StringLength(300)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả ở chân trang")]
    public string? FooterDescription { get; set; }

    [StringLength(100)]
    [Display(Name = "Giờ hỗ trợ")]
    public string? SupportHours { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
