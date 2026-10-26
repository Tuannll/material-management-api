# KỊCH BẢN NÓI CHI TIẾT TỪNG SLIDE (BUỔI 10 CI/CD)

---

## PHẦN 1: CI/CD và Jenkins (Slide 1 – 6)

### Slide 1 (Trang bìa - PHẦN 1: CI/CD và Jenkins)
"Xin chào thầy/cô và các bạn. Hôm nay nhóm chúng mình sẽ trình bày và demo chủ đề Buổi 10: Xây dựng quy trình CI/CD tự động hóa với Jenkins, kết hợp GitHub Private Repository và triển khai qua Webhook trên 2 máy chủ độc lập. Đầu tiên, chúng ta cùng đi vào Phần 1: Tổng quan về CI/CD và Jenkins."

### Slide 2 (Vấn đề của quy trình triển khai thủ công)
"Trước khi có CI/CD, hãy nhìn lại quy trình triển khai thủ công truyền thống gồm 3 bước: Lập trình viên code xong và tự build trên máy cá nhân; sau đó copy file hoặc image sang server; rồi SSH vào server gõ lệnh chạy lại.
Cách làm này dẫn đến 4 hậu quả lớn: Thứ nhất là câu nói kinh điển 'Chạy được trên máy em mà' do môi trường mỗi máy một khác. Thứ hai, không ai biết code mới thêm vào có vô tình làm hỏng chức năng cũ hay không. Thứ ba, thao tác tay rất tốn thời gian, dễ sai sót và khó lặp lại. Cuối cùng là không thể truy vết được phiên bản code nào đang thực sự chạy trên server. Khi một nhóm nhiều người cùng commit liên tục mỗi ngày, việc kiểm tra và deploy bằng tay là bất khả thi — đó chính là lý do CI/CD ra đời."

### Slide 3 (CI và CD)
"Vậy CI và CD cụ thể là gì?
CI (Continuous Integration - Tích hợp liên tục) là tự động hóa việc gom code từ nhiều người: tự động lấy code, chạy kiểm thử (test) và kiểm tra chất lượng trước khi cho phép đóng gói. Đầu ra của bước CI là một Docker image đã vượt qua kiểm thử và được đẩy lên kho lưu trữ.
CD (Continuous Delivery hoặc Continuous Deployment) là tự động hóa việc đưa image đã build ở bước CI lên môi trường chạy thật, bao gồm kéo image về, cập nhật database (migrate) và khởi chạy container.
Mọi người lưu ý sự khác biệt nhỏ ở phần ghi chú: Continuous Delivery sẽ dừng lại ở bước 'sẵn sàng deploy' và chờ con người bấm nút duyệt, còn Continuous Deployment là tự động hoàn toàn từ đầu đến cuối."

### Slide 4 (Jenkins là gì?)
"Để hiện thực hóa quy trình CI/CD đó, nhóm sử dụng Jenkins. Đây là công cụ mã nguồn mở giúp tự động hóa CI/CD, tích hợp rất tốt với Git, Docker, có thể gọi sang hệ thống khác qua giao thức HTTP và cho phép định nghĩa toàn bộ quy trình bằng code qua file Jenkinsfile.
Về ưu điểm, Jenkins hoàn toàn miễn phí, cho phép tự cài đặt trên máy chủ riêng (self-host), cực kỳ linh hoạt nhờ kho plugin khổng lồ và cộng đồng lớn. Nhược điểm của nó là bước cài đặt, cấu hình ban đầu khá thủ công, giao diện hơi cũ và chúng ta phải tự bảo trì server."

### Slide 5 (Vì sao demo cần 2 máy CI và CD? - Phần 1/2)
"Một câu hỏi quan trọng trong bài hôm nay là: Tại sao nhóm lại phải demo trên 2 máy riêng biệt? Trong thực tế doanh nghiệp, máy chạy Jenkins (làm nhiệm vụ CI) và máy chạy ứng dụng thật (làm nhiệm vụ CD) luôn tách rời nhau. Như sơ đồ trên slide: khi Dev đẩy code lên GitHub Private, Máy A (Jenkins) kéo code về, build xong sẽ không tự chạy ứng dụng tại chỗ. Nó đóng gói thành Docker image, đẩy lên Registry, rồi bắn một tín hiệu webhook sang Máy B (CD) để Máy B tự kéo image đó về chạy."

