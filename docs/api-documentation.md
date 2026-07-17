EduBridge API Documentation
Tài liệu này tổng hợp toàn bộ các API endpoint hiện có trong hệ thống backend của dự án EduBridge (PRN232_Gr3). Các API này phục vụ cho ứng dụng Web Client (Razor Pages) và có thể sử dụng cho ứng dụng Mobile App/SPA thông qua JWT token (hoặc Cookie Auth).

1. Xác thực & Hồ sơ chung (Authentication & Universal Profile)
Quản lý đăng ký, đăng nhập, bảo mật và thông tin cá nhân dùng chung cho mọi Role. Nằm trong AuthController.

POST /api/Auth/login: Đăng nhập, trả về JWT Token.

POST /api/Auth/logout: Đăng xuất.

POST /api/Auth/change-password: Đổi mật khẩu cho người dùng hiện tại.

GET /api/Auth/profile: Lấy thông tin cá nhân của người dùng hiện tại (áp dụng cho mọi Role).

PUT /api/Auth/profile: Cập nhật thông tin cá nhân (áp dụng cho mọi Role).

2. Quản lý Lớp học (Center Class Management)
Dành cho Role Trung tâm (Center) để quản lý Lớp học và Học viên.

GET /api/center/classes: Lấy danh sách toàn bộ lớp học của trung tâm.

GET /api/center/classes/{classId}: Xem chi tiết một lớp học.

POST /api/center/classes: Tạo mới một lớp học.

PUT /api/center/classes/{classId}: Cập nhật thông tin cơ bản lớp học.

PATCH /api/center/classes/{classId}/status: Cập nhật trạng thái lớp học (ví dụ: Active, Inactive).

GET /api/center/classes/{classId}/students: Lấy danh sách học viên trong một lớp.

POST /api/center/classes/{classId}/students: Thêm học viên vào lớp.

DELETE /api/center/classes/{classId}/students/{studentId}: Xóa/rút học viên khỏi lớp.

POST /api/center/students/{studentId}/transfer-class: Chuyển lớp cho học viên.

**Dành cho Role Giáo viên (Teacher):**

GET /api/Class/my-classes: Lấy danh sách các lớp học được phân công và đang hoạt động (Active).

GET /api/Class/other-teachers: Lấy danh sách các giáo viên khác để phục vụ đơn xin đổi lớp.

3. Quản lý Buổi học (Lessons)
Quản lý lịch học chi tiết của từng Lớp học.

GET /api/center/classes/{classId}/lessons: Lấy toàn bộ buổi học của một lớp.

POST /api/center/classes/{classId}/lessons: Tạo mới một buổi học cho lớp.

GET /api/center/lessons/{lessonId}: Xem chi tiết một buổi học cụ thể.

PUT /api/center/lessons/{lessonId}: Cập nhật nội dung/thời gian buổi học.

DELETE /api/center/lessons/{lessonId}: Xóa buổi học.

POST /api/center/lessons/{lessonId}/publish: Publish buổi học để học viên và phụ huynh có thể nhìn thấy.

4. Tài liệu bài giảng (Materials)
Quản lý tài liệu đính kèm cho từng buổi học (Slide, Bài tập, v.v.).

GET /api/center/lessons/{lessonId}/materials: Lấy danh sách tài liệu của buổi học.

POST /api/center/lessons/{lessonId}/materials: Thêm metadata tài liệu vào buổi học.

POST /api/center/materials/upload-file: Upload file vật lý lên server.

DELETE /api/center/materials/{materialId}: Xóa tài liệu khỏi hệ thống.

5. Điểm danh (Attendance & Roll Call)
Quản lý và ghi nhận chuyên cần của học viên.

GET /api/lessons/{lessonId}/attendance hoặc GET /api/lessons/{lessonId}/rollcall: Lấy danh sách điểm danh của lớp trong 1 buổi học.

POST /api/lessons/{lessonId}/attendance: Khởi tạo danh sách điểm danh cho buổi học.

PUT /api/lessons/{lessonId}/rollcall: Lưu/Cập nhật hàng loạt (Bulk) trạng thái điểm danh của cả lớp.

