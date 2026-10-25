# 🎯 Kịch bản Demo Buổi 10: CI/CD với Jenkins + GitHub Deploy Key

Tài liệu hướng dẫn thực hành 2 kịch bản bắt buộc khi thuyết trình và chấm điểm bài tập lớn theo yêu cầu trong đề bài PDF.

---

## 📋 THÔNG TIN HỆ THỐNG DEMO

- **Máy A (Host Windows 11 - Máy CI)**:
  - IP: `192.168.61.1`
  - Jenkins: `http://localhost:8080` (admin / admin123)
  - Docker Registry: `http://192.168.61.1:5000`
  - Registry UI: `http://localhost:8081` (admin / admin123)
- **Máy B (Ubuntu VM - Máy CD Triển khai)**:
  - IP: `192.168.61.128`
  - Webhook Listener: `http://192.168.61.128:9000/deploy`
  - Web API Production: `http://192.168.61.128:5001/api/Materials`
  - Swagger UI: `http://192.168.61.128:5001/swagger/index.html`

---

## ⚡ TỔNG HỢP NHANH QUY TRÌNH DEMO TRONG 2 PHÚT (Cheat Sheet Trước Khi Lên Trình Bày)

### 1. Chuẩn bị trên Máy A (Windows Host):
1. Bật **Docker Desktop** (chờ icon chuyển màu xanh).
2. Mở terminal chạy 2 lệnh bật container nếu chưa chạy:
   ```bash
   cd D:\1.DH\n4k1\1.Adv-Server\pratice\b10-cicd\docker-registry && docker compose up -d
   cd D:\1.DH\n4k1\1.Adv-Server\pratice\b10-cicd\jenkins-docker-setup && docker compose up -d
   ```
3. Mở sẵn 3 tab trình duyệt:
   - Jenkins: `http://localhost:8080` (job: `material-management-api`)
   - Registry UI: `http://localhost:8081` (xem danh sách image)
   - GitHub Repo (Private): `https://github.com/Tuannll/material-management-api`

### 2. Chuẩn bị trên Máy B (Ubuntu VM):
Mở terminal Ubuntu, chạy 2 lệnh chuẩn bị:
```bash
cd ~/cd-deploy
sudo docker compose up -d sqlserver
sudo python3 webhook_listener.py
```
*(Để nguyên cửa sổ terminal này trên màn hình chiếu để người xem thấy log CD nhảy realtime)*.

### 3. Thực hiện Demo Cơ Bản (Bấm Build):
1. Giới thiệu mô hình 2 máy: Máy A đóng vai trò CI (Jenkins), Máy B đóng vai trò CD (máy chủ deploy).
2. Trên Jenkins: Bấm **Build Now**.
3. Cho người xem quan sát:
   - Jenkins đi qua đủ 5 stage xanh (`Checkout` → `Test` → `Build Image` → `Push Image` → `Trigger Deploy`).
   - Terminal Ubuntu nhảy log: tự động `docker pull` image theo tag mới, re-tag sang `latest`, recreate container `cd-material-api`.
   - Mở Swagger trên trình duyệt: `http://192.168.61.128:5001/swagger/index.html` gọi thử API `GET /api/Materials` trả về `200 OK` có dữ liệu vật tư thật.

Xong phần demo cơ bản! Tiếp tục thực hiện 2 kịch bản bắt buộc bên dưới:

---

## 🧪 KỊCH BẢN 1: CHỨNG MINH STAGE TEST CHẶN ĐỨNG PIPELINE (Bắt buộc theo mục 3.4 PDF)

### 1. Ý nghĩa trình bày cho người xem / giảng viên
> *"Trong quy trình CI thực tế, CI không chỉ đơn thuần là build ra được file là xong. Unit Test là chốt chặn chất lượng bắt buộc. Nếu developer vô tình commit code làm hỏng logic nghiệp vụ, pipeline phải dừng ngay lập tức tại bước Test, tuyệt đối không được đóng gói Docker image và không được đẩy sang môi trường deploy."*