### Slide 6 (Vì sao demo cần 2 máy CI và CD? - Phần 2/2)
"Bảng so sánh này làm rõ vai trò của từng máy: Máy A (CI) cài Jenkins, Docker, làm nhiệm vụ test, build, push và là nơi duy nhất biết tới source code. Ngược lại, Máy B (CD) chỉ cài Docker, một bộ lắng nghe webhook (webhook listener) để chạy Web API cùng Database, và hoàn toàn không biết gì về source code. Việc tách 2 máy giúp chúng ta thấy rõ triết lý triển khai thực tế: Build một nơi, chạy một nơi, kết nối với nhau qua Registry và Webhook."

---

## PHẦN 2: Private Repo, Jenkins, Deploy Key/PAT (Slide 7 – 20)

### Slide 7 (PHẦN 2: Private Repo, Jenkins, Deploy Key/PAT)
"Tiếp theo, chúng ta sang Phần 2: Cách chuẩn bị kho mã nguồn Private, cài đặt Jenkins và cấp quyền truy cập bảo mật."

### Slide 8 (Project và Private Repository)
"Đầu tiên, toàn bộ source code Web API của nhóm được đưa lên GitHub ở chế độ Private. Cấu trúc thư mục gốc của repo bắt buộc có 4 thành phần: thư mục src/MyApi chứa code Web API, thư mục tests/MyApi.Tests chứa Unit Test, file Dockerfile để đóng gói ứng dụng và Jenkinsfile đặt ngay ở thư mục gốc. Lý do phải để Private là vì ngoài thực tế, code sản phẩm công ty là tài sản mật không bao giờ để public; do đó Jenkins muốn kéo được code thì phải được cấp quyền rõ ràng."

### Slide 9 (Cài đặt Jenkins)
"Để cài Jenkins trên Máy A, có 2 cách:
Cách 1 là cài trực tiếp trên Windows qua file .msi như một dịch vụ nền; ưu điểm là đơn giản, gọi được lệnh Docker trực tiếp trên máy chủ nhưng nhược điểm là khó cài đặt lặp lại và khi gỡ bỏ thường để lại file rác.
Cách 2 là chạy Jenkins bằng Docker container chỉ với một lệnh docker run và lưu dữ liệu qua volume; cách này rất dễ dựng, dễ dọn dẹp, không làm bẩn máy thật. Tuy nhiên, lưu ý quan trọng ở Cách 2 là nếu muốn Jenkins ngồi bên trong container mà lại gọi được lệnh build image, ta bắt buộc phải mount file docker.sock của máy thật vào và cài thêm Docker CLI cho image Jenkins."

### Slide 10 (Vì sao cần Image Jenkins tùy chỉnh - Phần 1/2)
"Tại sao phải tạo image Jenkins tùy chỉnh? Vì image gốc jenkins/jenkins:lts của nhà phát hành chỉ chứa bản thân Jenkins chứ chưa hề cài đặt công cụ Docker CLI. Nếu dùng luôn image gốc, tới bước Build Image pipeline sẽ báo lỗi ngay là docker: not found. Giải pháp là nhóm viết một Dockerfile ngắn như trên màn hình để cài thêm gói docker.io vào image gốc, rồi chạy lệnh docker build -t jenkins-docker . để tạo ra image mới."

### Slide 11 (Vì sao cần Image Jenkins tùy chỉnh - Phần 2/2)
"Nhìn vào bảng giải thích từng dòng trong Dockerfile vừa rồi: FROM jenkins/jenkins:lts là lấy image gốc làm nền; sau đó phải dùng USER root để chuyển sang quyền quản trị cao nhất thì mới chạy lệnh RUN apt-get install -y docker.io cài đặt Docker CLI được; cài xong ta trả lại USER jenkins mặc định và đóng gói thành image tên là jenkins-docker tại thư mục hiện tại (dấu chấm .)."

### Slide 12 (Lệnh 'docker run' chạy Jenkins - Phần 1/2)
"Sau khi có image jenkins-docker, ta khởi chạy container bằng lệnh docker run trên màn hình. Ý nghĩa các tham số đầu tiên: -d để container chạy ngầm; --name jenkins đặt tên cho dễ quản lý; -p 8080:8080 ánh xạ cổng để ta mở giao diện web của Jenkins tại địa chỉ localhost:8080; và -u root là tạm cấp quyền root trong container để Jenkins có quyền truy cập vào socket của Docker."

