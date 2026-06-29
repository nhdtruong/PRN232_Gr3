# Attendance & Assessment API

Quản lý Điểm danh và Đánh giá nhận xét học viên.

## AttendanceController & LessonRollCallController

### Endpoint
`GET /api/lessons/{lessonId}/attendance` (hoặc `GET /api/lessons/{lessonId}/rollcall`)
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách điểm danh của lớp trong 1 buổi học cụ thể.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Danh sách trạng thái điểm danh của từng học sinh.
### Error Cases
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`POST /api/lessons/{lessonId}/attendance`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `AttendanceCreateDto`
### Purpose
Khởi tạo danh sách điểm danh cho buổi học (nếu chưa có).
### Roles
`Center`, `Teacher`
### Response
- `201 Created`: Khởi tạo thành công.
### Error Cases
- `400 Bad Request`: Đã khởi tạo điểm danh trước đó.

---

### Endpoint
`PUT /api/lessons/{lessonId}/rollcall`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `LessonRollCallBulkUpsertDto` (List của học viên và trạng thái: Có mặt, Vắng, Đi muộn).
### Purpose
Lưu hoặc cập nhật hàng loạt (Bulk) trạng thái điểm danh của cả lớp.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Lưu trạng thái điểm danh thành công.
### Error Cases
- `400 Bad Request`: Lỗi validation.
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`PUT /api/center/attendance/{attendanceId}`
### Query Parameters
`attendanceId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `AttendanceUpdateDto`
### Purpose
Cập nhật trạng thái điểm danh cho một sinh viên cụ thể.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Cập nhật thành công.
### Error Cases
- `404 Not Found`: Không tìm thấy record điểm danh.

---

### Endpoint
`GET /api/parent/children/{studentId}/attendance`
### Query Parameters
`studentId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Dành cho Phụ huynh: Xem lại lịch sử điểm danh của con (các buổi đã học).
### Roles
`Parent`
### Response
- `200 OK`: Danh sách lịch sử điểm danh.
### Error Cases
- `403 Forbidden`: Học viên không thuộc quyền quản lý.

---

## AssessmentController
Giáo viên/Trung tâm ghi chú, nhận xét năng lực của học viên.

### Endpoint
`GET /api/lessons/{lessonId}/assessment`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách nhận xét đánh giá của buổi học.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: `List<AssessmentResponseDto>`.
### Error Cases
- `404 Not Found`: Không tìm thấy buổi học.

---

### Endpoint
`PUT /api/lessons/{lessonId}/assessment`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `AssessmentUpsertDto`
### Purpose
Cập nhật hoặc thêm mới nhận xét đánh giá cho từng học viên sau buổi học.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Đánh giá thành công.
### Error Cases
- `400 Bad Request`: Lỗi validation dữ liệu.

---

### Endpoint
`GET /api/parent/children/{studentId}/assessment`
### Query Parameters
`studentId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Dành cho Phụ huynh: Xem lại lịch sử điểm số, đánh giá, nhận xét học tập của con (các buổi đã học và đã xuất bản).
### Roles
`Parent`
### Response
- `200 OK`: Danh sách lịch sử đánh giá nhận xét (`List<AssessmentResponseDto>`).
### Error Cases
- `401 Unauthorized`: Lỗi xác thực.
- `403 Forbidden`: Học viên không thuộc quyền quản lý của phụ huynh.
