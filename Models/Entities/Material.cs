namespace TH4.Models.Entities; // Khai báo namespace cho các lớp entity của miền nghiệp vụ.

/* Biểu diễn một bản ghi vật liệu trong mô hình dữ liệu của ứng dụng. */
public class Material // Định nghĩa entity vật liệu dùng trong bộ dữ liệu trong bộ nhớ.
{
    public int Id { get; set; } // Lưu mã định danh của vật liệu.

    public string Name { get; set; } = string.Empty; // Lưu tên của vật liệu.

    public string Unit { get; set; } = string.Empty; // Lưu đơn vị tính của vật liệu.

    public decimal UnitPrice { get; set; } // Lưu đơn giá của vật liệu.

    public int? SupplierId { get; set; } // Lưu mã nhà cung cấp liên kết nếu có.
}