### Slide 13 (Lệnh 'docker run' chạy Jenkins - Phần 2/2)
"Các tham số còn lại cũng rất quan trọng: cổng -p 50000:50000 dành cho các Jenkins agent kết nối vào; -v jenkins_home:/var/jenkins_home giúp lưu toàn bộ cấu hình, job và mật khẩu ra một volume bền vững, lỡ có xóa container đi tạo lại thì dữ liệu vẫn còn nguyên; đặc biệt -v /var/run/docker.sock:/var/run/docker.sock chính là sợi dây nối giúp Jenkins bên trong container điều khiển được Docker Engine của máy host bên ngoài. Lưu ý nhỏ là cờ -u root chỉ nên dùng khi demo cho nhanh, còn thực tế sản xuất ta nên thêm user jenkins vào group docker để bảo mật hơn."

### Slide 14 (Sau khi Jenkins chạy lên, thiết lập lần đầu)
"Khi container đã chạy, bước thiết lập lần đầu gồm 5 thao tác: Mở trình duyệt ở localhost:8080; lấy mật khẩu admin mặc định bằng lệnh docker logs jenkins hoặc đọc từ file initialAdminPassword; chọn cài bộ plugin gợi ý (Install suggested plugins); tạo tài khoản admin; và vào mục Manage Jenkins -> Plugins để cài bổ sung plugin Docker Pipeline. Cuối cùng, ta gõ thử docker exec jenkins docker ps, nếu in ra được danh sách container nghĩa là Jenkins đã gọi được Docker thành công."

### Slide 15 (Cài Plugin)
"Tổng hợp lại, có 4 plugin cốt lõi phục vụ bài hôm nay: Git để kéo code về, Pipeline để đọc hiểu file Jenkinsfile, Credentials Binding để nạp khóa bảo mật vào quy trình một cách an toàn — 3 cái này đã có sẵn khi ta bấm Install suggested plugins. Chúng ta chỉ cần cài thêm duy nhất Docker Pipeline để chạy được các bước Docker."

### Slide 16 (Jenkins truy cập private repo: Deploy Key - 4 bước)
"Bây giờ làm sao để Jenkins kéo được code từ GitHub Private? Có 2 cách là dùng Deploy Key (qua SSH) hoặc Personal Access Token - PAT (qua HTTPS).
Với cách dùng Deploy Key, quy trình gồm 4 bước:
1. Tạo một cặp khóa SSH riêng trên máy Jenkins và tuyệt đối không đặt passphrase.
2. Lên GitHub repo, vào Settings -> Deploy keys, dán khóa công khai (public key) vào và nhớ để tắt mục 'Allow write access' vì Jenkins chỉ cần quyền đọc code chứ không cần quyền sửa code trên GitHub.
3. Vào mục Credentials của Jenkins, tạo thông tin xác thực kiểu SSH Username with private key và dán khóa bí mật (private key) vào.
4. Khai báo đường dẫn SSH git@github.com:... cùng ID của credential đó vào Jenkinsfile. Cách này có đặc điểm là mỗi khóa chỉ gắn với đúng 1 repo duy nhất."

### Slide 17 (Lệnh 'ssh-keygen' - Phần 1/2)
"Đây là câu lệnh chi tiết để sinh khóa ở Bước 1: ssh-keygen -t ed25519 -C \"jenkins-deploy-key\" -f jenkins_deploy_key. Trong đó -t ed25519 là chọn thuật toán mã hóa hiện đại, ngắn gọn, chạy nhanh và an toàn nhất hiện nay; -C là ghi chú tên khóa; và -f là tên file đầu ra."

### Slide 18 (Lệnh 'ssh-keygen' - Phần 2/2)
"Lệnh trên sẽ đẻ ra 2 file: File jenkins_deploy_key.pub (Public key) đóng vai trò như ổ khóa mang gắn lên cửa GitHub; còn file jenkins_deploy_key (Private key) là chìa khóa chỉ mình Jenkins cất giữ trong mục Credentials. Khi tạo khóa, máy hỏi passphrase thì ta nhấn Enter 2 lần để bỏ trống, vì Jenkins chạy tự động hoàn toàn, nếu có mật khẩu khóa nó sẽ bị treo để chờ người gõ. Ta có thể kiểm tra chìa khóa hoạt động chưa bằng lệnh ssh -i jenkins_deploy_key -T git@github.com."

