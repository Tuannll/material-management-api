namespace TH4.Exceptions; // Khai báo namespace cho các exception tùy chỉnh.

/* Biểu diễn exception khi có lỗi kinh doanh kèm error code để client xử lý. */
public class UserFriendlyException : Exception // Định nghĩa exception tùy chỉnh kế thừa từ lớp Exception cơ sở.
{
    public string ErrorCode { get; set; } // Lưu mã lỗi để client xử lý theo logic cụ thể.

    public UserFriendlyException(string errorCode, string message) : base(message) // Truyền thông báo lỗi và error code.
    {
        ErrorCode = errorCode; // Gán error code để sử dụng sau này.
    }
}
