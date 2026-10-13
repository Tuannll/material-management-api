# Construction Material Management API 🏗️

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET Build](https://github.com/Tuannll/material-management-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Tuannll/material-management-api/actions/workflows/dotnet.yml)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-blue?logo=csharp)](https://learn.microsoft.com/aspnet/core)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-6C257E?logo=nuget)](https://learn.microsoft.com/ef/core)
[![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

RESTful Web API quản lý danh mục vật tư xây dựng và nhà cung cấp, thiết kế theo tiêu chuẩn Clean Architecture với ASP.NET Core 10 và Entity Framework Core. Dự án cung cấp triển khai song song giữa **LINQ Method Syntax** và **LINQ Query Syntax**, hỗ trợ phân giải động runtime, kết hợp phân trang chuẩn, lọc nâng cao và cơ chế xử lý lỗi tập trung.

---

## 🌟 Tính Năng Nổi Bật

- **Kiến trúc rõ ràng (Separation of Concerns)**: Tách biệt Controller, Service, DTO, Entity và DbContext.
- **Đối chiếu LINQ song song**:
  - `MethodSyntaxMaterialService`: Triển khai qua Fluent extension methods (`Join`, `GroupJoin`, `SelectMany`, `Where`).
  - `QuerySyntaxMaterialService`: Triển khai qua từ khóa declarative query comprehension (`from`, `join ... into`, `where`, `select`).
  - **Dynamic Resolver**: Chuyển đổi cú pháp LINQ tức thời qua query parameter hoặc request header.
  - **Endpoint đối chiếu trực tiếp (`/syntax-comparison`)**: Kiểm tra đồng thời 2 service để chứng minh tính nhất quán của dữ liệu.
- **Phép kết nối dữ liệu nâng cao**:
  - `INNER JOIN`: Chỉ lấy vật tư có nhà cung cấp hợp lệ.
  - `LEFT OUTER JOIN`: Giữ lại toàn bộ vật tư (vật tư chưa gán nhà cung cấp được gán giá trị mặc định `"Chưa có nhà cung cấp"`).
- **Phân trang & Lọc an toàn**:
  - `PagingDto<T>`: Trả về đầy đủ `PageIndex`, `PageSize`, `TotalPages`, `TotalItems`, `HasPreviousPage`, `HasNextPage`.
  - `FilterMaterialDto`: Tự động trim từ khóa tìm kiếm, kiểm tra logic khoảng giá hợp lệ (`MinPrice <= MaxPrice`, không âm) qua `IValidatableObject`.
- **Xử lý lỗi tập trung (Global Exception Handling)**:
  - Bắt lỗi qua middleware, chuẩn hóa mọi phản hồi theo cấu trúc `ApiResponse<T>`.
  - Tự động bắt lỗi Data Annotations và trả về mã lỗi 400 có cấu trúc.
- **Cơ sở dữ liệu linh hoạt**:
  - Tự động sử dụng SQL Server nếu có connection string.
  - Tự động fallback về In-Memory Database (`TH4_Db`) nếu không có SQL Server để chạy thử nghiệm ngay lập tức.
  - Khởi tạo dữ liệu mẫu (`SeedData`) 1 lần duy nhất lúc startup thông qua `DbInitializer`.

---

## 🏗️ Cấu Trúc Dự Án

```text
TH4/
├── Controllers/
│   └── MaterialsController.cs         # REST API endpoints, routing, OpenAPI annotations
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core DbContext, Fluent API mapping
│   ├── DbInitializer.cs               # Database seeding logic lúc khởi động
│   └── SeedData.cs                    # Dữ liệu mẫu (4 nhà cung cấp, 10 vật tư)
├── Exceptions/
│   ├── NotFoundException.cs           # Ngoại lệ 404
│   └── UserFriendlyException.cs       # Ngoại lệ 400 kèm business error code
├── Middleware/
│   └── GlobalExceptionMiddleware.cs   # Bắt lỗi toàn cục, trả về JSON camelCase
├── Models/
│   ├── Common/
│   │   ├── ApiResponse.cs             # Chuẩn hóa vỏ phản hồi API
│   │   └── ErrorCodes.cs              # Hằng số mã lỗi nghiệp vụ
│   ├── Dtos/
│   │   ├── CreateMaterialDto.cs       # DTO tạo mới vật tư
│   │   ├── UpdateMaterialDto.cs       # DTO cập nhật vật tư
│   │   ├── MaterialDto.cs             # DTO danh sách vật tư
│   │   ├── MaterialDetailDto.cs       # DTO chi tiết vật tư & nhà cung cấp
│   │   ├── FilterDto.cs               # DTO lọc & phân trang cơ bản
│   │   ├── FilterMaterialDto.cs       # DTO lọc giá & từ khóa
│   │   └── PagingDto.cs               # Generic pagination response metadata
│   └── Entities/
│       ├── Material.cs                # Entity vật tư
│       └── Supplier.cs                # Entity nhà cung cấp
├── Services/
│   ├── Interfaces/
│   │   └── IMaterialService.cs        # Hợp đồng nghiệp vụ vật tư
│   └── Implementations/
│       ├── MethodSyntaxMaterialService.cs # Triển khai LINQ Method Syntax
│       └── QuerySyntaxMaterialService.cs  # Triển khai LINQ Query Syntax
├── GiaiThich-Inner.txt                # Báo cáo lý thuyết chi tiết về phép Inner/Left Join
└── Program.cs                         # Bootstrapper, DI container, Middleware pipeline
```

---

## 🚀 Khởi Chạy Dự Án

### Yêu cầu hệ thống
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) trở lên.
- (Tùy chọn) SQL Server nếu muốn lưu dữ liệu thực tế.

### Cài đặt và chạy

1. **Clone repository:**
   ```bash
   git clone https://github.com/Tuannll/material-management-api.git
   cd material-management-api
   ```

2. **Restore dependencies & Build:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Chạy ứng dụng:**
   ```bash
   dotnet run
   ```

4. **Truy cập Swagger UI:**
   Mở trình duyệt tại: `http://localhost:5018/swagger` hoặc `http://localhost:5000/swagger`.

---

## 📡 Danh Sách API Endpoints

| Method | Endpoint | Mô tả | Mã HTTP |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/materials` | Lấy danh sách vật tư phân trang & lọc | `200`, `400` |
| `GET` | `/api/materials/{id}` | Lấy chi tiết vật tư theo ID | `200`, `404` |
| `GET` | `/api/materials/inner-join` | Lấy danh sách qua phép INNER JOIN | `200` |
| `GET` | `/api/materials/left-join` | Lấy danh sách qua phép LEFT JOIN | `200` |
| `GET` | `/api/materials/syntax-comparison` | Đối chiếu song song kết quả 2 cú pháp LINQ | `200` |
| `POST` | `/api/materials` | Tạo mới vật tư | `201`, `400` |
| `PUT` | `/api/materials/{id}` | Cập nhật thông tin vật tư | `200`, `400`, `404` |
| `DELETE` | `/api/materials/{id}` | Xóa vật tư theo ID | `204`, `404` |

---

## 💡 Hướng Dẫn Kiểm Thử LINQ Chuyên Sâu

### 1. Chuyển đổi động Method Syntax vs Query Syntax
Hệ thống cho phép đổi service trực tiếp trên từng request:
- **Cách 1: Query String**: Thêm `?syntax=query` hoặc `?syntax=method`
  ```http
  GET /api/materials?syntax=query
  ```
- **Cách 2: Request Header**: Truyền header `X-Syntax-Style: query` hoặc `X-Syntax-Style: method`
  ```bash
  curl -X GET "http://localhost:5018/api/materials" -H "X-Syntax-Style: query"
  ```
Trường `sourceImplementation` trong dữ liệu trả về sẽ thể hiện rõ: `"Method Syntax Style"` hoặc `"Query Syntax Style"`.

### 2. So sánh INNER JOIN vs LEFT OUTER JOIN
- **Dữ liệu mẫu**: Gồm 10 vật tư. Trong đó:
  - Vật tư ID 1 đến 8: Có `SupplierId` hợp lệ (1, 2, 3, hoặc 4).
  - Vật tư ID 9 (`Ống nhựa PVC`) và ID 10 (`Sơn chống thấm`): Có `SupplierId = null`.
- **Kiểm tra Inner Join**:
  ```http
  GET /api/materials/inner-join
  ```
  -> Trả về **8 bản ghi** (ID 9 và ID 10 bị loại vì `SupplierId` là `null`).
- **Kiểm tra Left Join**:
  ```http
  GET /api/materials/left-join
  ```
  -> Trả về đủ **10 bản ghi** (ID 9 và ID 10 hiển thị `supplierName: "Chưa có nhà cung cấp"`).

---

## 📦 Chuẩn Định Dạng Phản Hồi (API Envelope)

### Phản hồi thành công (Success Response):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Retrieved materials successfully.",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Xi măng PC40",
        "unit": "Tấn",
        "unitPrice": 1500000,
        "supplierName": "Xi măng Xuân Thành",
        "supplierAddress": "Hà Nam",
        "sourceImplementation": "Method Syntax Style"
      }
    ],
    "totalItems": 10,
    "pageIndex": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errors": []
}
```

### Phản hồi lỗi (Error Response):
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Giá nhỏ nhất (minPrice) không được lớn hơn giá lớn nhất (maxPrice).",
  "data": null,
  "errors": [
    "MinPrice",
    "MaxPrice"
  ]
}
```

---

## 📄 License
Phát hành theo giấy phép [MIT License](LICENSE).