### Slide 19 (Personal Access Token - PAT - Phần 1/2)
"Cách thay thế thứ hai đơn giản hơn là dùng PAT (Personal Access Token). Ta vào phần Developer settings của tài khoản GitHub để sinh ra một mã token, và chỉ tick cấp đúng quyền tối thiểu: quyền repo để kéo code private, và nếu tiện dùng chung mã PAT này để đẩy Docker image lên kho ghcr.io của GitHub thì tick thêm quyền write:packages và read:packages. Sau đó lưu vào Jenkins dưới dạng Username with password và clone code qua đường dẫn HTTPS."

### Slide 20 (Personal Access Token và so sánh với Deploy Key - Phần 2/2)
"Bảng này giúp chúng ta biết khi nào nên chọn cách nào: Deploy Key (SSH) chỉ mở được đúng 1 repository, mặc định chỉ đọc, gắn trực tiếp vào repo đó nên rất hợp khi Jenkins chỉ phục vụ 1 dự án cố định. Còn PAT (HTTPS) gắn với tài khoản người tạo, mở được nhiều repo và kiêm luôn việc đẩy package lên GitHub Registry. Dù chọn cách nào cũng phải nhớ nguyên tắc vàng: lưu khóa trong Jenkins Credentials, tuyệt đối không viết thẳng mật khẩu vào Jenkinsfile hay commit lên GitHub."

---

## PHẦN 3: Unit Test (Slide 21 – 25)

### Slide 21 (PHẦN 3: Unit Test)
"Sau khi Jenkins đã lấy được code về, bước tiếp theo trong quy trình CI là chạy kiểm thử tự động. Chúng ta bước sang Phần 3: Unit Test."

### Slide 22 (Unit Test là gì?)
"Unit Test (Kiểm thử đơn vị) là những đoạn code tự động dùng để kiểm tra một đơn vị nhỏ nhất của ứng dụng — thường là một hàm hoặc một method — xem nó tính toán có đúng kỳ vọng không. Một Unit Test chuẩn có 3 đặc điểm: Độc lập (không cần kết nối database hay mạng thật), Chạy rất nhanh, và Chạy lại được bất cứ lúc nào. Ví dụ trên slide là một hàm test bằng thư viện xUnit trong .NET: truyền vào 2 món hàng giá 100 và 50, và dùng Assert.Equal để khẳng định tổng tiền trả về phải bằng 150."

### Slide 23 (Vì sao Unit Test bắt buộc trong CI?)
"Tại sao trong bài này Unit Test lại là yêu cầu bắt buộc? Bởi vì nếu pipeline chỉ dừng ở mức 'biên dịch code không lỗi là xong' thì chẳng khác gì lập trình viên tự bấm build trên máy cá nhân, làm mất đi ý nghĩa cốt lõi của CI. Vai trò thật sự của Unit Test là làm chốt chặn chất lượng: như sơ đồ trên màn hình, chỉ khi tất cả Test đều PASS thì mới được đi tiếp sang bước Build Image, Push và Deploy. Nếu có bất kỳ test nào FAIL, pipeline phải lập tức dừng ngay tại chỗ, báo lỗi đỏ và tuyệt đối không tạo ra bất kỳ Docker image lỗi nào."

### Slide 24 (Lệnh dotnet test và exit code - Phần 1/2)
"Đối với dự án .NET của nhóm, để chạy kiểm thử tự động trong pipeline, Jenkins sẽ gọi lệnh dotnet test tests/MyApi.Tests/MyApi.Tests.csproj. Lệnh này sẽ tự động tìm và thực thi toàn bộ các kịch bản test có trong project kiểm thử."

### Slide 25 (Lệnh dotnet test và exit code - Phần 2/2)
"Vậy câu hỏi đặt ra là: Bản thân Jenkins làm sao biết được bài test đúng hay sai? Thực chất Jenkins không hề hiểu ngôn ngữ C#, Node.js hay Python, mà nó dựa vào một cơ chế then chốt của hệ điều hành gọi là Exit Code (Mã thoát).
Khi lệnh dotnet test (hoặc npm test, pytest) chạy xong: nếu tất cả test đều vượt qua, lệnh trả về Exit code = 0, Jenkins hiểu là thành công và cho chạy bước tiếp theo. Ngược lại, chỉ cần 1 test bị sai, lệnh sẽ trả về Exit code khác 0, Jenkins lập tức đánh dấu stage đó thất bại và hủy bỏ toàn bộ các bước phía sau."

---

## PHẦN 4: Jenkinsfile (Slide 26 – 33)

