# OnThiLaiXe
Web hỗ trợ luyện thi lái xe 
# Web luyện thi bằng lái xe
Đây là một dự án trang web hỗ trợ người dùng ôn luyện thi sát hạch bằng lái xe máy và ô tô, bao gồm các loại bằng như A1, A2, B1, B2,...
Trang web cung cấp các đề thi thử, ngân hàng câu hỏi đầy đủ và tính năng chấm điểm, thống kê kết quả cho người dùng. Hệ thống được xây dựng bằng ASP.NET Core  kết hợp MVC.

## Các chức năng chính

- Làm bài thi thử gồm 25 câu hỏi mỗi lần, bám sát cấu trúc đề thật
- Hệ thống câu hỏi được phân loại theo từng chủ đề
- Gợi ý câu hỏi ôn tập dựa trên các câu đã làm sai
- Cho phép làm bài không cần đăng nhập
- Người dùng có thể đăng nhập để lưu kết quả và theo dõi tiến độ học
- Giáo viên có thể xem thống kê kết quả học viên
- Quản trị viên có thể thêm, sửa, xóa câu hỏi và quản lý người dùng
- Gửi email nhắc nhở hoặc thống kê kết quả bằng hệ thống nền (Hangfire + SendGrid)

## Công nghệ sử dụng

- ASP.NET Core 8 (Web API + Razor Pages)
- Entity Framework Core (Code First)
- SQL Server
- JWT (Xác thực người dùng)
- Hangfire (Xử lý nền)
- SendGrid (Gửi email)
- HTML, CSS, Bootstrap 

## Hướng dẫn cài đặt
1. Clone dự án:
   git clone https://github.com/ABVDR/OnThiLaiXe.git

2. Cấu hình cơ sở dữ liệu và API:
   - Mở file appsettings.json
   - Cập nhật chuỗi kết nối đến SQL Server và API key của SendGrid của bạn
3. Tạo database:
   dotnet ef database update

4. Chạy dự án:
   dotnet run

## Mục tiêu dự án
- Giúp người học ôn thi bằng lái dễ dàng và chủ động
- Tạo công cụ luyện tập đơn giản, hiệu quả
- Hỗ trợ giáo viên và trung tâm đào tạo trong việc theo dõi học viên


