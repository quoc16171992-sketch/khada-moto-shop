using System.ComponentModel.DataAnnotations;

namespace Demo1.ViewModels;

public class OrderTrackingInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã đơn hàng."), StringLength(30)]
    [Display(Name = "Mã đơn hàng")]
    public string OrderCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone, StringLength(30)]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;
}
