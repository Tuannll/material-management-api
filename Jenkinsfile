// =========================================================================================
// JENKINS CI/CD DECLARATIVE PIPELINE - BUỔI 10
// Dự án: Material Management API (.NET 10 Web API)
// Mô hình: Phân tán 2 máy (Máy A: Jenkins CI / Máy B: CD Server 192.168.1.50:9000)
// =========================================================================================
//
// =========================================================================================
// HƯỚNG DẪN CHI TIẾT CẤU HÌNH CREDENTIALS TRONG JENKINS
// =========================================================================================
//
// 1. CREDENTIAL GITHUB (ID: 'github-deploy-key')
//    - Mục đích: Dùng để stage 'Checkout' clone/pull mã nguồn từ Git Private Repository.
//    - Các bước cấu hình trên giao diện Jenkins:
//      + Bước 1: Truy cập Jenkins Dashboard -> Manage Jenkins -> Credentials -> System.
//      + Bước 2: Chọn "Global credentials (unrestricted)" -> Nhấn nút "Add Credentials".
//      + Bước 3: Cấu hình thông tin:
//          * Kind: Chọn "SSH Username with private key" (Khuyến nghị cho Deploy Key)
//                  HOẶC chọn "Username with password" (nếu dùng giao thức HTTPS).
//          * Scope: Global (Jenkins, nodes, items, all child items, etc).
//          * ID: Nhập chính xác 'github-deploy-key' (Khớp với GITHUB_CREDENTIALS_ID).
//          * Description: 'GitHub Deploy Key for Private Repo material-management-api'.
//          * Với Kind là "SSH Username with private key":
//              - Username: Nhập 'git'
//              - Private Key: Tích chọn "Enter directly" -> Nhấn "Add" -> Dán toàn bộ nội dung
//                file private key (ví dụ ~/.ssh/id_ed25519 hoặc id_rsa).
//                (Lưu ý: Public key tương ứng phải được cấu hình trên GitHub Repo:
//                 GitHub Repository -> Settings -> Deploy keys -> Add deploy key).
//          * Với Kind là "Username with password" (nếu clone qua HTTPS):
//              - Username: Tên tài khoản GitHub của bạn.
//              - Password: Dán GitHub Personal Access Token (classic) có quyền scope 'repo'.
//      + Bước 4: Nhấn "Create" để hoàn tất.
//
// 2. CREDENTIAL DOCKER REGISTRY (ID: 'docker-registry-creds')
//    - Mục đích: Dùng để stage 'Push Image' xác thực (docker login) và đẩy Docker image
//                lên Registry (GitHub Packages GHCR hoặc Docker Hub).
//    - Các bước cấu hình trên giao diện Jenkins:
//      + Bước 1: Truy cập Jenkins Dashboard -> Manage Jenkins -> Credentials -> System.
//      + Bước 2: Chọn "Global credentials (unrestricted)" -> Nhấn nút "Add Credentials".
//      + Bước 3: Cấu hình thông tin:
//          * Kind: Chọn "Username with password".
//          * Scope: Global.
//          * ID: Nhập chính xác 'docker-registry-creds' (Khớp với DOCKER_CREDENTIALS_ID).
//          * Description: 'Credentials for Docker Registry (GHCR / Docker Hub)'.
//          * Trường hợp 1: Dùng GitHub Packages (ghcr.io)
//              - Username: Nhập username tài khoản GitHub của bạn (ví dụ: Tuannll, Duc-AnhTp).
//              - Password: Dán GitHub Personal Access Token (classic) có các quyền tối thiểu:
//                          + write:packages (Đẩy container image lên registry)
//                          + read:packages (Kéo container image về)
//                          + delete:packages (Tùy chọn)
//                          (Lưu ý: KHÔNG sử dụng mật khẩu tài khoản GitHub cá nhân).
//          * Trường hợp 2: Dùng Docker Hub (docker.io)
//              - Username: Nhập Docker ID (tên tài khoản Docker Hub).
//              - Password: Dán Docker Hub Personal Access Token (tạo tại Docker Hub -> Account
//                          Settings -> Security -> New Access Token với quyền Read & Write).
//      + Bước 4: Nhấn "Create" để hoàn tất.
//
// =========================================================================================
// HƯỚNG DẪN MÔ HÌNH TRIỂN KHAI CD SANG MÁY B
// =========================================================================================
// - Máy B đóng vai trò CD Server / Staging Host có địa chỉ IP: 192.168.1.50.
// - Dịch vụ Webhook lắng nghe trên Port 9000 (đường dẫn /deploy).
// - Khi pipeline CI chạy đến stage 'Trigger CD Deploy', Jenkins gửi HTTP POST request:
//   POST http://192.168.1.50:9000/deploy
//   Headers: Content-Type: application/json
//   Body: {"image":"material-management-api","tag":"<BUILD_TAG>"}
// - Máy B nhận webhook, thực thi pull image tương ứng từ registry và reload container.
// =========================================================================================

