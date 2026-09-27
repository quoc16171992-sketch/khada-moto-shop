using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

public class Banner
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề."), StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Subtitle { get; set; }

    [StringLength(500)]
    public string? ImagePath { get; set; }

    [StringLength(50)]
    public string? ButtonText { get; set; }

    [StringLength(1000)]
    public string? TargetUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm.")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
