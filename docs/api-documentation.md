# 📚 EduBridge API Documentation

Dự án: **EduBridge (PRN232_Gr3)**  
Mô tả: Tài liệu tổng hợp toàn bộ các API Endpoint thuộc hệ thống Backend EduBridge. Phục vụ cho Web Client (Razor Pages) và Mobile App/SPA (sử dụng JWT Token hoặc Cookie Auth).

---

## 📌 Danh Mục API (Table of Contents)
1. [Xác thực & Hồ sơ chung](#1-x%C3%A1c-th%E1%BB%B1c--h%E1%BB%93-s%C6%A1-chung-authentication--universal-profile)
2. [Quản lý Lớp học](#2-qu%E1%BA%A3n-l%C3%BD-l%E1%BB%9Bp-h%E1%BB%8Dc-class-management)
3. [Quản lý Buổi học](#3-qu%E1%BA%A3n-l%C3%BD-bu%E1%BB%95i-h%E1%BB%8Dc-lessons)
4. [Tài liệu bài giảng](#4-t%C3%A0i-li%E1%BB%87u-b%C3%A0i-gi%E1%BA%A3ng-materials)
5. [Điểm danh](#5-%C4%91i%E1%BB%83m-danh-attendance--roll-call)
6. [Đánh giá & Điểm số](#6-%C4%91%C3%A1nh-gi%C3%A1--%C4%91i%E1%BB%83m-s%E1%BB%91-assessment--grades)
7. [Chức năng Phụ huynh](#7-ch%E1%BB%A9c-n%C4%83ng-ph%E1%BB%A5-huynh-parent-features)
8. [Nhắn tin](#8-nh%E1%BA%AFn-tin-chat)
9. [Thông báo](#9-th%C3%B4ng-b%C3%A1o-notifications)
10. [Quản lý Giáo viên](#10-qu%E1%BA%A3n-l%C3%BD-gi%C3%A1o-vi%C3%Aan-teachers)
11. [Quản lý Phụ huynh](#11-qu%E1%BA%A3n-l%C3%BD-ph%E1%BB%A5-huynh-parents)
12. [Quản lý Học viên độc lập](#12-qu%E1%BA%A3n-l%C3%BD-h%E1%BB%8Dc-vi%C3%Aan-%C4%91%E1%BB%99c-l%E1%BA%ADp-students)
13. [Quản lý Môn học](#13-qu%E1%BA%A3n-l%C3%BD-m%C3%B4n-h%E1%BB%8Dc-subjects)
14. [Yêu cầu Chuyển lớp](#14-y%C3%AAu-c%E1%BA%A7u-chuy%E1%BB%83n-l%E1%BB%9Bp-class-transfer)
15. [Lịch sử Cập nhật & Kiểm tra](#15-b%E1%BA%A3ng-t me-t%E1%BB%95ng-h%E1%BB%A3p-c%C3%A1c-%C4%91i%E1%BB%83m-thi%E1%BA%BFu--sai-%C4%91%C3%A3-%C4%91%C6%B0%E1%BB%A3c-ki%E1%BB%83m-tra-v%C3%A0-s%E1%BB%ADa-%C4%91%E1%BB me-chong)

---

## 1. Xác thực & Hồ sơ chung (Authentication & Universal Profile)
**Controller:** `AuthController`  
Quản lý đăng nhập, bảo mật và thông tin cá nhân dùng chung cho mọi Role. *(Lưu ý: Hệ thống không mở đăng ký công khai; toàn bộ tài khoản do Trung tâm/Admin khởi tạo).*

| HTTP Method | Endpoint Path | Chức năng | Role / Ghi chú |
|---|---|---|---|
| `POST` | `/api/Auth/login` | Đăng nhập hệ thống | Trả về JWT Token (Mobile/SPA) & Cookie Auth (Web) |
| `POST` | `/api/Auth/logout` | Đăng xuất | Hủy phiên Cookie |
| `GET` | `/api/Auth/profile` | Lấy thông tin cá nhân của user hiện tại | All Roles |
| `PUT` | `/api/Auth/profile` | Cập nhật thông tin cá nhân | FullName, Email, Phone |
| `POST` | `/api/Auth/change-password` | Đổi mật khẩu người dùng hiện tại | Authenticated Users |

---

## 2. Quản lý Lớp học (Class Management)

### 🏫 Dành cho Role Trung tâm (Center)
**Controller:** `CenterClassController`

| HTTP Method | Endpoint Path | Chức năng |
|---|---|---|
| `GET` | `/api/center/classes` | Lấy danh sách toàn bộ lớp học của trung tâm |
| `GET` | `/api/center/classes/{classId}` | Xem chi tiết một lớp học |
| `POST` | `/api/center/classes` | Tạo mới lớp học (Tự động sinh lịch học theo Ca/Ngày) |
| `PUT` | `/api/center/classes/{classId}` | Cập nhật thông tin và lịch học của lớp |
| `DELETE` | `/api/center/classes/{classId}` | Xóa lớp học khỏi hệ thống |
| `PATCH` | `/api/center/classes/{classId}/status` | Cập nhật trạng thái lớp học (ví dụ: Active, Inactive) |
| `GET` | `/api/center/classes/{classId}/schedules` | Lấy danh sách chi tiết các ca học / lịch học |
| `GET` | `/api/center/classes/{classId}/students` | Lấy danh sách học viên trong một lớp |
| `POST` | `/api/center/classes/{classId}/students` | Xếp học viên đã có sẵn vào lớp |
| `POST` | `/api/center/classes/{classId}/students/create-and-enroll` | Tạo nhanh Học sinh (+Phụ huynh) và xếp thẳng vào lớp |
| `DELETE` | `/api/center/classes/{classId}/students/{studentId}` | Xóa / rút học viên khỏi lớp |
| `POST` | `/api/center/students/{studentId}/transfer-class` | Chuyển lớp cho học viên |

### 👨‍🏫 Dành cho Role Giáo viên (Teacher) & CRUD cơ bản
**Controller:** `ClassController`

| HTTP Method | Endpoint Path | Chức năng | Role / Ghi chú |
|---|---|---|---|
| `GET` | `/api/Class/my-classes` | Lấy danh sách lớp được phân công (Active) | Teacher |
| `GET` | `/api/Class/other-teachers` | Lấy danh sách giáo viên khác (phục vụ đơn xin đổi lớp) | Teacher |
| `GET` | `/api/Class` | Lấy toàn bộ danh sách lớp học | Authenticated Users (CRUD gốc) |
| `GET` | `/api/Class/{id}` | Lấy chi tiết thông tin lớp học theo ID | Authenticated Users (CRUD gốc) |
| `POST` | `/api/Class` | Tạo mới lớp học | Center (CRUD gốc) |
| `PUT` | `/api/Class/{id}` | Cập nhật thông tin lớp học | Center (CRUD gốc) |
| `DELETE` | `/api/Class/{id}` | Xóa lớp học theo ID | Center (CRUD gốc) |

---

## 3. Quản lý Buổi học (Lessons)
**Controller:** `LessonController`  
Quản lý lịch học chi tiết của từng Lớp học.

| HTTP Method | Endpoint Path | Chức năng | Role Access |
|---|---|---|---|
| `GET` | `/api/center/classes/{classId}/lessons` | Lấy toàn bộ danh sách buổi học của một lớp | Teacher |
| `POST` | `/api/center/classes/{classId}/lessons` | Tạo mới một buổi học cho lớp | Teacher |
| `GET` | `/api/center/lessons/{lessonId}` | Xem chi tiết một buổi học cụ thể | Teacher |
| `PUT` | `/api/center/lessons/{lessonId}` | Cập nhật nội dung, thời gian, phòng học | Teacher |
| `DELETE` | `/api/center/lessons/{lessonId}` | Xóa / hủy buổi học | Teacher |
| `POST` | `/api/center/lessons/{lessonId}/publish` | Xuất bản buổi học & tự động bắn thông báo SignalR | Teacher |
| `GET` | `/api/parent/children/{studentId}/lessons` | Xem lịch học / buổi học của con | Parent |

---

## 4. Tài liệu bài giảng (Materials)
**Controller:** `MaterialController`  
Quản lý tài liệu đính kèm cho từng buổi học (Slide, Bài tập, File đính kèm,...).

| HTTP Method | Endpoint Path | Chức năng | Role / Ghi chú |
|---|---|---|---|
| `GET` | `/api/center/lessons/{lessonId}/materials` | Lấy danh sách tài liệu đính kèm của một buổi học | Authenticated Users |
| `POST` | `/api/center/lessons/{lessonId}/materials` | Thêm liên kết / metadata tài liệu vào buổi học | Teacher |
| `POST` | `/api/center/materials/upload-file` | Upload file vật lý lên server | Teacher (PDF, Word, Excel, PNG... Max 20MB) |
| `DELETE` | `/api/center/materials/{materialId}` | Xóa tài liệu khỏi hệ thống | Teacher |

---

## 5. Điểm danh (Attendance & Roll Call)
**Controllers:** `AttendanceController`, `LessonRollCallController`  
Quản lý và ghi nhận chuyên cần của học viên.

| HTTP Method | Endpoint Path | Chức năng | Controller / Role |
|---|---|---|---|
| `GET` | `/api/lessons/{lessonId}/attendance` | Lấy danh sách điểm danh của lớp trong 1 buổi | AttendanceController (Authenticated) |
| `POST` | `/api/lessons/{lessonId}/attendance` | Khởi tạo / lưu danh sách điểm danh hàng loạt (Bulk) | Center (AttendanceController) |
| `PUT` | `/api/center/attendance/{attendanceId}` | Cập nhật trạng thái điểm danh cho 1 sinh viên | Center (AttendanceController) |
| `GET` | `/api/lessons/{lessonId}/rollcall` | Lấy danh sách điểm danh và nhận xét của buổi học | Teacher (LessonRollCallController) |
| `PUT` | `/api/lessons/{lessonId}/rollcall` | Lưu / Cập nhật hàng loạt điểm danh & nhận xét cả lớp | Teacher (LessonRollCallController) |

---

## 6. Đánh giá & Điểm số (Assessment & Grades)
**Controllers:** `AssessmentController`, `ClassGradeSheetController`  
Nhập điểm và nhận xét cho học viên (tách biệt theo từng buổi và theo kỳ).

| HTTP Method | Endpoint Path | Chức năng | Controller / Role |
|---|---|---|---|
| `GET` | `/api/lessons/{lessonId}/assessment` | Lấy danh sách đánh giá / nhận xét của buổi học | AssessmentController (Authenticated) |
| `PUT` | `/api/lessons/{lessonId}/assessment` | Lưu/cập nhật hàng loạt (Bulk) đánh giá buổi học | Center (AssessmentController) |
| `POST` | `/api/classes/{classId}/gradesheet` | Lưu điểm thường xuyên (TX) & bắn thông báo phụ huynh | Teacher (ClassGradeSheetController) |
| `POST` | `/api/classes/{classId}/transcript` | Lưu điểm định kỳ (Giữa kỳ, Cuối kỳ) & Tự tính điểm TB | Teacher (ClassGradeSheetController) |

---

## 7. Chức năng Phụ huynh (Parent Features)
Các API truy xuất thông tin dành riêng cho Phụ huynh.

| HTTP Method | Endpoint Path | Chức năng | Controller |
|---|---|---|---|
| `GET` | `/api/parent/my-children` | Lấy danh sách các con đang quản lý | ParentChildController |
| `GET` | `/api/parent/children/{studentId}/classes` | Lấy danh sách các lớp con đang theo học | ParentController |
| `GET` | `/api/parent/children/{studentId}/lessons` | Lấy lịch học / các buổi học của con | LessonController |
| `GET` | `/api/parent/children/{studentId}/attendance` | Lấy lịch sử điểm danh của con | AttendanceController |
| `GET` | `/api/parent/children/{studentId}/assessment` | Lấy lịch sử đánh giá & nhận xét của con | AssessmentController |

---

## 8. Nhắn tin (Chat)
**Controller:** `ChatController`  
Quản lý kênh giao tiếp thời gian thực giữa Trung tâm, Giáo viên và Phụ huynh.

| HTTP Method | Endpoint Path | Chức năng | Ghi chú |
|---|---|---|---|
| `GET` | `/api/Chat/channels` | Lấy danh sách các kênh nhắn tin (Chat rooms) | User hiện tại |
| `GET` | `/api/Chat/channels/{channelId}/messages` | Lấy danh sách tin nhắn trong một kênh | Hỗ trợ phân trang/limit=50 |
| `POST` | `/api/Chat/upload` | Upload file / ảnh / video đính kèm | Max file size: 25MB |

---

## 9. Thông báo (Notifications)
**Controller:** `NotificationController`  
Gửi và hiển thị thông báo thời gian thực (Real-time).

| HTTP Method | Endpoint Path | Chức năng | Ghi chú |
|---|---|---|---|
| `GET` | `/api/notifications` | Lấy danh sách thông báo cá nhân | Mặc định lấy 20 thông báo mới nhất |
| `PUT` | `/api/notifications/mark-all-read` | Đánh dấu đã đọc tất cả thông báo | Role Parent, Center, Teacher |
| `PUT` | `/api/notifications/{id}/mark-read` | Đánh dấu đã đọc một thông báo cụ thể | Theo Notification ID (`{id:int}`) |

---

## 10. Quản lý Giáo viên (Teachers)
**Controller:** `TeacherController`  
Dành cho Role Trung tâm (Center) quản lý danh sách và tài khoản giáo viên.

| HTTP Method | Endpoint Path | Chức năng | Role |
|---|---|---|---|
| `GET` | `/api/center/teachers` | Lấy danh sách toàn bộ giáo viên | Center |
| `GET` | `/api/center/teachers/{id}` | Xem chi tiết thông tin giáo viên | Center |
| `POST` | `/api/center/teachers` | Tạo mới tài khoản giáo viên | Center |
| `PUT` | `/api/center/teachers/{id}` | Cập nhật thông tin giáo viên | Center |
| `POST` | `/api/center/teachers/{id}/toggle-status` | Khóa / Mở khóa tài khoản giáo viên | Center |

---

## 11. Quản lý Phụ huynh (Parents)
**Controller:** `ParentController`  
Dành cho Role Trung tâm (Center) quản lý danh sách và tài khoản phụ huynh.

| HTTP Method | Endpoint Path | Chức năng | Ghi chú |
|---|---|---|---|
| `GET` | `/api/center/parents` | Lấy danh sách tất cả phụ huynh | Hỗ trợ query `?classId=` để lọc theo lớp |
| `GET` | `/api/center/parents/{id}` | Xem chi tiết thông tin tài khoản phụ huynh | Role Center |
| `POST` | `/api/center/parents` | Tạo mới tài khoản phụ huynh | Role Center |
| `PUT` | `/api/center/parents/{id}` | Cập nhật thông tin tài khoản phụ huynh | Role Center |
| `POST` | `/api/center/parents/{id}/toggle-status` | Khóa / Mở khóa tài khoản phụ huynh | Role Center |

---

## 12. Quản lý Học viên độc lập (Students)
**Controller:** `StudentController`  
Dành cho Role Trung tâm (Center) quản lý hồ sơ học viên trên toàn hệ thống.

| HTTP Method | Endpoint Path | Chức năng | Role |
|---|---|---|---|
| `GET` | `/api/center/students` | Lấy danh sách tất cả học viên | Center |
| `GET` | `/api/center/students/{id}` | Xem chi tiết hồ sơ học viên | Center |
| `POST` | `/api/center/students` | Tạo mới hồ sơ học viên | Center |
| `PUT` | `/api/center/students/{id}` | Cập nhật hồ sơ học viên | Center |
| `DELETE` | `/api/center/students/{id}` | Xóa hồ sơ học viên | Center |

---

## 13. Quản lý Môn học (Subjects)
**Controller:** `SubjectController`  
Dành cho Role Trung tâm (Center) quản lý danh mục môn học.

| HTTP Method | Endpoint Path | Chức năng | Role / Route Param |
|---|---|---|---|
| `GET` | `/api/center/subjects` | Lấy danh sách môn học | Center |
| `GET` | `/api/center/subjects/{subjectId}` | Xem chi tiết môn học | Center (`{subjectId}`) |
| `POST` | `/api/center/subjects` | Tạo mới môn học | Center |
| `PUT` | `/api/center/subjects/{subjectId}` | Cập nhật thông tin môn học | Center (`{subjectId}`) |
| `DELETE` | `/api/center/subjects/{subjectId}` | Xóa môn học (và các tài liệu liên quan) | Center (`{subjectId}`) |

---

## 14. Yêu cầu Chuyển lớp (Class Transfer)
**Controller:** `ClassTransferController`  
Quản lý các luồng yêu cầu xin chuyển lớp từ Giáo viên.

| HTTP Method | Endpoint Path | Chức năng | Role |
|---|---|---|---|
| `POST` | `/api/ClassTransfer` | Tạo yêu cầu chuyển lớp | Authenticated |
| `GET` | `/api/ClassTransfer/teacher/{teacherId}` | Lấy danh sách yêu cầu liên quan đến một giáo viên | Authenticated |
| `GET` | `/api/ClassTransfer/pending` | Lấy danh sách các yêu cầu đang chờ xử lý | Authenticated |
| `PUT` | `/api/ClassTransfer/{id}/process` | Phê duyệt hoặc từ chối yêu cầu chuyển lớp | Authenticated |

---

## 15. Bảng tổng hợp các điểm Thiếu / Sai đã được kiểm tra và sửa đổi (Audit Changelog)

| STT | Mục / Controller | Endpoint Path | Trạng thái cũ | Cập nhật / Sửa đổi trong tài liệu |
|---|---|---|---|---|
| 1 | **Xác thực** (`AuthController`) | `POST /api/Auth/register` | Thừa / Không dùng | Loại bỏ API đăng ký tự do (Nghiệp vụ: Admin/Center quản lý tạo tài khoản). |
| 2 | **Lớp học** (`CenterClassController`) | `DELETE /api/center/classes/{classId}` | Thiếu | Bổ sung API xóa lớp học dành cho Role Center. |
| 3 | **Lớp học** (`CenterClassController`) | `GET /api/center/classes/{classId}/schedules` | Thiếu | Bổ sung API xem danh sách ca học / lịch học chi tiết của lớp. |
| 4 | **Lớp học** (`CenterClassController`) | `POST /api/center/classes/{classId}/students/create-and-enroll` | Thiếu | Bổ sung API tạo nhanh học sinh + phụ huynh và xếp thẳng vào lớp. |
| 5 | **Lớp học** (`ClassController`) | `GET/POST/PUT/DELETE /api/Class/...` | Thiếu | Bổ sung bộ API CRUD lớp học cơ bản của `ClassController`. |
| 6 | **Đánh giá** (`AssessmentController`) | `GET /api/lessons/{lessonId}/assessment` | Thiếu | Bổ sung API lấy đánh giá nhận xét buổi học. |
| 7 | **Đánh giá** (`AssessmentController`) | `PUT /api/lessons/{lessonId}/assessment` | Thiếu | Bổ sung API lưu/cập nhật hàng loạt đánh giá buổi học (Role Center). |
| 8 | **Phụ huynh** (`AssessmentController`) | `GET /api/parent/children/{studentId}/assessment` | Thiếu | Bổ sung API cho Phụ huynh xem lịch sử đánh giá nhận xét của con. |
| 9 | **Quản lý Phụ huynh** (`ParentController`) | `GET/POST/PUT /api/center/parents/...` | Thiếu cả mục | Bổ sung **Mục 11: Quản lý Phụ huynh (Parents)** cho Role Center (xem, tạo, sửa, toggle-status, lọc theo classId). |
| 10 | **Môn học** (`SubjectController`) | `{id}` ➔ `{subjectId}` | Sai tham số | Điều chỉnh route parameter chính xác theo controller (`{subjectId}`). |
| 11 | **API Thừa** | `GET /WeatherForecast` | Thừa | Loại bỏ khỏi tài liệu vì hệ thống thực tế không có controller này. |