# 🎙️ KỊCH BẢN THUYẾT TRÌNH BÁO CÁO BUỔI 10 (BÁM SÁT 52 SLIDE POWERPOINT)

Tài liệu kịch bản thuyết trình chi tiết từng phần, bám sát toàn bộ 7 phần của slide `Buoi10_CICD.pptx`, kèm lời thoại mẫu và các câu hỏi giám khảo thường hỏi.

---

## 📑 MỤC LỤC BÀI THUYẾT TRÌNH

- **PHẦN 1**: Tổng quan về CI/CD và Jenkins (Slide 1 - 6)
- **PHẦN 2**: Private Repo, Jenkins và SSH Deploy Key (Slide 7 - 20)
- **PHẦN 3**: Unit Test — Trái tim của CI (Slide 21 - 25)
- **PHẦN 4**: Jenkinsfile — Pipeline as Code (Slide 26 - 33)
- **PHẦN 5**: Docker Registry và Quản lý Image Tag (Slide 34 - 39)
- **PHẦN 6**: Cơ chế Webhook sang Máy CD (Slide 40 - 46)
- **PHẦN 7**: Database Migration và Tổng kết luồng (Slide 47 - 52)
- **PHẦN DEMO THỰC TẾ**: Kịch bản thao tác trực tiếp trên 2 máy

---

## 📌 PHẦN 1: TỔNG QUAN VỀ CI/CD VÀ JENKINS (Slide 1 - 6)

### Lời thoại thuyết trình:
> *"Kính thưa thầy/cô và các bạn, nhóm xin phép bắt đầu bài báo cáo Buổi 10 với chủ đề: **Xây dựng quy trình CI/CD hoàn chỉnh với Jenkins và GitHub Deploy Key**.*
>
> *(Slide 2) Trong thực tế phát triển phần mềm, quy trình triển khai thủ công thường gặp vấn đề kinh điển: 'Code chạy tốt trên máy em nhưng lên server lại lỗi'. Việc dev tự build rồi copy file sang server rất dễ sai sót, không kiểm tra được code mới có phá vỡ tính năng cũ hay không, và không thể truy vết phiên bản nào đang chạy trên production.*
>
> *(Slide 3 - 4) Để giải quyết vấn đề này, CI/CD ra đời:
> - **CI (Continuous Integration)**: Tự động tích hợp code, tự chạy test để bảo đảm chất lượng trước khi đóng gói.
> - **CD (Continuous Deployment)**: Tự động đưa gói ứng dụng đã kiểm thử lên môi trường thật.
> Và **Jenkins** chính là công cụ mã nguồn mở mạnh mẽ, phổ biến nhất để tự động hóa toàn bộ quy trình này thông qua hệ thống Pipeline as Code.*
>
> *(Slide 5 - 6) Điểm đặc biệt của bài thực hành hôm nay là **Mô hình 2 máy độc lập**:
> - **Máy A (CI)**: Chạy Jenkins và Docker Registry. Máy này nắm giữ source code, chịu trách nhiệm build và test.
> - **Máy B (CD)**: Máy chủ triển khai thật. Máy này **hoàn toàn không chứa source code**, chỉ nhận lệnh qua Webhook, kéo Docker image về chạy container và cập nhật database.
> Việc tách 2 máy giúp mô phỏng chính xác môi trường doanh nghiệp: build một nơi, chạy một nơi."*

---

## 📌 PHẦN 2: PRIVATE REPO, JENKINS & DEPLOY KEY (Slide 7 - 20)

### Lời thoại thuyết trình:
> *(Slide 7 - 8) Dự án Web API của nhóm được đặt ở chế độ **Private Repository** trên GitHub. Trong thực tế, mã nguồn sản phẩm của công ty luôn luôn là private, do đó hệ thống CI bắt buộc phải được cấp quyền truy cập bảo mật rõ ràng.*
>
> *(Slide 9 - 13) Về cách cài đặt Jenkins, nhóm chọn giải pháp **chạy Jenkins bằng Docker container** với image tùy chỉnh. Lý do là image gốc của Jenkins chưa có sẵn Docker CLI, nhóm đã cài thêm Docker CLI và mount socket `/var/run/docker.sock` từ máy host vào container. Nhờ đó, Jenkins có thể trực tiếp ra lệnh cho Docker Engine của máy host build và push image.*
>
> *(Slide 14 - 20) Để Jenkins clone được private repo một cách an toàn nhất, nhóm sử dụng **SSH Deploy Key**:
> - Khóa được sinh riêng biệt cho Jenkins, không dùng chung key cá nhân.
> - Public Key được dán lên GitHub Deploy Keys và **chỉ cấp quyền Read-Only** (không cấp quyền ghi).
> - Private Key được lưu an toàn trong Jenkins Credentials.
> So với việc dùng Personal Access Token (PAT), Deploy Key an toàn hơn vì phạm vi chỉ giới hạn trong duy nhất 1 repository này, nếu máy CI có bị lộ key thì các repository khác của tài khoản vẫn hoàn toàn bảo mật."*