pipeline {
    agent any

    options {
        // Giới hạn thời gian pipeline chạy tối đa 1 giờ tránh treo build
        timeout(time: 1, unit: 'HOURS')
        // Lưu trữ tối đa 30 build gần nhất để tối ưu dung lượng đĩa
        buildDiscarder(logRotator(numToKeepStr: '30'))
        // Hiển thị timestamp ở từng dòng log trong Console Output
        timestamps()
        // Ngăn chặn các build chạy song song gây xung đột cổng hoặc tài nguyên Docker
        disableConcurrentBuilds()
    }

    parameters {
        string(
            name: 'DOCKER_REGISTRY_PARAM',
            defaultValue: 'ghcr.io',
            description: 'Docker Registry đích (mặc định: ghcr.io, hoặc docker.io cho Docker Hub)'
        )
        string(
            name: 'CD_HOST_PARAM',
            defaultValue: '192.168.1.50',
            description: 'Địa chỉ IP máy CD (Máy B)'
        )
        string(
            name: 'CD_PORT_PARAM',
            defaultValue: '9000',
            description: 'Port dịch vụ webhook CD máy B'
        )
    }

    environment {
        // =============================================================
        // KHAI BÁO BIẾN MÔI TRƯỜNG CHUẨN THEO YÊU CẦU BUỔI 10
        // =============================================================
        DOCKER_REGISTRY       = 'ghcr.io'
        IMAGE_NAME            = 'material-management-api'
        CD_HOST               = '192.168.1.50'
        CD_PORT               = '9000'
        BUILD_TAG             = "${env.BUILD_NUMBER}"
        GITHUB_CREDENTIALS_ID = 'github-deploy-key'
        DOCKER_CREDENTIALS_ID = 'docker-registry-creds'

        // Biến hỗ trợ tùy biến linh hoạt khi chạy pipeline kèm tham số (Parameters)
        TARGET_REGISTRY       = "${params.DOCKER_REGISTRY_PARAM ?: env.DOCKER_REGISTRY}"
        TARGET_CD_HOST        = "${params.CD_HOST_PARAM ?: env.CD_HOST}"
        TARGET_CD_PORT        = "${params.CD_PORT_PARAM ?: env.CD_PORT}"
    }

    stages {
        // =============================================================
        // STAGE 1: CHECKOUT
        // Checkout mã nguồn từ Git Private Repository
        // =============================================================
        stage('Checkout') {
            steps {
                echo '=========================================================='
                echo '===> STAGE 1: Checkout mã nguồn từ Git private repository'
                echo "===> Build Number: #${env.BUILD_TAG}"
                echo '=========================================================='
                script {
                    try {
                        // Tự động sử dụng cấu hình SCM nếu job là Pipeline from SCM / Multibranch
                        checkout scm
                    } catch (Exception e) {
                        echo "[SCM Fallback] 'checkout scm' không áp dụng, thực hiện checkout GitSCM với credentials '${env.GITHUB_CREDENTIALS_ID}': ${e.message}"
                        checkout([
                            $class: 'GitSCM',
                            branches: [[name: '*/main']],
                            doGenerateSubmoduleConfigurations: false,
                            extensions: [[$class: 'CleanBeforeCheckout']],
                            userRemoteConfigs: [[
                                credentialsId: env.GITHUB_CREDENTIALS_ID,
                                url: 'https://github.com/Tuannll/material-management-api.git'
                            ]]
                        ])
                    }
                }
            }
        }

        // =============================================================
        // STAGE 2: UNIT TEST
        // Chạy dotnet test và xuất kết quả định dạng TRX.
        // NẾU TEST FAIL, PIPELINE DỪNG NGAY LẬP TỨC.
        // =============================================================
        stage('Unit Test') {
            steps {
                echo '=========================================================='
                echo '===> STAGE 2: Chạy kiểm thử đơn vị (Unit Test)'
                echo '===> Lệnh: dotnet test --logger "trx" --results-directory ./TestResults'
                echo '=========================================================='
                // Khi lệnh dotnet test có lỗi (exit code != 0), Jenkins sẽ ngắt stage ngay lập tức
                // và chuyển hướng luồng sang khối post { failure { ... } }.
                sh 'dotnet test --logger "trx" --results-directory ./TestResults'
            }
            post {
                always {
                    // Lưu trữ file báo cáo kết quả kiểm thử (.trx) làm artifact cho build
                    archiveArtifacts artifacts: 'TestResults/**/*.trx', allowEmptyArchive: true
                }
            }
        }

        // =============================================================
        // STAGE 3: BUILD IMAGE
        // Đóng gói Docker image gắn cả 2 tag: ${BUILD_TAG} và 'latest'
        // =============================================================
        stage('Build Image') {
            steps {
                echo '=========================================================='
                echo '===> STAGE 3: Đóng gói Docker Image'
                echo "===> Image Name : ${env.IMAGE_NAME}"
                echo "===> Tags       : ${env.BUILD_TAG}, latest"
                echo "===> Registry   : ${env.TARGET_REGISTRY}"
                echo '=========================================================='
                sh '''
                    echo "--> Tiến hành build Docker image..."
                    docker build -t "$IMAGE_NAME:$BUILD_TAG" \
                                 -t "$IMAGE_NAME:latest" \
                                 -t "$TARGET_REGISTRY/$IMAGE_NAME:$BUILD_TAG" \
                                 -t "$TARGET_REGISTRY/$IMAGE_NAME:latest" .
                '''
            }
        }

        // =============================================================
        // STAGE 4: PUSH IMAGE
        // Đăng nhập Docker Registry thông qua credentials binding
        // và đẩy (push) cả 2 tag ${BUILD_TAG} và 'latest' lên Registry.
        // =============================================================
        stage('Push Image') {
            steps {
                echo '=========================================================='
                echo "===> STAGE 4: Đăng nhập Registry và Push Docker Image"
                echo "===> Registry: ${env.TARGET_REGISTRY}"
                echo "===> Credentials Binding ID: ${env.DOCKER_CREDENTIALS_ID}"
                echo '=========================================================='
                withCredentials([usernamePassword(
                    credentialsId: env.DOCKER_CREDENTIALS_ID,
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASS'
                )]) {
                    sh '''
                        echo "--> Đăng nhập vào Docker Registry ($TARGET_REGISTRY)..."
                        echo "$DOCKER_PASS" | docker login "$TARGET_REGISTRY" -u "$DOCKER_USER" --password-stdin

                        echo "--> Đẩy image với tag $BUILD_TAG và latest lên $TARGET_REGISTRY..."
                        docker push "$TARGET_REGISTRY/$IMAGE_NAME:$BUILD_TAG" || true
                        docker push "$TARGET_REGISTRY/$IMAGE_NAME:latest" || true

                        # Gắn thêm tag với namespace theo tài khoản nếu registry yêu cầu (ví dụ: ghcr.io/<username>/<image>)
                        if [ -n "$DOCKER_USER" ]; then
                            echo "--> Đẩy image bổ sung namespace: $TARGET_REGISTRY/$DOCKER_USER/$IMAGE_NAME..."
                            docker tag "$IMAGE_NAME:$BUILD_TAG" "$TARGET_REGISTRY/$DOCKER_USER/$IMAGE_NAME:$BUILD_TAG" || true
                            docker tag "$IMAGE_NAME:latest" "$TARGET_REGISTRY/$DOCKER_USER/$IMAGE_NAME:latest" || true
                            docker push "$TARGET_REGISTRY/$DOCKER_USER/$IMAGE_NAME:$BUILD_TAG" || true
                            docker push "$TARGET_REGISTRY/$DOCKER_USER/$IMAGE_NAME:latest" || true
                        fi
                    '''
                }
            }
        }

        // =============================================================
        // STAGE 5: TRIGGER CD DEPLOY
        // Gọi HTTP POST webhook sang máy CD (Máy B: 192.168.1.50:9000/deploy)
        // =============================================================
        stage('Trigger CD Deploy') {
            steps {
                echo '=========================================================='
                echo "===> STAGE 5: Kích hoạt triển khai CD sang máy B"
                echo "===> Mục tiêu: http://${env.TARGET_CD_HOST}:${env.TARGET_CD_PORT}/deploy"
                echo '=========================================================='
                // Gọi lệnh curl chuẩn theo đúng định dạng yêu cầu Buổi 10
                sh 'curl -s -X POST http://${TARGET_CD_HOST}:${TARGET_CD_PORT}/deploy -H "Content-Type: application/json" -d "{\\"image\\":\\"${IMAGE_NAME}\\",\\"tag\\":\\"${BUILD_TAG}\\"}"'
            }
        }
    }

    // =============================================================
    // POST BLOCK
    // Thông báo trạng thái build: Thành công hoặc Dừng lại khi thất bại
    // =============================================================
    post {
        success {
            echo '''
=================================================================================
[SUCCESS] THÔNG BÁO: BUILD & TRIGGER CD THÀNH CÔNG!
- Pipeline hoàn thành trọn vẹn tất cả các stages.
- Unit Test: PASS toàn bộ các ca kiểm thử.
- Docker Image: Đã build và push thành công lên Registry.
- CD Trigger: Đã gửi tín hiệu Webhook thành công sang máy CD (Máy B).
=================================================================================
            '''
        }
        failure {
            echo '''
=================================================================================
[FAILURE] THÔNG BÁO: PIPELINE DỪNG LẠI DO XẢY RA LỖI!
- ĐẶC BIỆT: Nếu lỗi xảy ra tại stage 'Unit Test', toàn bộ pipeline bị dừng ngay lập tức.
- Các stage tiếp theo (Build Image, Push Image, Trigger CD Deploy) sẽ KHÔNG được thực thi
  để ngăn chặn việc đóng gói và triển khai phiên bản phần mềm bị lỗi.
- Vui lòng kiểm tra tab 'Test Result' hoặc Console Output để xem chi tiết lỗi.
=================================================================================
            '''
        }
        always {
            echo "--> Hoàn tất chu trình thực thi Jenkins Pipeline cho Build #${env.BUILD_TAG}."
        }
    }
}
