using System.ComponentModel.DataAnnotations;

namespace TH4.Models.Dtos;

public class UpdateMaterialDto
{
    [Required(ErrorMessage = "Tên vật liệu không được để trống.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên vật liệu phải từ 1 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đơn vị tính không được để trống.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Đơn vị tính phải từ 1 đến 50 ký tự.")]
    public string Unit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đơn giá không được để trống.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal UnitPrice { get; set; }

    public int? SupplierId { get; set; }
}