---

### 2. Các bước thực hiện

#### Bước 1.1: Cố tình làm sai 1 test case
Mở file `TH4.Tests/MethodSyntaxMaterialServiceTests.cs`.
Tìm test đầu tiên `Create_ValidMaterial_ReturnsDtoAndPersists`:
Sửa dòng:
```csharp
Assert.Equal("Thep Hoa Phat", result.Name);
```
Thành:
```csharp
Assert.Equal("TEN_CO_TINH_LAM_SAI_DE_DEMO_FAIL", result.Name);
```

#### Bước 1.2: Commit và push code lỗi lên GitHub
Chạy trong terminal Windows (`material-management-api`):
```bash
git add .
git commit -m "test: introduce intentional test failure for CI demo"
git push origin main
```

#### Bước 1.3: Bấm Build trên Jenkins và giải thích
1. Vào Jenkins `http://localhost:8080` → Bấm **Build Now**.
2. Chỉ cho người xem thấy:
   - Stage `Checkout`: **Xanh (Success)**.
   - Stage `Test`: **ĐỎ (Failed)**.
   - Các stage sau (`Build Image`, `Push Image`, `Trigger Deploy`): **Không được chạy (Skipped)**.
3. Bấm vào Console Output của build vừa chạy, cuộn xuống chỉ dòng lỗi:
   ```text
   Assert.Equal() Failure: Strings differ
   Expected: TEN_CO_TINH_LAM_SAI_DE_DEMO_FAIL
   Actual:   Thep Hoa Phat
   ```
4. **Nhấn mạnh với thầy cô**:
   - Docker Registry không có tag mới sinh ra.
   - Máy CD (Ubuntu) không hề nhận được webhook, container production vẫn an toàn giữ nguyên bản cũ, không bị lỗi.

---

#### Bước 1.4: Khắc phục lỗi và đưa pipeline xanh trở lại
Sửa lại dòng trong `TH4.Tests/MethodSyntaxMaterialServiceTests.cs` về đúng:
```csharp
Assert.Equal("Thep Hoa Phat", result.Name);
```

Commit và push:
```bash
git add .
git commit -m "fix: revert test to correct assertion"
git push origin main
```

Bấm **Build Now** trên Jenkins:
- Cả 5 stage đều chạy xanh lét từ đầu đến cuối!

---

## 🚀 KỊCH BẢN 2: LIVE CODE CHANGE - TỪ CODE MỚI ĐẾN PRODUCTION (Bắt buộc theo mục 3.8 PDF)

### 1. Ý nghĩa trình bày cho người xem / giảng viên
> *"Bây giờ nhóm sẽ chứng minh tính tự động hóa hoàn toàn của chu trình CI/CD: Viết thêm tính năng mới ngay trước mặt người xem, commit lên GitHub, bấm Build, và hệ thống sẽ tự động test, tự build image gắn tag mới, đẩy lên registry và máy CD tự động kéo về triển khai bản mới mà không cần thao tác tay trên server deploy."*

---

### 2. Các bước thực hiện

#### Bước 2.1: Thêm một endpoint mới vào Web API
Mở file `Controllers/MaterialsController.cs`.
Thêm phương thức sau vào ngay bên dưới constructor hoặc cuối Controller:

```csharp
/// <summary>
/// Endpoint kiểm tra phiên bản deploy trực tiếp trong buổi thuyết trình
/// </summary>
[HttpGet("live-demo-version")]
public IActionResult GetLiveDemoVersion()
{
    return Ok(new
    {
        version = "v-LIVE-DEMO-BUOI-10",
        author = "Nhom Demo CI/CD",
        message = "Tinh nang moi da duoc Jenkins CI/CD tu dong deploy thanh cong len May B!",
        deployedAt = DateTime.UtcNow
    });
}
```

#### Bước 2.2: Commit & push lên GitHub
Chạy trong terminal Windows (`material-management-api`):
```bash
git add .
git commit -m "feat: add live-demo-version endpoint for evaluation"
git push origin main
```