### Slide 26 (PHẦN 4: Jenkinsfile)
"Làm thế nào để ghép nối bước lấy code và bước chạy test vừa rồi thành một dây chuyền tự động? Chúng ta sang Phần 4: Định nghĩa quy trình bằng Jenkinsfile."

### Slide 27 (Jenkinsfile: Pipeline as Code - Phần 1/2)
"Jenkinsfile là một file văn bản viết bằng ngôn ngữ Groovy DSL, dùng để định nghĩa toàn bộ quy trình CI/CD bằng code (gọi là Pipeline as Code) và được lưu trực tiếp trong repository cùng với mã nguồn ứng dụng. Cách làm này mang lại 3 lợi ích lớn: biết rõ lịch sử ai sửa quy trình lúc nào qua Git, có thể review như code bình thường, và nếu máy chủ Jenkins có bị hỏng thì toàn bộ kịch bản pipeline vẫn còn nguyên trong repo. Trên màn hình là bộ khung chuẩn gồm đúng 5 giai đoạn (stages) tuần tự của bài hôm nay: Checkout -> Test -> Build Image -> Push Image -> Trigger Deploy."

### Slide 28 (Jenkinsfile: Pipeline as Code - Phần 2/2)
"Giải thích nhanh các từ khóa trong khung Declarative Pipeline: pipeline là khối lệnh gốc ngoài cùng; agent any cho phép chạy quy trình trên bất kỳ máy thực thi nào đang rảnh; stages chứa danh sách các giai đoạn mà mỗi stage sẽ hiển thị thành một cột trực quan trên giao diện web của Jenkins; còn steps là nơi viết các câu lệnh thực thi cụ thể của từng giai đoạn."

### Slide 29 (Các dòng Groovy quan trọng - Phần 1/3)
"Đi sâu vào các câu lệnh Groovy bên trong Jenkinsfile, thành phần quan trọng đầu tiên là khối biến môi trường: environment { IMAGE_TAG = \"${BUILD_NUMBER}\" }. Ở đây, BUILD_NUMBER là biến có sẵn của Jenkins, tự động tăng dần sau mỗi lần bấm chạy (lần 1, lần 2, lần 3...). Việc gán nó vào IMAGE_TAG giúp mỗi lần build sẽ sinh ra một nhãn image hoàn toàn riêng biệt. Lưu ý cú pháp Groovy: phải dùng dấu nháy kép \"${...}\" thì nó mới lấy giá trị của biến thế vào, còn dùng nháy đơn '...' thì nó chỉ hiểu là chữ thuần túy."

### Slide 30 (Các dòng Groovy quan trọng - Phần 2/3)
"Thành phần thứ hai ở bước Checkout code từ repo private là lệnh git branch: 'main', credentialsId: 'github-deploy-key', url: 'git@github.com:...'. Điểm ăn tiền về mặt bảo mật ở đây là trong Jenkinsfile, chúng ta chỉ viết cái tên định danh credentialsId chứ hoàn toàn không chứa nội dung khóa bí mật, nên có đẩy file này lên GitHub cũng hoàn toàn an toàn."

### Slide 31 (Các dòng Groovy quan trọng - Phần 3/3)
"Thành phần thứ ba là khối withCredentials dùng khi cần đăng nhập vào kho chứa image. Khối này sẽ lấy tài khoản và mật khẩu đang cất trong Jenkins gán tạm vào 2 biến REG_USER và REG_PASS chỉ tồn tại bên trong cặp dấu ngoặc nhọn { }. Ngay khi chạy hết dấu ngoặc, biến tự động biến mất; thậm chí nếu ai đó cố tình in giá trị biến này ra màn hình log, Jenkins cũng tự động che lại bằng hàng dấu sao ****. Cuối cùng, để gọi lệnh hệ thống, ta dùng bat nếu Jenkins cài trên Windows và sh nếu cài trên Linux."

### Slide 32 (Cơ chế 'fail thì dừng')
"Slide này tóm tắt lại cơ chế bảo vệ dây chuyền của Jenkinsfile: Mọi lệnh bat hay sh đều trả về exit code. Ngay khi lệnh dotnet test ở bước 2 trả về mã lỗi khác 0, Jenkins đánh dấu đỏ ở bước Test và bỏ qua toàn bộ các bước Build, Push, Trigger phía sau. Tương tự ở bước cuối cùng, nhóm dùng lệnh curl -f để nếu máy CD trả về lỗi HTTP 4xx hoặc 5xx thì bước Trigger Deploy cũng sẽ báo đỏ ngay lập tức."