---

## 📌 PHẦN 3: UNIT TEST — TRÁI TIM CỦA CI (Slide 21 - 25)

### Lời thoại thuyết trình:
> *(Slide 21 - 23) Một sai lầm phổ biến khi làm CI là nghĩ rằng 'chỉ cần build không lỗi biên dịch là xong'. Nếu chỉ build mà không test, pipeline hoàn toàn mất đi ý nghĩa của CI.*
>
> *Unit Test là các đoạn code tự động kiểm tra logic của các hàm nghiệp vụ, chạy độc lập và cực kỳ nhanh. Trong pipeline của nhóm, stage **Test** được đặt ngay trước stage **Build Image**. Chỉ khi 100% các bài test chạy thành công thì hệ thống mới cho phép đóng gói image.*
>
> *(Slide 24 - 25) Cơ chế hoạt động ở đây dựa trên **Exit Code**:
> - Khi chạy lệnh `dotnet test`, nếu tất cả test pass, hệ thống trả về Exit Code `0` → Jenkins cho chạy tiếp.
> - Nếu có dù chỉ 1 test fail, `dotnet test` trả về Exit Code khác 0 → Jenkins lập tức đánh dấu stage thất bại, dừng pipeline ngay và không tạo ra bất kỳ Docker image lỗi nào."*

---

## 📌 PHẦN 4: JENKINSFILE — PIPELINE AS CODE (Slide 26 - 33)

### Lời thoại thuyết trình:
> *(Slide 26 - 28) Toàn bộ quy trình CI/CD được nhóm định nghĩa bằng **Jenkinsfile** (sử dụng Declarative Pipeline DSL) lưu trực tiếp trong repository cùng với source code. Điều này giúp pipeline được version-control và review như code bình thường.*
>
> *(Slide 29 - 32) Pipeline của nhóm gồm đúng 5 giai đoạn tuần tự:
> 1. `Checkout`: Kéo source code private về workspace bằng SSH Deploy Key.
> 2. `Test`: Chạy Unit Test kiểm thử logic nghiệp vụ.
> 3. `Build Image`: Đóng gói ứng dụng thành Docker image, gắn tag phiên bản tự tăng theo biến `${BUILD_NUMBER}`.
> 4. `Push Image`: Đăng nhập an toàn bằng `withCredentials` và đẩy image lên Registry nội bộ.
> 5. `Trigger Deploy`: Dùng lệnh `curl` gọi sang Webhook của máy CD để kích hoạt triển khai.*
>
> *(Slide 33) **Lưu ý kỹ thuật quan trọng**: Vì Jenkins đang chạy trên máy cá nhân trong mạng LAN, không có IP Public ngoài Internet nên GitHub không thể gửi webhook tự động vào Jenkins khi có commit mới. Do đó, nhóm kích hoạt pipeline bằng cách bấm **Build Now** thủ công. Điều này hoàn toàn đúng thực tế và không ảnh hưởng đến chiều Jenkins chủ động clone code ra ngoài."*

---

## 📌 PHẦN 5: DOCKER REGISTRY & QUẢN LÝ IMAGE TAG (Slide 34 - 39)

### Lời thoại thuyết trình:
> *(Slide 34 - 35) Sau khi build image xong, CI cần một nơi lưu trữ để máy CD có thể kéo về. Nhóm đã dựng một **Private Docker Registry v2** nội bộ (port 5000 kèm giao diện Web UI port 8081). Giải pháp này giúp triển khai cực nhanh trong mạng LAN nội bộ, bảo mật và không phụ thuộc vào đường truyền Internet ra ngoài.*
>
> *(Slide 36 - 37) Nhóm **tuyệt đối không dùng mãi tag `latest`**, mà mỗi lần build đều gắn tag rõ ràng `v${BUILD_NUMBER}` (ví dụ `v7`, `v8`). Việc gắn version tag mang lại 2 lợi ích cốt lõi:
> - Dễ dàng truy vết chính xác commit nào đang chạy trên production.
> - Khi bản release mới gặp sự cố, ta có thể rollback về tag cũ ngay lập tức.*
>
> *(Slide 38 - 39) Khi push image, Jenkins sử dụng tùy chọn `--password-stdin` kết hợp với credential binding để bảo đảm mật khẩu không bao giờ bị lộ ra log hệ thống."*

---

## 📌 PHẦN 6: CƠ CHẾ WEBHOOK SANG MÁY CD (Slide 40 - 46)