PUT /api/center/attendance/{attendanceId}: Cập nhật trạng thái điểm danh cho một sinh viên cụ thể.

6. Đánh giá / Điểm số (Assessment & Grades)
Giáo viên/Trung tâm nhập điểm và nhận xét cho học viên (Tách biệt theo buổi và theo kỳ).

POST /api/classes/{classId}/gradesheet: Lưu điểm thường xuyên (TX) và nhận xét theo từng buổi học.

POST /api/classes/{classId}/transcript: Lưu điểm định kỳ (Giữa kỳ, Cuối kỳ) và tự tính điểm Tổng kết cho cả lớp.

7. Chức năng Phụ huynh (Parent Features)
Các API truy xuất thông tin dành riêng cho Phụ huynh.

GET /api/parent/my-children: Lấy danh sách các con mà phụ huynh đang quản lý.

GET /api/parent/children/{studentId}/classes: Lấy danh sách các lớp con đang theo học.

GET /api/parent/children/{studentId}/lessons: Lấy lịch học/các buổi học của con.

GET /api/parent/children/{studentId}/attendance: Lấy lịch sử điểm danh của con.

8. Nhắn tin (Chat)
Quản lý kênh giao tiếp giữa Trung tâm và Phụ huynh.

GET /api/Chat/channels: Lấy danh sách các kênh nhắn tin (Chat rooms) của người dùng hiện tại.

GET /api/Chat/channels/{channelId}/messages: Lấy danh sách tin nhắn trong một kênh cụ thể.

POST /api/Chat/upload: Upload file/ảnh trong khung chat.

9. Thông báo (Notifications)
Gửi và hiển thị thông báo thời gian thực (Real-time).

GET /api/notifications: Lấy danh sách thông báo cá nhân của user.

PUT /api/notifications/mark-all-read: Đánh dấu đã đọc tất cả thông báo.

PUT /api/notifications/{id}/mark-read: Đánh dấu đã đọc một thông báo cụ thể.

10. Quản lý Giáo viên (Teachers)
Dành cho Role Trung tâm (Center) quản lý danh sách giáo viên.

GET /api/center/teachers: Lấy danh sách giáo viên.

GET /api/center/teachers/{id}: Xem chi tiết giáo viên.

POST /api/center/teachers: Tạo mới tài khoản giáo viên.

PUT /api/center/teachers/{id}: Cập nhật thông tin giáo viên.

POST /api/center/teachers/{id}/toggle-status: Khóa/Mở khóa tài khoản giáo viên.

11. Quản lý Học viên độc lập (Students)
Dành cho Role Trung tâm (Center) quản lý hồ sơ học viên trên toàn hệ thống.

GET /api/center/students: Lấy danh sách tất cả học viên.

GET /api/center/students/{id}: Xem chi tiết học viên.

POST /api/center/students: Tạo mới hồ sơ học viên.

PUT /api/center/students/{id}: Cập nhật hồ sơ học viên.

DELETE /api/center/students/{id}: Xóa hồ sơ học viên.

12. Quản lý Môn học (Subjects)
Dành cho Role Trung tâm (Center).

GET /api/center/subjects: Lấy danh sách môn học.

GET /api/center/subjects/{id}: Xem chi tiết môn học.

POST /api/center/subjects: Tạo mới môn học.

PUT /api/center/subjects/{id}: Cập nhật môn học.

DELETE /api/center/subjects/{id}: Xóa môn học.

13. Yêu cầu Chuyển lớp (Class Transfer)
Quản lý các luồng yêu cầu xin chuyển lớp.

POST /api/ClassTransfer: Tạo yêu cầu chuyển lớp.

GET /api/ClassTransfer/teacher/{teacherId}: Lấy danh sách yêu cầu liên quan đến một giáo viên.

GET /api/ClassTransfer/pending: Lấy danh sách các yêu cầu đang chờ xử lý.

PUT /api/ClassTransfer/{id}/process: Duyệt/Từ chối yêu cầu chuyển lớp.

14. Các API khác
GET /WeatherForecast: (Mặc định) API template kiểm tra hệ thống.