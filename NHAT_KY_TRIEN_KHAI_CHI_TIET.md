# 📖 NHẬT KÝ CHI TIẾT TOÀN BỘ QUÁ TRÌNH TRIỂN KHAI CI/CD (BUỔI 10)

Tài liệu ghi lại toàn bộ công việc, cấu hình và cách xử lý lỗi đã thực hiện từ đầu đến cuối cho hệ thống CI/CD với Jenkins + Docker Registry + GitHub Deploy Key.

---

## 🏛️ I. TỔNG QUAN HỆ THỐNG ĐÃ DỰNG

Hệ thống được tách biệt trên **2 máy** đúng chuẩn mô hình thực tế của doanh nghiệp:

```
[ MÁY A: Windows Host (CI) - IP: 192.168.61.1 ]
   │
   ├── 1. Source Code: GitHub Private Repository (material-management-api)
   ├── 2. Jenkins Server (Port 8080): Kéo code qua SSH Deploy Key, chạy test, build image
   └── 3. Docker Registry (Port 5000, UI 8081): Kho lưu trữ Docker image nội bộ
   │
   │ (Kích hoạt HTTP POST /deploy qua mạng LAN)
   ▼
[ MÁY B: Ubuntu VM (CD) - IP: 192.168.61.128 ]
   │
   ├── 1. Webhook Listener (Port 9000): Script Python lắng nghe yêu cầu deploy từ Jenkins
   ├── 2. Docker Container Web API (Port 5001): Triển khai ứng dụng từ image kéo về
   └── 3. SQL Server 2022 (Port 1433): Cơ sở dữ liệu, tự động chạy Migration khi API khởi động
```

---

## 🛠️ II. CHI TIẾT CÁC BƯỚC ĐÃ THỰC HIỆN

### GIAI ĐOẠN 1: Bổ sung code cho `material-management-api`
Repo ban đầu của nhóm chỉ có code Web API thuần, ta đã bổ sung 4 thành phần bắt buộc:

1. **Tạo Project Unit Test (`TH4.Tests`)**:
   - Sử dụng xUnit + EF Core InMemory.
   - Viết 5 bài test logic nghiệp vụ cho `MethodSyntaxMaterialService` (`Create`, `GetById`, `GetAll` kèm keyword, `GetInnerJoin`).
   - Đã add project vào file `TH4.sln`.
   - **Mục đích**: Chặn pipeline ngay lập tức nếu code làm hỏng nghiệp vụ.

2. **Viết `Dockerfile` Multi-Stage cho .NET 10**:
   - Stage 1 (Build): Dùng `mcr.microsoft.com/dotnet/sdk:10.0` restore và build Release.
   - Stage 2 (Publish): Publish ra thư mục `/app/publish`.
   - Stage 3 (Runtime): Dùng base nhẹ `mcr.microsoft.com/dotnet/aspnet:10.0`, expose port `8080`.
   - Kèm file `.dockerignore` loại bỏ thư mục `bin/`, `obj/`, `TH4.Tests/` để image nhẹ và build nhanh.

3. **Cập nhật `Program.cs` hỗ trợ Docker & Database Migration**:
   - Thêm `sqlOptions.EnableRetryOnFailure()`: Giúp Web API tự thử kết nối lại 10 lần nếu SQL Server container khởi động chậm hơn.
   - Đổi cơ chế khởi tạo database:
     ```csharp
     if (dbContext.Database.IsRelational())
     {
         dbContext.Database.Migrate(); // Tự động chạy Migration vào SQL Server thật
     }
     else
     {
         dbContext.Database.EnsureCreated(); // Dùng InMemory khi test
     }
     ```
   - Cho phép Swagger UI chạy cả trong môi trường container/production (`EnableSwagger = true`).