### Lời thoại thuyết trình:
> *(Slide 40 - 42) Đây là điểm mấu chốt trong mô hình 2 máy. Máy B (Ubuntu VM) là máy chủ triển khai độc lập, được chuẩn bị sẵn:
> - Một dịch vụ **Webhook Listener** viết bằng script Python chuẩn lắng nghe tại port 9000.
> - File `docker-compose.yml` chứa cấu hình ứng dụng Web API và SQL Server.*
>
> *(Slide 43 - 46) Khi Jenkins hoàn thành stage Push Image, nó sẽ thực hiện lệnh:
> `curl -f -X POST http://192.168.61.128:9000/deploy -d '{"tag": "vX"}'`
> Ngay khi nhận được request, script trên máy B sẽ tự động:
> 1. Gọi `docker pull` đúng image mang tag `vX` vừa build từ Registry.
> 2. Re-tag thành `latest` và chạy `docker compose up -d --force-recreate api`.
> Nguyên tắc vàng được bảo đảm: **Build một lần, deploy image đã được kiểm thử, tuyệt đối không build lại trên máy CD**."*

---

## 📌 PHẦN 7: DATABASE MIGRATION & TỔNG KẾT LUỒNG (Slide 47 - 52)

### Lời thoại thuyết trình:
> *(Slide 47 - 51) Một ứng dụng thực tế luôn đi kèm cơ sở dữ liệu. Nếu code mới thêm cột/bảng mà database chưa cập nhật, ứng dụng sẽ sập ngay khi chạy.
> Trong dự án `material-management-api`, nhóm đã tích hợp cơ chế **Auto-Migration** trong `Program.cs`. Khi container Web API trên máy B vừa khởi động, Entity Framework Core sẽ tự động gọi phương thức `dbContext.Database.Migrate()`:
> - Đối chiếu với bảng `__EFMigrationsHistory` trong SQL Server.
> - Tự động áp dụng tất cả các migration còn thiếu mà không làm mất dữ liệu cũ.*
>
> *(Slide 52) Tóm lại toàn bộ luồng: Dev push code → Bấm Build trên Máy A → Checkout private → Test pass → Build image vN → Push registry → Gửi Webhook sang Máy B → Máy B pull image vN → Chạy migration → Ứng dụng production hoạt động đồng bộ hoàn toàn!"*

---

## 🚀 PHẦN THỰC HÀNH DEMO TRỰC TIẾP (3 BƯỚC ĂN TRỌN ĐIỂM)

### Bước 1: Demo Luồng Cơ Bản (Chạy xanh toàn bộ)
1. Chiếu 2 cửa sổ: Màn hình Máy A (Jenkins) và màn hình Máy B (Terminal Ubuntu đang chạy `webhook_listener.py`).
2. Trên Jenkins, bấm **Build Now**.
3. Cho người xem thấy:
   - Jenkins chạy tuần tự 5 stage màu xanh.
   - Terminal Ubuntu nhảy log: tự động `docker pull` image tag mới và recreate container.
4. Mở Swagger UI tại `http://192.168.61.128:5001/swagger/index.html` gọi thử API `GET /api/Materials`, trả về danh sách 10 vật tư thật.

---

### Bước 2: Kịch Bản Test Chặn Pipeline (Bắt buộc theo mục 3.4 PDF)
1. Mở file `TH4.Tests/MethodSyntaxMaterialServiceTests.cs` sửa 1 dòng assertion thành sai:
   ```csharp
   Assert.Equal("TEN_CO_TINH_LAM_SAI_DE_DEMO_FAIL", result.Name);
   ```
2. Commit & push lên GitHub:
   ```bash
   git commit -am "test: introduce intentional test failure for CI demo" && git push origin main
   ```
3. Bấm **Build Now** trên Jenkins:
   - Stage `Checkout`: **Xanh**.
   - Stage `Test`: **ĐỎ (Failed)**.
   - Các stage `Build Image`, `Push Image`, `Trigger Deploy`: **Bị ngắt (Skipped)**.
4. **Nhấn mạnh với giảng viên**: Image lỗi không được tạo ra, máy B không hề bị cập nhật bậy.
5. Sửa lại code đúng, commit, push và bấm Build lại → Pipeline xanh trở lại.

---

### Bước 3: Kịch Bản Live Code Change (Bắt buộc theo mục 3.8 PDF)
1. Mở `Controllers/MaterialsController.cs`, thêm endpoint mới:
   ```csharp
   [HttpGet("live-demo-version")]
   public IActionResult GetLiveDemoVersion()
   {
       return Ok(new { version = "v-LIVE-DEMO", message = "Jenkins CI/CD deploy thanh cong!" });
   }
   ```
2. Commit & push lên GitHub:
   ```bash
   git commit -am "feat: add live-demo endpoint" && git push origin main
   ```
3. Bấm **Build Now** trên Jenkins.
4. Quan sát: Jenkins tự build tag mới, máy B tự pull và cập nhật container.
5. F5 lại trang Swagger trên máy B: Thấy endpoint `live-demo-version` xuất hiện và gọi trả về `200 OK` ngay lập tức!
