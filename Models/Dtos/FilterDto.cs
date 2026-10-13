using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc; // Nhập attribute bind query string của ASP.NET Core MVC.

namespace TH4.Models.Dtos; // Khai báo namespace cho các DTO dùng chung.

/* Chứa các tham số lọc và phân trang cơ bản nhận từ query string. */
public class FilterDto // Định nghĩa DTO nền cho các màn hình danh sách.
{
    private const int DefaultPageSize = 10; // Số bản ghi mặc định mỗi trang.
    private const int DefaultPageIndex = 1; // Trang mặc định bắt đầu từ 1.

    // KỸ THUẬT VALIDATION: Yêu cầu giá trị nhỏ nhất và lớn nhất, kèm câu thông báo lỗi
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    [FromQuery(Name = "pageSize")] // Bind giá trị từ query string pageSize.
    public int PageSize { get; set; } = DefaultPageSize; // Lưu số bản ghi cần lấy.

    [Range(1, int.MaxValue, ErrorMessage = "Page index must be greater than 0.")]
    [FromQuery(Name = "pageIndex")] // Bind giá trị từ query string pageIndex.
    public int PageIndex { get; set; } = DefaultPageIndex; // Lưu trang hiện tại.

    // KỸ THUẬT TRIM VALUE: Tự động loại bỏ khoảng trắng thừa ở hai đầu chuỗi khi gán giá trị
    [FromQuery(Name = "keyword")] // Bind giá trị từ query string keyword.
    public string? Keyword
    {
        get => field;
        set => field = value?.Trim();
    } // Lưu từ khóa tìm kiếm nếu có.

    public int GetSkipCount() // Tính số bản ghi cần bỏ qua trước khi lấy trang hiện tại.
    {
        return (GetSafePageIndex() - 1) * GetSafePageSize(); // Dùng giá trị đã chuẩn hóa để tránh skip âm.
    }

    public int GetSafePageSize() // Chuẩn hóa PageSize để tránh giá trị không hợp lệ.
    {
        return PageSize > 0 ? PageSize : DefaultPageSize; // Nếu client truyền sai thì dùng mặc định.
    }

    public int GetSafePageIndex() // Chuẩn hóa PageIndex để tránh giá trị không hợp lệ.
    {
        return PageIndex > 0 ? PageIndex : DefaultPageIndex; // Nếu client truyền sai thì dùng trang đầu.
    }
}