### Slide 33 (Vì sao không có trigger tự động từ GitHub?)
"Có một điểm đặc thù khi demo mà nhóm cần nhấn mạnh: Tại sao khi commit code mới lên GitHub, Jenkins không tự chạy luôn mà phải bấm nút 'Build Now' bằng tay?
Hãy nhìn vào bảng chiều gọi mạng: Chiều từ Jenkins đi ra GitHub để kéo code về thì hoạt động bình thường vì máy cá nhân của chúng ta có mạng Internet đi ra ngoài. Nhưng chiều ngược lại từ GitHub gọi webhook vào Jenkins để báo có commit mới thì không thực hiện được, bởi vì Jenkins đang chạy trên máy cá nhân trong mạng LAN, không có địa chỉ IP public hay tên miền thật để máy chủ GitHub ngoài Internet tìm thấy."

---

## PHẦN 5: Registry và Image (Slide 34 – 39)

### Slide 34 (PHẦN 5: Registry và Image)
"Sau khi code đã vượt qua bài test, bước tiếp theo là đóng gói thành Docker Image và đẩy lên kho lưu trữ. Chúng ta sang Phần 5: Registry và Image."

### Slide 35 (Registry và 2 lựa chọn)
"Registry là nơi lưu trữ và phân phối Docker image: Máy CI sẽ đẩy (push) lên đây và Máy CD sẽ kéo (pull) từ đây về. Chúng ta có 2 lựa chọn phổ biến:
Một là Docker Hub — kho mặc định phổ biến nhất, miễn phí cho image công khai nhưng phải tạo tài khoản và cấu hình thêm một Credential riêng trong Jenkins.
Hai là GitHub Container Registry (ghcr.io) — kho tích hợp sẵn trong GitHub Packages, gắn liền với repo code và rất tiện ở chỗ có thể dùng chung luôn mã PAT đã tạo ở bước kéo code, hỗ trợ tốt cả image private."

### Slide 36 (Build image có gắn tag - Phần 1/2)
"Tại stage Build Image, Jenkins chạy câu lệnh: docker build -t ghcr.io/ten-user/ten-app:15 . . Trong đó: -t là gắn nhãn tên cho image; ghcr.io là địa chỉ kho chứa; ten-user là tài khoản sở hữu; ten-app là tên ứng dụng; số :15 chính là số thứ tự lần build (BUILD_NUMBER); và dấu chấm . ở cuối báo cho Docker biết hãy tìm Dockerfile ngay tại thư mục hiện tại."

### Slide 37 (Build image có gắn tag - Phần 2/2)
"Tại sao chúng ta phải gắn tag theo số lần build (:15) hoặc mã commit SHA mà tuyệt đối không dùng mãi cái nhãn :latest mặc định? Có 3 lý do sống còn: Thứ nhất, mỗi lần build có một số hiệu riêng giúp biết chính xác phiên bản nào đang chạy trên server thật. Thứ hai, nếu bản mới bị lỗi, ta có thể rollback (khôi phục) cực nhanh chỉ bằng cách bảo máy CD chạy lại số tag của bản cũ. Thứ ba, nhãn latest liên tục bị ghi đè qua các lần build, rất dễ gây nhầm lẫn."

### Slide 38 (docker login và docker push - Phần 1/2)
"Đóng gói xong, sang stage Push Image, ta cần đăng nhập rồi đẩy image lên bằng cặp lệnh trên màn hình. Ở đây nhóm sử dụng kỹ thuật đường ống (dấu gạch đứng |): lệnh echo \"$REG_PASS\" sẽ truyền thẳng mật khẩu sang làm đầu vào cho lệnh docker login ghcr.io thông qua cờ --password-stdin."

### Slide 39 (docker login và docker push - Phần 2/2)
"Vì sao phải viết dài dòng với --password-stdin thay vì gõ luôn -p <token> cho ngắn? Bởi vì nếu dùng cờ -p, chuỗi mật khẩu sẽ bị lưu lộ liễu trong lịch sử dòng lệnh (command history) và danh sách tiến trình của hệ điều hành, ai có quyền xem tiến trình đều đọc được; chính Docker cũng phát cảnh báo bảo mật về điều này. Đăng nhập an toàn xong, lệnh docker push sẽ đẩy image mang tag mới nhất lên Registry."

