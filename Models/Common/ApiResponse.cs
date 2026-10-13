namespace TH4.Models.Common; // Khai báo namespace cho các model phản hồi dùng chung.

/* Chuẩn hóa cấu trúc phản hồi API cho cả trường hợp thành công và thất bại. */
public class ApiResponse<T> // Định nghĩa lớp bao generic có thể chứa mọi kiểu dữ liệu phản hồi.
{
    public bool Success { get; set; } // Cho biết request có được xử lý thành công hay không.

    public int StatusCode { get; set; } = 200; // Mã trạng thái HTTP tương ứng của kết quả.

    public string Message { get; set; } = string.Empty; // Lưu thông điệp tóm tắt để trả về cho người dùng.

    public T? Data { get; set; } // Chứa dữ liệu phản hồi khi có.

    public List<string> Errors { get; set; } = []; // Chứa một hoặc nhiều thông báo lỗi khi request thất bại.

    /* Tạo object phản hồi thành công với dữ liệu, thông điệp và mã trạng thái tùy chọn. */
    public static ApiResponse<T> SuccessResponse(T data, string message, int statusCode = 200) // Định nghĩa hàm factory cho phản hồi thành công.
    {
        return new ApiResponse<T> // Tạo mới một object bao phản hồi.
        {
            Success = true, // Đánh dấu phản hồi là thành công.
            StatusCode = statusCode, // Gán mã trạng thái HTTP thành công.
            Message = message, // Gán thông điệp thành công được truyền vào.
            Data = data, // Gán dữ liệu phản hồi được truyền vào.
            Errors = [] // Khởi tạo danh sách lỗi rỗng.
        };
    }

    /* Tạo object phản hồi thất bại với thông điệp lỗi và chi tiết lỗi tùy chọn. */
    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null, int statusCode = 400) // Định nghĩa hàm factory cho phản hồi thất bại.
    {
        return new ApiResponse<T> // Tạo mới một object bao phản hồi.
        {
            Success = false, // Đánh dấu phản hồi là thất bại.
            StatusCode = statusCode, // Gán mã trạng thái HTTP thất bại.
            Message = message, // Gán thông điệp thất bại được truyền vào.
            Data = default, // Để dữ liệu rỗng trong phản hồi thất bại.
            Errors = errors ?? [] // Dùng danh sách lỗi được truyền vào hoặc danh sách rỗng nếu null.
        };
    }

    /* Tạo object phản hồi lỗi với error code kèm thông điệp. */
    public static ApiResponse<T> ErrorResponse(string errorCode, string message, int statusCode = 400) // Định nghĩa hàm factory cho phản hồi với error code.
    {
        return new ApiResponse<T> // Tạo mới một object bao phản hồi.
        {
            Success = false, // Đánh dấu phản hồi là thất bại.
            StatusCode = statusCode, // Gán mã trạng thái HTTP thất bại.
            Message = message, // Gán thông điệp lỗi được truyền vào.
            Data = default, // Để dữ liệu rỗng trong phản hồi thất bại.
            Errors = [errorCode] // Gán danh sách lỗi chứa error code.
        };
    }
}