#### Bước 2.3: Kích hoạt pipeline và theo dõi 2 màn hình song song
1. Vào Jenkins bấm **Build Now**.
2. **Màn hình Máy A (Jenkins)**:
   - Stage `Checkout`: Kéo commit mới nhất về.
   - Stage `Test`: Chạy 5 test cases đảm bảo tính năng mới không phá vỡ logic cũ.
   - Stage `Build Image`: Build Docker image gắn tag mới (ví dụ `v9`).
   - Stage `Push Image`: Push tag `v9` lên Docker Registry `192.168.61.1:5000`.
   - Stage `Trigger Deploy`: Gửi POST webhook sang Máy B.
3. **Màn hình Máy B (Terminal Ubuntu đang chạy `python3 webhook_listener.py`)**:
   - Thấy ngay log nhảy realtime:
     ```text
     [CD Webhook] Nhan trigger deploy cho image: 192.168.61.1:5000/material-management-api:v9
     --> [Step 1] Pulling image ...:v9...
     --> [Step 2] Tagging ...:v9 as ...:latest...
     --> [Step 3] Recreating api container via docker compose...
     [CD Webhook] TRIEN KHAI THANH CONG PHIEN BAN v9!
     ```

#### Bước 2.4: Kiểm chứng tính năng mới hoạt động trên Production
1. Mở trình duyệt vào Swagger trên Máy B:
   👉 `http://192.168.61.128:5001/swagger/index.html`
   - Refresh trang: Thấy xuất hiện ngay endpoint mới `GET /api/Materials/live-demo-version`.
   - Bấm **Try it out** → **Execute**.
2. Hoặc gọi bằng lệnh `curl`:
   ```bash
   curl http://192.168.61.128:5001/api/Materials/live-demo-version
   ```
   Kết quả trả về:
   ```json
   {
     "version": "v-LIVE-DEMO-BUOI-10",
     "author": "Nhom Demo CI/CD",
     "message": "Tinh nang moi da duoc Jenkins CI/CD tu dong deploy thanh cong len May B!",
     "deployedAt": "2026-10-25T..."
   }
   ```

---

## 💡 CÁC CÂU HỎI THƯỜNG GẶP CỦA GIẢNG VIÊN & CÁCH TRẢ LỜI

**Câu 1: Vì sao Jenkins không tự chạy khi vừa push git mà phải bấm "Build Now" thủ công?**
> *Trả lời: Do Jenkins chạy trên máy cá nhân/mạng LAN nội bộ, không có IP Public hoặc Domain ngoài Internet nên GitHub không thể gửi webhook ngược vào máy cá nhân được. Chiều đi ra (Jenkins chủ động kéo code từ GitHub qua SSH Deploy Key) thì hoạt động bình thường.*

**Câu 2: Vì sao Deploy Key lại an toàn hơn dùng tài khoản cá nhân?**
> *Trả lời: Deploy Key gắn trực tiếp theo từng Repository và được cấp quyền Read-Only. Dù máy CI có bị lộ key thì kẻ xấu chỉ đọc được duy nhất 1 repo này, không sửa được code và không truy cập được các repo khác của tài khoản.*

**Câu 3: Vì sao lại cần tag version (`v${BUILD_NUMBER}`) mà không dùng mỗi tag `latest`?**
> *Trả lời: Dùng tag định danh phiên bản giúp truy vết chính xác container production đang chạy bản code commit nào. Nếu có lỗi nghiêm trọng phát sinh, ta có thể rollback về tag cũ ngay lập tức mà không cần build lại.*

**Câu 4: Ứng dụng tự cập nhật database như thế nào khi container mới khởi động?**
> *Trả lời: Trong `Program.cs`, nhóm sử dụng `dbContext.Database.Migrate()`. Mỗi khi container mới bật lên kết nối tới SQL Server, EF Core sẽ tự động kiểm tra bảng `__EFMigrationsHistory` và áp dụng các file Migration mới nhất vào database.*