---

## PHẦN 6: Webhook sang máy CD (Slide 40 – 46)

### Slide 40 (PHẦN 6: Webhook sang máy CD)
"Image đã nằm trên mây (Registry). Giờ làm sao để Máy B biết mà kéo về chạy? Chúng ta sang Phần 6: Kích hoạt CD qua HTTP Webhook."

### Slide 41 (Kịch bản 2 máy và việc Máy B cần chuẩn bị - Phần 1/2)
"Trong kịch bản 2 máy của nhóm, Máy B (đóng vai trò server triển khai CD) hoàn toàn độc lập với Jenkins và cần được chuẩn bị sẵn 4 thứ từ trước:
1. Một dịch vụ nhỏ luôn lắng nghe giao thức HTTP tại đường dẫn POST /deploy (viết bằng Node.js, Python hoặc công cụ webhook CLI).
2. Các biến môi trường nhạy cảm như chuỗi kết nối Database, secret đã được cấu hình sẵn trên Máy B.
3. Đã cài sẵn Docker (và đã docker login nếu dùng image private).
4. Mở sẵn tường lửa (Firewall) cho cổng lắng nghe webhook (ví dụ cổng 9000) và cổng chạy Web API.
Nguyên tắc bất di bất dịch là: Máy CD chỉ làm đúng một việc 'kéo image mới nhất về và chạy', không build lại, không biết gì về code."

### Slide 42 (Kịch bản 2 máy và việc Máy B cần chuẩn bị - Phần 2/2)
"Sơ đồ này mô tả luồng tự động trên Máy B ngay khi nhận được tín hiệu: Nhận gói tin POST /deploy mang nội dung {\"tag\":\"15\"} -> tự động gõ lệnh docker pull kéo đúng image tag 15 về -> gọi docker compose up -d để thay thế container cũ bằng container phiên bản mới."

### Slide 43 (curl gọi webhook và docker pull + compose up - Phần 1/3)
"Để kích hoạt luồng đó, ở bước cuối cùng trên Máy A (Jenkins), pipeline sẽ chạy lệnh curl như trên màn hình. Điểm kỹ thuật cực kỳ quan trọng ở đây là cờ -f (fail): bình thường nếu máy B bị lỗi và trả về mã HTTP 400 hay 500, lệnh curl vẫn coi như đã gửi tin xong và trả về exit code = 0, làm cho Jenkins vẫn báo màu xanh thành công giả tạo. Có thêm cờ -f, hễ server trả về lỗi 4xx/5xx là curl báo lỗi ngay, giúp stage Trigger Deploy hiện màu đỏ đúng thực tế."

### Slide 44 (curl gọi webhook và docker pull + compose up - Phần 2/3)
"Các thành phần còn lại của lệnh curl: gửi tới địa chỉ IP của Máy B ở cổng 9000/deploy; đính kèm Header báo dữ liệu dạng JSON; kèm theo một Header bảo mật X-Deploy-Secret để tránh người lạ gọi bừa vào API deploy; và phần thân dữ liệu -d truyền sang số hiệu tag vừa build."

### Slide 45 (curl gọi webhook và docker pull + compose up - Phần 3/3)
"Phía bên kia chiến tuyến, Máy B nhận được lệnh sẽ thực thi 2 dòng: docker pull để tải image về, và IMAGE_TAG=15 docker compose up -d. Việc truyền biến IMAGE_TAG=15 ngay đầu câu lệnh giúp file docker-compose.yml biết chính xác phải chạy phiên bản số 15 vừa tải. Điểm thông minh của docker compose up -d là nó tự so sánh trạng thái hiện tại với file cấu hình: chỉ có service Web API bị đổi số tag mới bị khởi tạo lại, còn container Database không thay đổi gì thì vẫn giữ nguyên chạy tiếp, không bị gián đoạn."

### Slide 46 (Ý nghĩa của thiết kế này)
"Thiết kế CI chủ động 'gõ cửa' CD qua webhook này giúp chúng ta mô phỏng hoàn hảo quy trình CD thực tế ngay trong mạng LAN mà không cần thuê server hay tên miền thật. Nó giữ trọn vẹn các nguyên tắc vàng của DevOps: Build một lần duy nhất, Deploy đúng cái image đã vượt qua kiểm thử, và Đồng nhất số Tag từ lúc gọi request, trên kho lưu trữ cho tới container đang chạy — nếu có sự cố, chỉ cần gọi lại webhook với số tag cũ là rollback xong trong vài giây."

