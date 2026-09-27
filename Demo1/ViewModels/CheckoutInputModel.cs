using System.ComponentModel.DataAnnotations;

namespace Demo1.ViewModels;

public class CheckoutInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên."), StringLength(150)]
    [Display(Name = "Họ và tên")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone(ErrorMessage = "Số điện thoại không hợp lệ."), StringLength(30)]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(200)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng."), StringLength(500)]
    [Display(Name = "Địa chỉ giao hàng")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }
}
