# 🎤 LỜI THOẠI & NỘI DUNG GIỚI THIỆU DỰ ÁN TRƯỚC KHI DEMO (1 - 2 PHÚT)

Tài liệu dùng để nói mở đầu trước khi bước vào phần demo kỹ thuật CI/CD. Đầy đủ từ bối cảnh, nghiệp vụ, kiến trúc đến lý do chọn dự án.

---

## 🗣️ KỊCH BẢN NÓI MẪU (DÀNH CHO NGƯỜI THUYẾT TRÌNH)

> *"Kính thưa thầy/cô và các bạn, trước khi đi vào demo luồng tự động hóa CI/CD trên 2 máy, nhóm xin phép giới thiệu sơ lược về dự án backend được nhóm sử dụng làm sản phẩm triển khai.*
>
> *Dự án của nhóm có tên là **Construction Material Management API (Hệ thống Web API Quản lý Vật tư Xây dựng)**, được phát triển trên nền tảng **ASP.NET Core .NET 10** và **Entity Framework Core 10**, sử dụng cơ sở dữ liệu **Microsoft SQL Server 2022**.*
>
> *Về mặt nghiệp vụ: Hệ thống quản lý danh mục các loại vật tư (như xi măng, sắt thép, gạch, cát đá) và thông tin các nhà cung cấp liên kết. Dự án hỗ trợ đầy đủ các thao tác CRUD, phân trang dữ liệu, tìm kiếm theo từ khóa và lọc khoảng giá.*
>
> *Về mặt kỹ thuật backend: Điểm đặc biệt của dự án là việc so sánh chuyên sâu hai phong cách viết truy vấn: **LINQ Method Syntax** và **LINQ Query Syntax**, cho phép hoán đổi thuật toán xử lý linh hoạt tại runtime thông qua Header hoặc Query string, đồng thời thực thi các phép **INNER JOIN** và **LEFT OUTER JOIN** để phân loại vật tư đã có hoặc chưa có nhà cung cấp.*
>
> *Để đáp ứng đúng tiêu chí đề bài Buổi 10 — 'phải là một ứng dụng backend thực tế có database, không phải Hello World', nhóm đã đóng gói Web API này vào môi trường container, bổ sung bộ **Unit Test xUnit** kiểm thử nghiệp vụ, cấu hình **Database Auto-Migration**, và thiết lập toàn bộ quy trình **CI/CD tự động qua Jenkins** đưa sản phẩm từ GitHub lên máy chủ triển khai.*
>
> *Sau đây, nhóm xin bắt đầu phần demo chi tiết luồng CI/CD giữa 2 máy."*

---

## 📊 TÓM TẮT THÔNG TIN DỰ ÁN ĐỂ CHIẾU SLIDE / BÁO CÁO

### 1. Thông tin chung
- **Tên dự án**: Construction Material Management API (`material-management-api`)
- **Nền tảng**: .NET 10.0 (ASP.NET Core Web API)
- **ORM / Database**: Entity Framework Core 10.0 + SQL Server 2022
- **Tài liệu API**: OpenAPI / Swagger UI tích hợp sẵn

### 2. Kiến trúc mã nguồn (Clean Architecture)
```text
material-management-api/
├── Controllers/         # Endpoint tiếp nhận request (/api/Materials)
├── Services/            # Tầng xử lý nghiệp vụ (MethodSyntax & QuerySyntax)
├── Models/
│   ├── Entities/        # Bảng dữ liệu: Material, Supplier
│   ├── Dtos/            # DTO truyền nhận dữ liệu, Validation, Phân trang
│   └── Common/          # Định dạng response ApiResponse<T>, ErrorCodes
├── Data/                # ApplicationDbContext, DbInitializer, SeedData
├── Middleware/          # GlobalExceptionMiddleware xử lý lỗi tập trung
├── Migrations/          # Lịch sử migration cấu trúc database EF Core
└── TH4.Tests/           # Project Unit Test xUnit phục vụ CI Pipeline
```

### 3. Các tính năng nghiệp vụ & kỹ thuật nổi bật
1. **CRUD & Search**: Tạo, đọc, cập nhật, xóa vật tư; tìm kiếm từ khóa, validate giá trị (`MinPrice <= MaxPrice`).
2. **Dual LINQ Execution**:
   - `MethodSyntaxMaterialService`: Triển khai bằng Fluent API (`.Join()`, `.GroupJoin()`, `.Where()`).
   - `QuerySyntaxMaterialService`: Triển khai bằng Query Comprehension (`from ... in ... join ... select`).
   - Hoán đổi linh hoạt qua Header `X-Syntax-Style: method|query` hoặc query param `?syntax=`.
3. **INNER JOIN vs LEFT JOIN**:
   - `INNER JOIN`: Chỉ lấy các vật tư đã được gán cho nhà cung cấp đang hoạt động.
   - `LEFT JOIN`: Lấy tất cả vật tư, vật tư nào chưa có nhà cung cấp thì hiển thị mặc định là `"Chưa có nhà cung cấp"`.
4. **Auto Database Migration**:
   - Khởi động container tự động gọi `dbContext.Database.Migrate()` để cập nhật bảng mà không cần can thiệp thủ công.
5. **Unit Testing (xUnit)**:
   - Kiểm thử độc lập tầng Service bằng InMemory Database, đảm bảo logic luôn đúng trước khi đóng gói Docker image.