4. **Sửa lỗi Seed Data (`DbInitializer.cs`)**:
   - Khi chạy với SQL Server thật, SQL Server chặn việc chèn Id thủ công (`Id = 1, 2, 3...`) vì bảng có cột Identity tự tăng (Error 544).
   - Đã sửa: Bật `SET IDENTITY_INSERT Suppliers ON;` và `SET IDENTITY_INSERT Materials ON;` trong transaction trước khi nạp seed data.

5. **Viết `Jenkinsfile` (Pipeline as Code)**:
   - Gồm đủ 5 stages chuẩn Declarative Pipeline:
     1. `Checkout`: Kéo code private từ GitHub bằng SSH Deploy Key.
     2. `Test`: Dùng container `mcr.microsoft.com/dotnet/sdk:10.0` chạy `dotnet test TH4.sln`. Nếu test fail → dừng pipeline ngay lập tức.
     3. `Build Image`: Build Docker image và gắn 2 tag: `v${BUILD_NUMBER}` (ví dụ `v7`) và `latest`.
     4. `Push Image`: Login bảo mật bằng `withCredentials` và đẩy cả 2 tag lên Registry máy A (`192.168.61.1:5000`).
     5. `Trigger Deploy`: Dùng lệnh `curl` gọi sang máy B (`http://192.168.61.128:9000/deploy`) gửi kèm tag vừa build để báo máy B cập nhật.

---

### GIAI ĐOẠN 2: Thiết lập Máy A (Windows 11 Host - Máy CI)

1. **Cấu hình Docker Desktop Insecure Registry**:
   - Docker mặc định chỉ cho phép push/pull qua HTTPS. Vì Registry nội bộ chạy HTTP (port 5000), ta đã thêm vào `Settings` → `Docker Engine`:
     ```json
     {
       "insecure-registries": ["192.168.61.1:5000", "localhost:5000"]
     }
     ```
2. **Khởi chạy Docker Registry nội bộ**:
   - Vào thư mục `docker-registry`, tạo tài khoản `admin` / `admin123` bằng `add-user.bat`.
   - Khởi chạy bằng `docker compose up -d` (mở port 5000 cho Registry, port 8081 cho Registry UI).
3. **Khởi chạy Jenkins Container**:
   - Vào thư mục `jenkins-docker-setup`, chạy `docker compose up -d --build`.
   - Jenkins chạy trên port 8080 (đã tích hợp sẵn JCasC tự cấu hình user `admin` / `admin123`).
   - Container Jenkins đã được mount file `/var/run/docker.sock` để gọi Docker Engine của máy Windows.
4. **Tạo SSH Deploy Key**:
   - Tạo cặp khóa RSA PEM 4096-bit:
     - Public Key: Dán vào GitHub repo `Settings` → `Deploy keys` (để Read-Only).
     - Private Key: Tạo Credential trên Jenkins (ID: `github-deploy-key`).
5. **Thêm Credentials và Cấu hình Jenkins**:
   - Thêm Credential `registry-credentials` (Username with password: `admin` / `admin123`).
   - Vào **Manage Jenkins** → **Security** → sửa **Host Key Verification Strategy** thành `Accept first connection` để Jenkins tự động nhận diện `github.com`.
   - Tạo Pipeline Job `material-management-api` trỏ về repo GitHub qua SSH URL (`git@github.com:Tuannll/material-management-api.git`).

---

### GIAI ĐOẠN 3: Thiết lập Máy B (Ubuntu VM - Máy CD)

1. **Cấu hình mạng LAN / Host-Only**:
   - Máy ảo nhận IP `192.168.61.128` (thông mạng 0ms với máy Windows `192.168.61.1`).
2. **Cấu hình Docker Insecure Registry trên Ubuntu**:
   - Thêm vào file `/etc/docker/daemon.json`:
     ```json
     {
       "insecure-registries": ["192.168.61.1:5000"]
     }
     ```
   - Khởi động lại Docker daemon (`sudo systemctl restart docker`).
   - Đăng nhập xác thực với Registry:
     ```bash
     sudo docker login 192.168.61.1:5000 -u admin -p admin123
     ```