---

## PHẦN 7: Migration và Tổng kết (Slide 47 – 52)

### Slide 47 (PHẦN 7: Migration và tổng kết)
"Cuối cùng, một ứng dụng Web API thực tế không thể tách rời cơ sở dữ liệu. Khi code thay đổi cấu trúc bảng thì Database trên Máy B cập nhật thế nào? Chúng ta vào Phần 7: Migration và Tổng kết."

### Slide 48 (Web API hoàn chỉnh và Migration)
"Hãy hình dung vấn đề: ở phiên bản code mới, lập trình viên thêm một cột mới vào bảng, nhưng Database đang chạy trên Máy B vẫn là cấu trúc (schema) cũ -> khi ứng dụng chạy sẽ lập tức văng lỗi runtime 'column does not exist'.
Giải pháp bắt buộc là khi deploy bản mới, hệ thống phải chạy Migration để đồng bộ cấu trúc Database khớp với code. Có 2 cách làm phổ biến: một là gọi hàm Database.Migrate() tự chạy ngay lúc ứng dụng khởi động (như trong .NET EF Core), hai là chạy một lệnh CLI migrate riêng trước khi bật lại service (như Prisma, Sequelize). Tuy nhiên ngoài thực tế cần lưu ý: kịch bản migration tuyệt đối không được làm mất dữ liệu hiện có, và phải cẩn thận hiện tượng xung đột nếu chạy nhiều container cùng lúc."

### Slide 49 (Lệnh migration EF Core - Phần 1/3)
"Với dự án .NET sử dụng Entity Framework Core của nhóm, quy trình gồm 2 phía. Đầu tiên là phía máy lập trình viên (trước khi đẩy code lên GitHub): khi sửa model trong code xong, dev gõ lệnh dotnet ef migrations add AddProductPrice. Lệnh này sẽ sinh ra một file migration mô tả các bước thêm bảng/cột mới, và lập trình viên bắt buộc phải commit cả file migration này lên Git cùng với code."

### Slide 50 (Lệnh migration EF Core - Phần 2/3)
"Thứ hai là phía ứng dụng khi khởi động trên Máy B. Trong file Program.cs, nhóm đặt đoạn code tự động chạy ngay lúc container bật lên: dùng app.Services.CreateScope() để tạo một phạm vi dịch vụ tạm thời, lấy ra đối tượng AppDbContext đại diện cho Database và gọi lệnh db.Database.Migrate()."

### Slide 51 (Lệnh migration EF Core - Phần 3/3)
"Hàm Database.Migrate() này hoạt động cực kỳ an toàn: nó sẽ mở bảng lịch sử __EFMigrationsHistory trong Database ra đối chiếu với các file migration có trong image mới. Những bản cập nhật nào chưa chạy thì nó mới thực thi lên Database, còn nếu Database đã ở bản mới nhất rồi thì nó bỏ qua không làm gì cả. Lát nữa trong phần demo trên Máy B, nhóm sẽ mở trực tiếp bảng __EFMigrationsHistory này ra để chứng minh bản cập nhật mới đã được áp dụng thành công."

### Slide 52 (Tổng kết luồng)
"Để khép lại phần lý thuyết trước khi bước vào phần Demo trực tiếp trên 2 máy, chúng ta cùng nhìn lại toàn bộ bức tranh 6 bước của hệ thống:
1. Lập trình viên đẩy code mới và file migration lên GitHub Private.
2. Bấm Build Now trên Jenkins ở Máy A.
3. Jenkins tự động chạy qua các chốt chặn: Checkout code bằng khóa bảo mật -> Chạy Unit Test (nếu sai dừng ngay lập tức) -> Build Image gắn số tag mới -> Push Image lên Registry.
4. Bước Trigger Deploy: Jenkins gọi HTTP Webhook POST /deploy kèm số tag sang Máy B.
5. Máy B tự động kéo đúng tag image đó về, khởi chạy lại container và tự động chạy Migration.
6. Kết quả cuối cùng: Web API phiên bản mới hoạt động trơn tru với Database đã được đồng bộ hoàn toàn.

Ngay sau đây, nhóm xin phép chuyển sang phần Demo thực tế trên 2 máy bao gồm cả kịch bản cố tình làm sai Unit Test để kiểm chứng chốt chặn CI và live-coding thêm tính năng mới!"
