# EduBridge API Documentation

Tài liệu này tổng hợp toàn bộ các API endpoint hiện có trong hệ thống backend của dự án EduBridge (PRN232_Gr3). Các API này phục vụ cho ứng dụng Web Client (Razor Pages) và có thể sử dụng cho ứng dụng Mobile App/SPA thông qua JWT token (hoặc Cookie Auth).

## 1. Xác thực (Authentication)
Quản lý đăng ký, đăng nhập và bảo mật người dùng. Mặc định các API này nằm trong **`AuthController`**.
- `POST /api/Auth/register`: Đăng ký tài khoản mới.
- `POST /api/Auth/login`: Đăng nhập, trả về JWT Token.
- `POST /api/Auth/logout`: Đăng xuất.
- `POST /api/Auth/change-password`: Đổi mật khẩu cho người dùng hiện tại (yêu cầu gửi Mật khẩu cũ & Mật khẩu mới).

## 2. Thông tin cá nhân (Profile)
Quản lý thông tin hồ sơ của người dùng (hiện tại tập trung vào Phụ huynh).
- `GET /api/parent-profile`: Lấy thông tin cá nhân của phụ huynh hiện tại.
- `PUT /api/parent-profile`: Cập nhật thông tin (Họ tên, Email, Số điện thoại).

## 3. Quản lý Lớp học (Center Class Management)
Dành cho Role Trung tâm (Center) để quản lý Lớp học và Học viên.
- `GET /api/center/classes`: Lấy danh sách toàn bộ lớp học của trung tâm.
- `GET /api/center/classes/{classId}`: Xem chi tiết một lớp học.
- `POST /api/center/classes`: Tạo mới một lớp học.
- `PUT /api/center/classes/{classId}`: Cập nhật thông tin cơ bản lớp học.
- `PATCH /api/center/classes/{classId}/status`: Cập nhật trạng thái lớp học (ví dụ: Active, Inactive).
- `GET /api/center/classes/{classId}/students`: Lấy danh sách học viên trong một lớp.
- `POST /api/center/classes/{classId}/students`: Thêm học viên vào lớp.
- `DELETE /api/center/classes/{classId}/students/{studentId}`: Xóa/rút học viên khỏi lớp.
- `POST /api/center/students/{studentId}/transfer-class`: Chuyển lớp cho học viên.

*(Lưu ý: Hệ thống cũng có một `ClassController` sinh sẵn tự động với các route `GET/POST/PUT/DELETE /api/Class` nhưng các API chính nên sử dụng là CenterClassController ở trên).*

## 4. Quản lý Buổi học (Lessons)
Quản lý lịch học chi tiết của từng Lớp học.
- `GET /api/center/classes/{classId}/lessons`: Lấy toàn bộ buổi học của một lớp.
- `POST /api/center/classes/{classId}/lessons`: Tạo mới một buổi học cho lớp.
- `GET /api/center/lessons/{lessonId}`: Xem chi tiết một buổi học cụ thể.
- `PUT /api/center/lessons/{lessonId}`: Cập nhật nội dung/thời gian buổi học.
- `DELETE /api/center/lessons/{lessonId}`: Xóa buổi học.
- `POST /api/center/lessons/{lessonId}/publish`: Publish buổi học để học viên và phụ huynh có thể nhìn thấy.

## 5. Tài liệu bài giảng (Materials)
Quản lý tài liệu đính kèm cho từng buổi học (Slide, Bài tập, v.v.).
- `GET /api/center/lessons/{lessonId}/materials`: Lấy danh sách tài liệu của buổi học.
- `POST /api/center/lessons/{lessonId}/materials`: Thêm metadata tài liệu vào buổi học.
- `POST /api/center/materials/upload-file`: Upload file vật lý lên server.
- `DELETE /api/center/materials/{materialId}`: Xóa tài liệu khỏi hệ thống.

## 6. Điểm danh (Attendance & Roll Call)
Quản lý và ghi nhận chuyên cần của học viên.
- `GET /api/lessons/{lessonId}/attendance` hoặc `GET /api/lessons/{lessonId}/rollcall`: Lấy danh sách điểm danh của lớp trong 1 buổi học.
- `POST /api/lessons/{lessonId}/attendance`: Khởi tạo danh sách điểm danh cho buổi học.
- `PUT /api/lessons/{lessonId}/rollcall`: Lưu/Cập nhật hàng loạt (Bulk) trạng thái điểm danh của cả lớp.
- `PUT /api/center/attendance/{attendanceId}`: Cập nhật trạng thái điểm danh cho một sinh viên cụ thể.

## 7. Đánh giá / Nhận xét (Assessment)
Giáo viên/Trung tâm ghi chú, nhận xét năng lực của học viên trong buổi học.
- `GET /api/lessons/{lessonId}/assessment`: Lấy danh sách nhận xét đánh giá của buổi học.
- `PUT /api/lessons/{lessonId}/assessment`: Cập nhật/Thêm mới nhận xét đánh giá cho học viên.

## 8. Chức năng Phụ huynh (Parent Features)
Các API truy xuất thông tin dành riêng cho Phụ huynh.
- `GET /api/parent/my-children`: Lấy danh sách các con mà phụ huynh đang quản lý.
- `GET /api/parent/children/{studentId}/classes`: Lấy danh sách các lớp con đang theo học.
- `GET /api/parent/children/{studentId}/lessons`: Lấy lịch học/các buổi học của con.
- `GET /api/parent/children/{studentId}/attendance`: Lấy lịch sử điểm danh của con.
- `GET /api/parent/children/{studentId}/assessment`: Lấy lịch sử đánh giá học tập/nhận xét của con.

## 9. Nhắn tin (Chat)
Quản lý kênh giao tiếp giữa Trung tâm và Phụ huynh.
- `GET /api/Chat/channels`: Lấy danh sách các kênh nhắn tin (Chat rooms) của người dùng hiện tại.
- `GET /api/Chat/channels/{channelId}/messages`: Lấy danh sách tin nhắn trong một kênh cụ thể.
- `POST /api/Chat/upload`: Upload file/ảnh trong khung chat.

## 10. Thông báo (Notifications)
Gửi và hiển thị thông báo thời gian thực (Real-time).
- `GET /api/notifications`: Lấy danh sách thông báo cá nhân của user.
- `PUT /api/notifications/mark-all-read`: Đánh dấu đã đọc tất cả thông báo.
- `PUT /api/notifications/{id}/mark-read`: Đánh dấu đã đọc một thông báo cụ thể.

## 11. Các API khác
- `GET /WeatherForecast`: (Mặc định) API template kiểm tra hệ thống.
