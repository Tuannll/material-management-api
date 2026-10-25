# Hạ Tầng Máy B (CD Machine) - Continuous Deployment

Thư mục này chứa toàn bộ cấu hình và kịch bản chạy trên **Máy B (CD Server)** phục vụ kịch bản CI/CD 2 máy độc lập (Máy A: Jenkins CI, Máy B: Deploy CD).

---

## 1. Kiến Trúc Hoạt Động (Flow)

```
[Máy A - Jenkins CI]
    │
    ├── 1. Checkout code từ GitHub (Private Repo qua SSH Deploy Key / PAT)
    ├── 2. Chạy Unit Test (dotnet test)
    ├── 3. Build Docker Image (gắn tag BUILD_NUMBER / Commit SHA)
    ├── 4. Push Image lên Registry (Docker Hub / GHCR / Local Registry)
    └── 5. Trigger Deploy (gọi HTTP POST sang Máy B)
              │
              │  HTTP POST http://<IP_May_B>:9000/deploy
              │  Payload: {"tag": "15"}
              ▼
[Máy B - CD Server]
    │
    ├── `webhook_receiver.py` (Lắng nghe cổng 9000)
    │     ├── Nhận tag "15"
    │     ├── Ghi log ra `webhook.log`
    │     ├── Export IMAGE_TAG=15 và cập nhật `.env`
    │     ├── Chạy `docker compose pull api` (kéo image mới về)
    │     └── Chạy `docker compose up -d api` (reload container api)
    │
    └── `docker-compose.yml`
          ├── Service `sqlserver`: MSSQL 2022, persist volume, healthcheck
          └── Service `api`: ASP.NET Core Web API, kết nối `sqlserver`, tự động apply migrations khi start
```

---

## 2. Danh Sách Tệp Tin Trong Thư Mục `deploy/`

| Tệp tin | Vai trò |
|---|---|
| `docker-compose.yml` | Định nghĩa 2 service: `sqlserver` (DB) và `api` (Web API). |
| `webhook_receiver.py` | Webhook HTTP receiver bằng Python thuần (thư viện chuẩn, không cần pip). Lắng nghe cổng 9000, nhận tín hiệu từ Jenkins và chạy lệnh pull/reload. |
| `.env.example` | Mẫu cấu hình biến môi trường (`SA_PASSWORD`, `IMAGE_NAME`, `IMAGE_TAG`, `PORT`, v.v.). |
| `setup_cd_machine.sh` | Script 1 lệnh cài đặt & khởi chạy CD trên môi trường Linux / WSL / macOS. |
| `setup_cd_machine.bat` | Script 1 lệnh cài đặt & khởi chạy CD trên môi trường Windows. |

---

## 3. Hướng Dẫn Thiết Lập Nhanh Trên Máy B

### Cách 1: Chạy bằng script tự động (Khuyên dùng)
- **Trên Windows**:
  Chạy file `setup_cd_machine.bat` (click đúp hoặc chạy qua Command Prompt).
- **Trên Linux / WSL**:
  ```bash
  chmod +x setup_cd_machine.sh
  ./setup_cd_machine.sh
  ```

### Cách 2: Chạy thủ công từng bước
1. Copy file `.env.example` thành `.env`:
   ```bash
   cp .env.example .env
   # Điền mật khẩu SA_PASSWORD và IMAGE_NAME tương ứng với Registry nhóm dùng
   ```
2. Khởi động SQL Server trước:
   ```bash
   docker compose up -d sqlserver
   ```
3. Bật Webhook Receiver:
   ```bash
   python webhook_receiver.py
   ```

---

## 4. Kiểm Thử Webhook

### 4.1. Kiểm tra trạng thái Receiver (Health Check)
```bash
curl http://localhost:9000/health
```
Kết quả mong đợi:
```json
{
  "status": "healthy",
  "service": "Machine B CD Webhook Receiver",
  "current_image_tag": "latest",
  "timestamp": "..."
}
```

### 4.2. Giả lập Jenkins Trigger Deploy
```bash
curl -X POST http://<IP_May_B>:9000/deploy \
     -H "Content-Type: application/json" \
     -d '{"tag": "v1.0.0"}'
```
Kết quả mong đợi:
```json
{
  "status": "success",
  "message": "Deployment for tag 'v1.0.0' completed successfully on Machine B",
  "tag": "v1.0.0",
  "details": {
    "pull": "...",
    "up": "..."
  }
}
```

---

## 5. Cấu Hình Jenkinsfile Trên Máy A (Tham khảo)

Trong Jenkinsfile trên Máy A, thêm stage `Trigger Deploy`:
```groovy
stage('Trigger Deploy') {
    steps {
        script {
            def cdMachineIp = "192.168.1.50" // IP của Máy B trong mạng LAN
            def cdPort = "9000"
            def payload = """{"tag":"${env.BUILD_NUMBER}"}"""

            echo "Sending deploy webhook to Machine B (http://${cdMachineIp}:${cdPort}/deploy)..."
            sh """
                curl -s -f -X POST http://${cdMachineIp}:${cdPort}/deploy \
                     -H "Content-Type: application/json" \
                     -d '${payload}'
            """
        }
    }
}
```
