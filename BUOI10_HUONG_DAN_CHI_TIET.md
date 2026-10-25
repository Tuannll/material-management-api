# KỊCH BẢN THUYẾT TRÌNH & VẬN HÀNH DEMO CI/CD BUỔI 10 (10 ĐIỂM)
## Dự án: Material Management API (.NET 10 Web API + SQL Server)
### Mô hình phân tán 2 máy: Máy A (Jenkins CI) & Máy B (Docker CD Server)

---

## MỤC LỤC
1. [Tổng Quan Kiến Trúc Đồ Án & Bảng Phân Vai](#tong-quan-kien-truc-do-an--bang-phan-vai)
2. [Phần 1: Chuẩn Bị Trước Giờ G (Pre-Flight Checklist & Setup)](#phan-1-chuan-bi-truoc-gio-g-pre-flight-checklist--setup)
3. [Phần 2: Kịch Bản Thuyết Trình Từng Bước (12 - 15 Phút)](#phan-2-kich-ban-thuyet-trinh-tung-buoc-12---15-phut)
4. [Phần 3: Kịch Bản Demo 1 - Chứng Minh Test Chặn Pipeline (Quality Gate)](#phan-3-kich-ban-demo-1---chung-minh-test-chan-pipeline-quality-gate)
5. [Phần 4: Kịch Bản Demo 2 - Triển Khai Tự Động Từ Code Sang Máy B & Auto Migration](#phan-4-kich-ban-demo-2---trien-khai-tu-dong-tu-code-sang-may-b--auto-migration)
6. [Phần 5: Bảng Câu Hỏi Vấn Đáp Thường Gặp Của Thầy Cô (Q&A 10 Điểm)](#phan-5-bang-cau-hoi-van-dap-thuong-gap-cua-thay-co-qa-10-diem)
7. [Phụ Lục: Cheatsheet Lệnh Nhanh Cho Nhóm Khi Lên Bảng](#phu-luc-cheatsheet-lenh-nhanh-cho-nhom-khi-len-bang)

---

<a id="tong-quan-kien-truc-do-an--bang-phan-vai"></a>
## TỔNG QUAN KIẾN TRÚC ĐỒ ÁN & BẢNG PHÂN VAI

### 1. Kiến trúc phân tán CI/CD 2 máy

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                  GITHUB PRIVATE REPO                                   │
│                     (Source Code + Dockerfile + Jenkinsfile + Tests)                   │
└──────────────────────────────────────────┬─────────────────────────────────────────────┘
                                           │ (Clone qua SSH Deploy Key / PAT)
                                           ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                 MÁY A: JENKINS CI SERVER                               │
│  Stage 1: Checkout      ──> Kéo mã nguồn từ GitHub Private Repo                        │
│  Stage 2: Unit Test     ──> dotnet test (xUnit, 5 test cases) [NẾU FAIL -> DỪNG NGAY]  │
│  Stage 3: Build Image   ──> docker build gắn tag :${BUILD_NUMBER} và :latest           │
│  Stage 4: Push Image    ──> docker login & push lên Registry (ghcr.io / Docker Hub)    │
│  Stage 5: Trigger CD    ──> HTTP POST webhook sang Máy B                               │
└──────────────────────────────────────────┬─────────────────────────────────────────────┘
                                           │
                    HTTP POST http://<IP_MÁY_B>:9000/deploy                              │
                    Body: {"image": "material-management-api", "tag": "15"}               │
                                           │
                                           ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                              MÁY B: CD STAGING / PRODUCTION                            │
│  1. Webhook Receiver    ──> Python HTTP Server (Cổng 9000) đón nhận tag "15"           │
│  2. Docker Compose Pull ──> docker compose pull api (Kéo image :15 từ Registry)        │
│  3. Docker Compose Up   ──> docker compose up -d api (Recreate container api)          │
│  4. Container Startup   ──> EF Core dbContext.Database.Migrate() tự cập nhật DB schema │
│  5. Service Ready       ──> Web API (:8080) kết nối Microsoft SQL Server 2022 (:1433)  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### 2. Bảng phân vai nhóm 3 thành viên

| Thành viên | Phụ trách chính | Nhiệm vụ khi thuyết trình |
|---|---|---|
| **TV1 (Trưởng nhóm)** | Kiến trúc CI/CD, Jenkinsfile, Trình bày Slide | Mở đầu, giới thiệu bài toán, phân tích luồng Declarative Pipeline 5 stages, điều phối vấn đáp |
| **TV2 (Máy A - CI)** | Cấu hình Jenkins, Deploy Key, Unit Test Demo | Thao tác trên Máy A: Chiếu Jenkins Dashboard, sửa test cố tình fail (Demo 1), commit push, giải thích log đỏ |
| **TV3 (Máy B - CD)** | Docker Compose, Webhook Receiver, DB Migration | Thao tác trên Máy B: Chiếu terminal Webhook log, container Docker, gọi API /health, kiểm tra migration DB (Demo 2) |

---

<a id="phan-1-chuan-bi-truoc-gio-g-pre-flight-checklist--setup"></a>
## PHẦN 1: CHUẨN BỊ TRƯỚC GIỜ G (PRE-FLIGHT CHECKLIST & SETUP)

### 1.1. Chuẩn bị mạng và kết nối giữa 2 máy
- [ ] Cả 2 máy (Máy A và Máy B) kết nối chung một mạng Wi-Fi hoặc mạng LAN (hoặc Wi-Fi phát từ điện thoại 4G).
- [ ] Xác định địa chỉ IP của Máy B:
  - **Trên Windows (Máy B)**: Chạy `ipconfig`, tìm `IPv4 Address` của card Wi-Fi/Ethernet (Ví dụ: `192.168.1.50`).
  - **Trên Linux/macOS (Máy B)**: Chạy `hostname -I` hoặc `ip a`.
- [ ] Từ Máy A, mở terminal gõ: `ping <IP_MÁY_B>` (Đảm bảo thông mạng, không bị Firewall Windows chặn ICMP).
  *(Lưu ý: Nếu không ping được, tắt tạm thời Windows Firewall trên Máy B hoặc mở port 9000 và 8080).*

---

### 1.2. Chuẩn bị GitHub Private Repository & Deploy Key / PAT

#### Cách 1: Cấu hình SSH Deploy Key (Khuyến nghị chuẩn Buổi 10)
1. **Trên Máy A (máy cài Jenkins)**, mở PowerShell/Bash tạo cặp SSH Key riêng cho Jenkins (không dùng chung key cá nhân):
   ```bash
   ssh-keygen -t ed25519 -C "jenkins-deploy-key" -f jenkins_deploy_key -N ""
   ```
   Sinh ra 2 file:
   - `jenkins_deploy_key.pub`: Public Key.
   - `jenkins_deploy_key`: Private Key.
2. **Thêm Public Key lên GitHub Repo**:
   - Truy cập: `https://github.com/<User>/material-management-api/settings/keys`.
   - Bấm **Add deploy key**.
   - Title: `Jenkins Deploy Key (Máy A CI)`.
   - Key: Dán toàn bộ nội dung file `jenkins_deploy_key.pub`.
   - **Lưu ý quan trọng**: Để NGUYÊN trạng thái TẮT ô *"Allow write access"*. Giải thích: Jenkins chỉ cần quyền đọc (clone/pull) source code để build, việc tắt quyền ghi tuân thủ nguyên tắc Least Privilege (Đặc quyền tối thiểu) trong bảo mật.
3. **Thêm Private Key vào Jenkins Credentials**:
   - Truy cập Jenkins: `http://localhost:8080` -> **Manage Jenkins** -> **Credentials** -> **System** -> **Global credentials (unrestricted)** -> **Add Credentials**.
   - Kind: `SSH Username with private key`.
   - Scope: `Global`.
   - ID: Nhập chính xác `github-deploy-key` (khớp với khai báo trong `Jenkinsfile`).
   - Description: `GitHub Deploy Key for Private Repo`.
   - Username: `git`.
   - Private Key: Chọn **Enter directly** -> Bấm **Add** -> Dán toàn bộ nội dung file `jenkins_deploy_key`.
   - Bấm **Create**.

#### Cách 2: Cấu hình Personal Access Token (PAT) (Nếu clone qua HTTPS)
- GitHub -> **Settings** -> **Developer settings** -> **Personal access tokens** -> **Tokens (classic)** -> Tạo token với quyền:
  - `repo` (Đọc private repo).
  - `write:packages` và `read:packages` (Nếu dùng GitHub Packages ghcr.io).
- Trên Jenkins: Tạo Credential dạng `Username with password` với ID `github-deploy-key`, username là tài khoản GitHub, password là PAT.

---

### 1.3. Cấu hình Credentials Docker Registry trên Jenkins (Máy A)
1. Truy cập Jenkins -> **Manage Jenkins** -> **Credentials** -> **Global credentials** -> **Add Credentials**.
2. Cấu hình:
   - Kind: `Username with password`.
   - ID: Nhập chính xác `docker-registry-creds` (khớp với `Jenkinsfile`).
   - Description: `Credentials for Docker Registry (GHCR / Docker Hub)`.
   - Username: Tài khoản Docker Hub hoặc GitHub username.
   - Password:
     - Nếu dùng Docker Hub: Access Token tạo từ `hub.docker.com`.
     - Nếu dùng GitHub Container Registry (ghcr.io): GitHub PAT có quyền `write:packages`.
3. Bấm **Create**.

---

### 1.4. Thiết lập Pipeline Job trên Jenkins (Máy A)
1. Trên Jenkins Dashboard -> Bấm **New Item** -> Đặt tên: `material-management-api-pipeline` -> Chọn **Pipeline** -> Bấm **OK**.
2. Tại mục **General**:
   - Tích chọn **This project is parameterized**:
     - Parameter 1 (String): `DOCKER_REGISTRY_PARAM` - Default: `ghcr.io` (hoặc `docker.io`).
     - Parameter 2 (String): `CD_HOST_PARAM` - Default: Điền chính xác `<IP_MÁY_B>` vừa tìm được ở mục 1.1 (Ví dụ: `192.168.1.50`).
     - Parameter 3 (String): `CD_PORT_PARAM` - Default: `9000`.
3. Tại mục **Pipeline**:
   - Definition: Chọn **Pipeline script from SCM**.
   - SCM: Chọn **Git**.
   - Repository URL:
     - Nếu dùng SSH Deploy Key: `git@github.com:<User>/material-management-api.git`
     - Nếu dùng HTTPS PAT: `https://github.com/<User>/material-management-api.git`
   - Credentials: Chọn `github-deploy-key`.
   - Branch Specifier: `*/feat/buoi10-presentation-guide` (hoặc `*/main`).
   - Script Path: `Jenkinsfile`.
4. Bấm **Save**.

---

### 1.5. Thiết lập và khởi chạy Máy B (CD Server)

1. Mở thư mục `deploy/` trên Máy B:
   ```bash
   cd D:/Users/Documents/HUCE/HK1_2026-2027/1. DevOps-Server2/ThuyetTrinh/material-management-api/deploy
   ```
2. Đảm bảo file `.env` tồn tại (sao chép từ `env.example` nếu chưa có):
   ```ini
   SA_PASSWORD=YourStrong@Passw0rd!
   SQL_PORT=1433
   DB_NAME=MaterialDb
   IMAGE_NAME=ghcr.io/<User>/material-management-api
   IMAGE_TAG=latest
   PORT=8080
   WEBHOOK_PORT=9000
   ASPNETCORE_ENVIRONMENT=Production
   ```
3. Khởi động hạ tầng SQL Server:
   ```bash
   docker compose up -d sqlserver
   ```
   *Kiểm tra trạng thái healthy của database:*
   ```bash
   docker compose ps
   # Cột STATUS phải hiển thị: Up ... (healthy)
   ```
4. Khởi động Webhook Receiver:
   - **Trên Windows**: Chạy file `setup_cd_machine.bat` hoặc lệnh:
     ```bash
     python webhook_receiver.py
     ```
   - **Trên Linux / macOS**:
     ```bash
     ./setup_cd_machine.sh
     ```
5. Mở một terminal khác trên Máy B, test thử healthcheck của receiver:
   ```bash
   curl http://localhost:9000/health
   ```
   Kết quả trả về HTTP 200:
   ```json
   {"status": "healthy", "service": "Machine B CD Webhook Receiver", "current_image_tag": "latest"}
   ```

---

<a id="phan-2-kich-ban-thuyet-trinh-tung-buoc-12---15-phut"></a>
## PHẦN 2: KỊCH BẢN THUYẾT TRÌNH TỪNG BƯỚC (12 - 15 PHÚT)

| Thời gian | Người nói | Màn hình chiếu | Hành động & Lời thoại chuẩn mẫu |
|---|---|---|---|
| **00:00 - 02:00** | **TV1** | Slide 1-3: Đề tài & Sơ đồ kiến trúc 2 máy | *"Kính thưa thầy cô và các bạn, nhóm chúng em xin trình bày đồ án Buổi 10: Tự động hóa CI/CD với Jenkins, GitHub Deploy Key và mô hình triển khai phân tán 2 máy. Trong thực tế, máy CI (Build Server) và máy CD (Production Server) luôn là 2 môi trường tách biệt. Nhóm chúng em mô phỏng chính xác mô hình này: Máy A đóng vai trò Jenkins CI, Máy B đóng vai trò CD Staging Server chạy Docker và SQL Server. Sau đây em xin mời bạn TV2 trình bày về luồng CI trên Máy A."* |
| **02:00 - 05:00** | **TV2** | Jenkins Dashboard (Máy A) & Jenkinsfile | *"Trên Máy A, chúng em định nghĩa toàn bộ quy trình qua Declarative Pipeline trong Jenkinsfile gồm 5 stages rõ ràng: 1. Checkout (sử dụng SSH Deploy Key chỉ cấp quyền read-only, bảo vệ Private Repo); 2. Unit Test (Quality Gate ngăn chặn build rác); 3. Build Image (gắn tag động BUILD_NUMBER, không lạm dụng tag latest); 4. Push Image lên Registry qua Credentials Binding; 5. Trigger CD Deploy qua Webhook HTTP POST sang Máy B. Đặc biệt, vì Jenkins chạy trên máy cá nhân không có Public IP, GitHub không thể webhook vào Jenkins, nên bước chạy CI chúng em chủ động bấm 'Build Now', nhưng chiều CI gọi sang CD thì hoàn toàn tự động."* |
| **05:00 - 08:00** | **TV2** | VS Code (sửa test) + Jenkins Console (Máy A) | **THỰC HIỆN DEMO 1 (Quality Gate):** Cố tình sửa test fail -> Commit push -> Jenkins đỏ dừng ngay tại stage Unit Test -> Giải thích chi tiết (Xem Phần 3). |
| **08:00 - 12:00** | **TV2 + TV3** | Jenkins Máy A + Terminal & Browser Máy B | **THỰC HIỆN DEMO 2 (End-to-End Delivery):** Sửa code tính năng mới -> Commit push -> Jenkins xanh 5 stages -> Máy B bắt webhook -> Tự pull image -> Tự apply DB migration -> Gọi API kiểm chứng (Xem Phần 4). |
| **12:00 - 15:00** | **Cả nhóm** | Slide Tổng kết & Màn hình hệ thống | Trả lời câu hỏi vấn đáp của thầy cô (Xem Phần 5). |

---

<a id="phan-3-kich-ban-demo-1---chung-minh-test-chan-pipeline-quality-gate"></a>
## PHẦN 3: KỊCH BẢN DEMO 1 - CHỨNG MINH TEST CHẶN PIPELINE (QUALITY GATE)

### Ý nghĩa kỹ thuật cần nói với giám khảo:
> *"CI thực thụ không phải chỉ là 'build ra được file là xong'. Nếu code mới làm gãy logic nghiệp vụ cũ mà vẫn build và đóng gói Docker image rồi deploy lên server thì sẽ gây ra thảm họa gián đoạn dịch vụ (outage). Vì vậy, Unit Test là một trạm kiểm soát chất lượng (Quality Gate) bắt buộc trong pipeline. Nếu có bất kỳ test case nào fail, Jenkins phải dừng toàn bộ pipeline ngay lập tức, không được phép build hay push Docker image."*

### Các bước thao tác chuẩn:

#### Bước 1: Mở mã nguồn Unit Test trên Máy A
Mở file `TH4.Tests/MaterialServiceTests.cs`. Tìm đến Test case đầu tiên:
```csharp
// Dòng 94 trong TH4.Tests/MaterialServiceTests.cs
// [DEMO FAIL]: Sửa số 2 thành số khác (ví dụ: 999) để test FAIL, minh chứng pipeline CI fail
Assert.Equal(2, result.TotalItems);
```

#### Bước 2: Sửa cố tình cho test fail
Sửa số `2` thành `999`:
```csharp
Assert.Equal(999, result.TotalItems);
```

#### Bước 3: Commit và Push lên GitHub
Mở terminal tại thư mục dự án trên Máy A:
```bash
git add TH4.Tests/MaterialServiceTests.cs
git commit -m "test: simulate broken unit test to verify pipeline quality gate"
git push origin feat/buoi10-presentation-guide
```

#### Bước 4: Kích hoạt Build trên Jenkins (Máy A)
1. Truy cập giao diện Jenkins: `http://localhost:8080/job/material-management-api-pipeline/`.
2. Bấm **Build with Parameters** (hoặc **Build Now**).
3. Bấm nút **Build**.

#### Bước 5: Chiếu kết quả trên màn hình lớn cho thầy cô xem
1. **Quan sát Pipeline Stage View**:
   - `Stage 1: Checkout` -> **Màu Xanh (Success)**.
   - `Stage 2: Unit Test` -> **Màu Đỏ (Failed)** sau khoảng 2-3 giây.
   - `Stage 3: Build Image` -> **Bị bỏ qua (Not Executed / Aborted)**.
   - `Stage 4: Push Image` -> **Bị bỏ qua**.
   - `Stage 5: Trigger CD Deploy` -> **Bị bỏ qua**.
2. **Bấm vào Build vừa fail -> Chọn "Console Output"**:
   - Cuộn đến đoạn log lỗi và chỉ rõ cho thầy cô:
     ```text
     [xUnit.net 00:00:00.65]   Assert.Equal() Failure: Values differ
     [xUnit.net 00:00:00.65]   Expected: 999
     [xUnit.net 00:00:00.65]   Actual:   2
     Failed!  - Failed: 1, Passed: 4, Skipped: 0, Total: 5
     script returned exit code 1
     [FAILURE] THÔNG BÁO: PIPELINE DỪNG LẠI DO XẢY RA LỖI!
     - ĐẶC BIỆT: Nếu lỗi xảy ra tại stage 'Unit Test', toàn bộ pipeline bị dừng ngay lập tức.
     ```
3. **Chiếu sang màn hình Máy B**:
   - Terminal của `webhook_receiver.py` trên Máy B hoàn toàn tĩnh lặng, không hề nhận được bất kỳ HTTP POST nào.
   - Container trên Máy B vẫn đang phục vụ phiên bản an toàn trước đó.

#### Bước 6: Phục hồi lại test đúng
Sửa lại dòng 94 trong `TH4.Tests/MaterialServiceTests.cs` về số `2`:
```csharp
Assert.Equal(2, result.TotalItems);
```
Commit và push:
```bash
git add TH4.Tests/MaterialServiceTests.cs
git commit -m "fix: restore valid unit test assertion"
git push origin feat/buoi10-presentation-guide
```
Bấm **Build Now** lần nữa trên Jenkins -> Stage View chuyển sang **Màu Xanh** toàn bộ!

---

<a id="phan-4-kich-ban-demo-2---trien-khai-tu-dong-tu-code-sang-may-b--auto-migration"></a>
## PHẦN 4: KỊCH BẢN DEMO 2 - TRIỂN KHAI TỰ ĐỘNG TỪ CODE SANG MÁY B & AUTO MIGRATION

### Ý nghĩa kỹ thuật cần nói với giám khảo:
> *"Đây là phần chứng minh quan trọng nhất của đồ án: 'Thêm code mới thì ra sản phẩm mới như thế nào ngay trong buổi thuyết trình'. Chúng em sẽ thêm một endpoint mới và cập nhật version của ứng dụng trong mã nguồn. Sau đó đẩy code lên GitHub, kích hoạt Pipeline. Pipeline sẽ tự động test, tự đóng gói Docker image với tag là BUILD_NUMBER, đẩy lên Registry, rồi bắn Webhook sang Máy B. Máy B sẽ tự kéo image mới về, reload container, tự chạy EF Core Migration cập nhật cơ sở dữ liệu mà không cần can thiệp thủ công bằng tay."*

### Các bước thao tác chuẩn:

#### Bước 1: Mở `Program.cs` và thêm code mới
Mở file `Program.cs`, tìm đến endpoint `/health` (dòng 137). Bổ sung thông tin version và một endpoint mới `/api/version`:
```csharp
// Thay đổi trong Program.cs:
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "2.0.0-demo-buoi10",
    message = "Automated CI/CD Delivery Verified successfully on Machine B!"
}))
.WithName("HealthCheck")
.WithTags("Health")
.AllowAnonymous();

// Endpoint mới thêm trực tiếp trong buổi thuyết trình:
app.MapGet("/api/demo-live", () => Results.Ok(new
{
    demo = "Buoi 10 DevOps HUCE",
    author = "Nhom Thuyet Trinh",
    buildTime = DateTime.UtcNow,
    features = new[] { "Jenkins Pipeline", "SSH Deploy Key", "Docker Compose", "Automated DB Migration" }
}))
.WithName("DemoLiveEndpoint")
.WithTags("Demo");
```

#### Bước 2: Chạy kiểm thử nhanh tại local đảm bảo không vỡ cú pháp
```bash
dotnet test
```
Xác nhận: `Passed! - Failed: 0, Passed: 5, Total: 5`.

#### Bước 3: Commit và Push code mới lên GitHub
```bash
git add Program.cs
git commit -m "feat: add demo-live endpoint and bump version to 2.0.0 for live presentation"
git push origin feat/buoi10-presentation-guide
```

#### Bước 4: Bấm "Build Now" trên Jenkins (Máy A)
1. Mở Jenkins UI -> Bấm **Build with Parameters** -> Kiểm tra:
   - `CD_HOST_PARAM`: `<IP_MÁY_B>`
   - `CD_PORT_PARAM`: `9000`
2. Bấm **Build**.
3. Xem trực tiếp quá trình thực thi:
   - **Stage 1 (Checkout)**: Jenkins dùng SSH key kéo commit mới nhất về.
   - **Stage 2 (Unit Test)**: `dotnet test` chạy đạt 5/5 passed.
   - **Stage 3 (Build Image)**: Docker build image với tag ví dụ `material-management-api:18` (khớp với `${env.BUILD_NUMBER}`).
   - **Stage 4 (Push Image)**: Đăng nhập Registry và push image `:18` lên ghcr.io / Docker Hub.
   - **Stage 5 (Trigger CD Deploy)**: Jenkins gửi lệnh `curl -X POST http://<IP_MÁY_B>:9000/deploy -d '{"image":"material-management-api","tag":"18"}'`.

#### Bước 5: Chiếu màn hình Máy B (CD Server)
Quan sát trực tiếp màn hình Terminal đang chạy `webhook_receiver.py` trên Máy B:
```text
2026-10-25 10:30:15 [INFO] Received deployment request from 192.168.1.20
2026-10-25 10:30:15 [INFO] Target Image Tag: 18
2026-10-25 10:30:15 [INFO] Updated deploy/.env with IMAGE_TAG=18
2026-10-25 10:30:15 [INFO] Executing: docker compose pull api
Pulling api ... 100% Downloaded newer image for ghcr.io/.../material-management-api:18
2026-10-25 10:30:20 [INFO] Executing: docker compose up -d api
Recreating material-api ... done
2026-10-25 10:30:22 [INFO] Deployment completed successfully for tag 18!
```

#### Bước 6: Kiểm tra log Container chứng minh Auto Migration trên Máy B
Mở một cửa sổ terminal mới trên Máy B, chạy:
```bash
docker logs material-api --tail 50
```
Chỉ rõ cho thầy cô dòng log Entity Framework Core khởi động:
```text
info: Program[0]
      Relational database provider detected (Microsoft.EntityFrameworkCore.SqlServer). Running database migrations...
info: Microsoft.EntityFrameworkCore.Migrations[20402]
      Applying migration '20261025_InitialCreate'...
info: Program[0]
      Database migrations applied successfully.
info: Program[0]
      Database seeded successfully.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:8080
```

#### Bước 7: Kiểm chứng API thực tế ngay trên trình duyệt / Postman
1. **Kiểm tra endpoint `/health`**:
   Truy cập trên trình duyệt hoặc chạy curl:
   ```bash
   curl http://<IP_MÁY_B>:8080/health
   ```
   Kết quả trả về JSON mới:
   ```json
   {
     "status": "healthy",
     "timestamp": "2026-10-25T10:31:00Z",
     "version": "2.0.0-demo-buoi10",
     "message": "Automated CI/CD Delivery Verified successfully on Machine B!"
   }
   ```
2. **Kiểm tra endpoint mới thêm trong lúc thuyết trình `/api/demo-live`**:
   ```bash
   curl http://<IP_MÁY_B>:8080/api/demo-live
   ```
   Kết quả trả về chính xác đoạn code vừa push cách đó 2 phút:
   ```json
   {
     "demo": "Buoi 10 DevOps HUCE",
     "author": "Nhom Thuyet Trinh",
     "features": [
       "Jenkins Pipeline",
       "SSH Deploy Key",
       "Docker Compose",
       "Automated DB Migration"
     ]
   }
   ```
3. **Mở Swagger UI trên Máy B**:
   Mở trình duyệt: `http://<IP_MÁY_B>:8080/swagger` -> Hiển thị đầy đủ giao diện tương tác API kết nối dữ liệu thật từ SQL Server!

---

<a id="phan-5-bang-cau-hoi-van-dap-thuong-gap-cua-thay-co-qa-10-diem"></a>
## PHẦN 5: BẢNG CÂU HỎI VẤN ĐÁP THƯỜNG GẶP CỦA THẦY CÔ (Q&A 10 ĐIỂM)

### Câu 1: Tại sao nhóm phải dùng SSH Deploy Key thay vì dùng username/password của GitHub? Tại sao phải tắt "Allow write access"?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, việc sử dụng SSH Deploy Key mang lại 3 ưu điểm vượt trội về bảo mật:
  1. **Tách biệt danh tính**: Deploy Key được sinh riêng cho repository `material-management-api`, không liên kết với tài khoản cá nhân của sinh viên. Nếu máy Jenkins bị xâm nhập, kẻ tấn công cũng chỉ truy cập được duy nhất repo này chứ không thể truy cập các repository cá nhân khác của thành viên.
  2. **Nguyên tắc Đặc quyền tối thiểu (Least Privilege)**: Jenkins trong bài toán này chỉ đóng vai trò Build Agent, nó chỉ cần quyền đọc (clone/pull) mã nguồn để build chứ không bao giờ cần push code ngược lại GitHub. Do đó chúng em tắt hoàn toàn 'Allow write access' để ngăn chặn rủi ro ghi đè hoặc phá hoại mã nguồn.
  3. **Xác thực phi mật khẩu (Passwordless)**: Sử dụng thuật toán bất đối xứng Ed25519 hiện đại, an toàn hơn nhiều so với việc lưu trữ mật khẩu plaintext."*

---

### Câu 2: Tại sao trên máy cá nhân Jenkins không thể tự động nhận Webhook từ GitHub khi có commit mới mà phải bấm "Build Now" thủ công?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, GitHub là một dịch vụ trên Internet công cộng (Public Cloud), trong khi máy cá nhân của chúng em chạy trong mạng nội bộ (NAT / Private IP như `192.168.x.x`). Các router mạng không mở port và không có IP tĩnh công khai nên GitHub không có cách nào gửi HTTP request ngược vào trong máy Jenkins cá nhân được.
  Trong môi trường doanh nghiệp thực tế, server Jenkins sẽ được gán IP tĩnh công khai (Public IP) hoặc domain có bản ghi DNS (ví dụ: `jenkins.company.com`). Khi đó, GitHub Webhook sẽ tự động kích hoạt pipeline mỗi khi có sự kiện Push hoặc Pull Request. Còn trong bài thực hành mô phỏng này, việc bấm 'Build Now' chỉ là thao tác kích hoạt đầu vào, toàn bộ luồng CI sang CD phía sau vẫn diễn ra hoàn toàn tự động."*

---

### Câu 3: Tại sao không được dùng mãi tag `latest` khi build và deploy Docker Image?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, lạm dụng tag `latest` là một anti-pattern nghiêm trọng trong DevOps vì 3 lý do:
  1. **Mất tính truy vết (Lack of Traceability)**: Khi hệ thống gặp sự cố trên Máy B, nếu chỉ dùng tag `latest`, chúng ta không thể biết container đang chạy từ commit nào hay bản build số mấy. Với tag `${BUILD_NUMBER}`, chúng em truy vết chính xác từ image về bản build tương ứng trên Jenkins và commit trên Git.
  2. **Không thể Rollback an toàn**: Nếu phiên bản mới bị lỗi runtime, với tag số chúng em chỉ cần gửi webhook yêu cầu kéo lại tag cũ (ví dụ: tag 17) là hệ thống phục hồi ngay lập tức. Nếu dùng `latest`, image mới đã ghi đè image cũ trên Registry, không thể rollback được.
  3. **Lỗi Docker Cache**: Một số Docker daemon sẽ không kéo lại image nếu trong máy đã có sẵn image tên `latest`, dẫn đến việc deploy code mới nhưng máy chủ vẫn chạy code cũ."*

---

### Câu 4: Tại sao Máy B không clone mã nguồn về rồi chạy `docker build` tại chỗ mà lại phải nhận image từ Registry?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, việc này tuân thủ nguyên lý vàng của CI/CD: **'Build Once, Deploy Anywhere' (Đóng gói một lần, triển khai mọi nơi)**:
  1. **Đảm bảo tính nhất quán (Immutability)**: Image đã được build và vượt qua Unit Test trên Máy A là một khối bất biến. Nếu sang Máy B lại build lại từ source code, môi trường build của Máy B (phiên bản SDK, compiler, thư viện) có thể khác Máy A, dẫn đến hiện tượng 'chạy được trên máy CI nhưng lỗi trên máy CD'.
  2. **Tối ưu tài nguyên Server Production**: Quá trình build và restore thư viện tốn rất nhiều CPU và RAM. Máy B chỉ đóng vai trò máy chủ ứng dụng nên chỉ cần kéo artifact (Docker image) về chạy, giúp tiết kiệm tài nguyên và rút ngắn thời gian deploy chỉ còn vài giây."*

---

### Câu 5: Quá trình Database Migration diễn ra như thế nào trên Máy B? Làm sao để tránh mất dữ liệu cũ của khách hàng?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, trong dự án của chúng em:
  1. Dữ liệu của SQL Server được lưu trữ trong một Docker Named Volume (`sqlserver_data`) được định nghĩa trong `docker-compose.yml`. Khi container `api` bị restart hoặc nâng cấp image mới, container `sqlserver` và volume dữ liệu hoàn toàn không bị ảnh hưởng, dữ liệu khách hàng được bảo toàn 100%.
  2. Khi container `api` phiên bản mới khởi động, trong hàm `Program.cs`, chúng em sử dụng lệnh `dbContext.Database.Migrate()`. Entity Framework Core sẽ tự động kiểm tra bảng lịch sử `__EFMigrationsHistory` trong cơ sở dữ liệu. Nó chỉ áp dụng những file Migration mới chưa được chạy, đảm bảo schema luôn đồng bộ với mã nguồn mà không làm mất dữ liệu hiện có."*

---

### Câu 6: Trong `Jenkinsfile`, thông tin nhạy cảm (Credentials) như Token Docker hay SSH Key được bảo vệ như thế nào?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, chúng em tuân thủ nghiêm ngặt chuẩn bảo mật của Jenkins:
  1. Tuyệt đối không hard-code bất kỳ mật khẩu hay token nào vào `Jenkinsfile` hoặc commit lên Git.
  2. Toàn bộ thông tin được lưu trong kho bí mật **Jenkins Credentials Store** với các ID `github-deploy-key` và `docker-registry-creds`.
  3. Trong pipeline, chúng em sử dụng plugin `withCredentials([usernamePassword(...)])`. Cơ chế Credentials Binding của Jenkins sẽ tự động mã hóa (masking) các biến mật khẩu, khi in log console output nó sẽ chỉ hiện thị ký tự `****` để chống lộ lọt thông tin trong trường hợp log bị chia sẻ."*

---

### Câu 7: Nếu Máy B bị mất mạng hoặc tắt nguồn đúng lúc Jenkins gửi Webhook thì sao?
- **Trả lời mẫu xuất sắc**:
  *"Dạ thưa thầy/cô, trong kịch bản hiện tại với lệnh `curl`, nếu Máy B không phản hồi, stage `Trigger CD Deploy` sẽ báo lỗi fail để thông báo cho kỹ sư DevOps biết việc triển khai chưa đến đích.
  Trong thực tế sản xuất lớn hơn, để tăng tính sẵn sàng (High Availability), chúng em có thể nâng cấp bằng 2 giải pháp:
  1. Sử dụng hàng đợi tin nhắn (Message Queue như RabbitMQ / Kafka) hoặc cơ chế Publish/Subscribe. Jenkins bắn sự kiện `ImagePushed` vào queue, khi nào Máy B online trở lại nó sẽ tự tiêu thụ message và deploy.
  2. Hoặc sử dụng công cụ GitOps (như ArgoCD / FluxCD) với cơ chế Pull-based: Máy B liên tục thăm dò (poll) Registry hoặc Git repository, khi phát hiện có tag mới nó sẽ tự động đồng bộ mà không cần phụ thuộc vào một request HTTP đơn lẻ từ Máy A."*

---

<a id="phu-luc-cheatsheet-lenh-nhanh-cho-nhom-khi-len-bang"></a>
## PHỤ LỤC: CHEATSHEET LỆNH NHANH CHO NHÓM KHI LÊN BẢNG

### Lệnh kiểm tra trên Máy A (Jenkins CI)
```bash
# 1. Chạy test thủ công tại local
dotnet test

# 2. Tạo SSH Deploy Key
ssh-keygen -t ed25519 -C "jenkins-deploy-key" -f jenkins_deploy_key -N ""

# 3. Test ping sang máy B
ping 192.168.1.50

# 4. Git commit & push nhanh
git add .
git commit -m "feat: live demo presentation update"
git push origin feat/buoi10-presentation-guide
```

### Lệnh kiểm tra trên Máy B (CD Server)
```bash
# 1. Khởi động SQL Server
cd deploy
docker compose up -d sqlserver

# 2. Khởi động Webhook Receiver
python webhook_receiver.py

# 3. Test sức khỏe Webhook Receiver
curl http://localhost:9000/health

# 4. Xem log Webhook trực tiếp
# Trên Linux/macOS:
tail -f webhook.log
# Trên Windows PowerShell:
Get-Content webhook.log -Wait

# 5. Xem log container Web API và quá trình chạy Migration
docker logs -f material-api

# 6. Test Web API sau deploy
curl http://localhost:8080/health
curl http://localhost:8080/api/materials
```