3. **Tạo thư mục triển khai `~/cd-deploy`**:
   - **`docker-compose.yml`**:
     - Service `sqlserver`: Container SQL Server 2022 (port 1433).
     - Service `api`: Container Web API (port 5001:8080), kết nối sang `sqlserver`.
   - **`webhook_listener.py`**:
     - Script Python chuẩn (chỉ dùng thư viện có sẵn `http.server`, không cần pip install gì thêm).
     - Lắng nghe port 9000, nhận request `POST /deploy` từ Jenkins.
     - Khi nhận được tag mới: tự động chạy `docker pull` image đúng tag đó → gắn tag `latest` → chạy `docker compose up -d --force-recreate api` để thay thế container cũ bằng container mới.

---

## ⚠️ III. CÁC LỖI ĐÃ GẶP VÀ CÁCH XỬ LÝ (RẤT QUAN TRỌNG ĐỂ TRẢ LỜI THẦY CÔ)

1. **Lỗi 1: `Host key verification failed for github.com` (Build #1, #2)**
   - *Nguyên nhân*: Container Jenkins lần đầu kết nối GitHub chưa có fingerprint của `github.com` trong file `known_hosts`.
   - *Cách xử lý*: Vào `Manage Jenkins` → `Security` → đổi `Git Host Key Verification` thành `Accept first connection`.

2. **Lỗi 2: `error in libcrypto` khi đọc SSH Key (Build #3, #4)**
   - *Nguyên nhân*: Khóa SSH ban đầu sinh bằng thuật toán `ed25519` kiểu mới của OpenSSH không tương thích với thư viện libcrypto trong plugin Git của Jenkins.
   - *Cách xử lý*: Chuyển sang sinh khóa `RSA` với định dạng truyền thống PEM (`ssh-keygen -t rsa -b 4096 -m PEM`).

3. **Lỗi 3: `pull access denied: no basic auth credentials` (Build #5)**
   - *Nguyên nhân*: Máy ảo Ubuntu chưa đăng nhập vào Docker Registry nội bộ của máy A.
   - *Cách xử lý*: Chạy lệnh `docker login 192.168.61.1:5000 -u admin -p admin123` trên máy ảo.

4. **Lỗi 4: `Error 544 IDENTITY_INSERT is set to OFF` (Build #6)**
   - *Nguyên nhân*: File `SeedData.cs` có gán sẵn Id (`Id = 1, 2, 3...`) cho Supplier và Material. SQL Server mặc định không cho chèn giá trị trực tiếp vào cột tự tăng Identity.
   - *Cách xử lý*: Cập nhật `DbInitializer.cs` chạy lệnh SQL `SET IDENTITY_INSERT ... ON;` trước khi SaveChanges.

5. **Lỗi 5: `podman.sock: connect: no such file or directory` (Build #8)**
   - *Nguyên nhân*: Khi máy ảo khởi động lại, phiên user thường bị cấu hình trỏ nhầm Docker socket sang podman rootless.
   - *Cách xử lý*: Chạy script listener với quyền root: `sudo python3 webhook_listener.py`.

---

## 🏆 IV. KẾT QUẢ CUỐI CÙNG ĐÃ NGHIỆM THU

1. **Jenkins Pipeline**: Chạy thành công toàn bộ 5 stage xanh lét (Build #7, #9).
2. **Docker Registry**: Lưu trữ các phiên bản image phân biệt rõ ràng (`v6`, `v7`, `v8`, `v9` và `latest`).
3. **CD Server (Máy B)**: Tự động bắt webhook từ Jenkins, pull image và deploy không cần can thiệp thủ công.
4. **Ứng dụng Production**:
   - Swagger UI: `http://192.168.61.128:5001/swagger/index.html` chạy mượt mà.
   - API `http://192.168.61.128:5001/api/Materials` trả về `200 OK` với đầy đủ 10 vật tư.
   - Database SQL Server: Tự động chạy migration bảng `__EFMigrationsHistory` thành công.
