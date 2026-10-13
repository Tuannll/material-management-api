namespace TH4.Models.Dtos; // Khai báo namespace cho các DTO của vật liệu.

/* Biểu diễn thông tin chi tiết của vật liệu, bao gồm dữ liệu liên hệ nhà cung cấp. */
public class MaterialDetailDto // Định nghĩa DTO dùng để trả dữ liệu vật liệu chi tiết về cho client.
{
    public int Id { get; set; } // Lưu mã định danh của vật liệu.

    public string Name { get; set; } = string.Empty; // Lưu tên của vật liệu.

    public string Unit { get; set; } = string.Empty; // Lưu đơn vị tính của vật liệu.

    public decimal UnitPrice { get; set; } // Lưu đơn giá của vật liệu.

    public string SupplierName { get; set; } = string.Empty; // Lưu tên nhà cung cấp hoặc chuỗi thay thế nếu không có.

    public string? SupplierAddress { get; set; } // Lưu địa chỉ nhà cung cấp nếu có.

    public string? SupplierContactPhone { get; set; } // Lưu số điện thoại nhà cung cấp nếu có.

    public string SourceImplementation { get; set; } = string.Empty; // Lưu tên kiểu triển khai service đã tạo ra kết quả này.
}
