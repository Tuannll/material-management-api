namespace TH4.Models.Common; // Khai báo namespace cho các hằng số ứng dụng.

/* Định nghĩa các error code dùng trong ứng dụng. */
public static class ErrorCodes // Định nghĩa các mã lỗi chuẩn hóa để client xử lý.
{
    public const string MaterialNotFound = "MATERIAL_NOT_FOUND"; // Khi không tìm thấy vật liệu theo id.
    public const string DuplicateMaterialName = "DUPLICATE_MATERIAL_NAME"; // Khi tên vật liệu bị trùng lặp trong hệ thống.
    public const string SupplierNotFound = "SUPPLIER_NOT_FOUND"; // Khi nhà cung cấp liên kết không tồn tại.
    public const string InvalidInput = "INVALID_INPUT"; // Khi dữ liệu đầu vào không hợp lệ.
    public const string InternalError = "INTERNAL_ERROR"; // Khi có lỗi hệ thống không mong muốn.
}
