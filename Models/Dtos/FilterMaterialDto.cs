using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc; // Nhập attribute bind query string của ASP.NET Core MVC.

namespace TH4.Models.Dtos; // Khai báo namespace cho các DTO của vật liệu.

/* Chứa các điều kiện lọc riêng cho danh sách vật liệu kèm xác thực nghiệp vụ. */
public class FilterMaterialDto : FilterDto, IValidatableObject // Kế thừa phân trang, keyword và cài đặt IValidatableObject.
{
    [FromQuery(Name = "minPrice")] // Bind giá thấp nhất từ query string minPrice.
    [Range(0, double.MaxValue, ErrorMessage = "Giá nhỏ nhất không được âm.")]
    public decimal? MinPrice { get; set; } // Lọc vật liệu có đơn giá từ mức này trở lên.

    [FromQuery(Name = "maxPrice")] // Bind giá cao nhất từ query string maxPrice.
    [Range(0, double.MaxValue, ErrorMessage = "Giá lớn nhất không được âm.")]
    public decimal? MaxPrice { get; set; } // Lọc vật liệu có đơn giá không vượt quá mức này.

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
        {
            yield return new ValidationResult(
                "Giá nhỏ nhất (minPrice) không được lớn hơn giá lớn nhất (maxPrice).",
                [nameof(MinPrice), nameof(MaxPrice)]);
        }
    }
}
