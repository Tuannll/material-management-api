namespace TH4.Exceptions; // Khai báo namespace cho các exception tùy chỉnh.

/* Biểu diễn exception riêng của ứng dụng khi không tìm thấy tài nguyên. */
public class NotFoundException : Exception // Định nghĩa exception tùy chỉnh kế thừa từ lớp Exception cơ sở.
{
    public NotFoundException(string message) : base(message)  // Truyền thông báo lỗi vào constructor của lớp cha.
    {
    } // Để trống thân constructor vì việc khởi tạo ở lớp cha là đủ.
}
