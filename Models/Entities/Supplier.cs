namespace TH4.Models.Entities; // Khai báo namespace cho các lớp entity của miền nghiệp vụ.

/* Biểu diễn một bản ghi nhà cung cấp trong mô hình dữ liệu của ứng dụng. */
public class Supplier // Định nghĩa entity nhà cung cấp dùng trong bộ dữ liệu trong bộ nhớ.
{
    public int Id { get; set; } // Lưu mã định danh của nhà cung cấp.

    public string Name { get; set; } = string.Empty; // Lưu tên của nhà cung cấp.

    public string Address { get; set; } = string.Empty; // Lưu địa chỉ của nhà cung cấp.

    public string ContactPhone { get; set; } = string.Empty; // Lưu số điện thoại liên hệ của nhà cung cấp.
}
