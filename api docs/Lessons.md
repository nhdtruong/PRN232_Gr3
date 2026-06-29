# Lessons API

Quản lý lịch học chi tiết của từng Lớp học. Mặc định các API này nằm trong **`LessonController`**.

---

### Endpoint
`GET /api/center/classes/{classId}/lessons`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy toàn bộ buổi học của một lớp cụ thể.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Danh sách buổi học (`List<LessonResponseDto>`).
### Error Cases
- `404 Not Found`: Không tìm thấy lớp học.

---

### Endpoint
`POST /api/center/classes/{classId}/lessons`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `LessonCreateDto`
### Purpose
Tạo mới một buổi học (hoặc lịch học) cho lớp.
### Roles
`Center`
### Response
- `201 Created`: Tạo thành công.
### Error Cases
- `400 Bad Request`: Lỗi dữ liệu tạo buổi học.

---

### Endpoint
`GET /api/center/lessons/{lessonId}`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Xem chi tiết một buổi học cụ thể.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Trả về `LessonResponseDto`.
### Error Cases
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`PUT /api/center/lessons/{lessonId}`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `LessonUpdateDto`
### Purpose
Cập nhật nội dung/thời gian buổi học.
### Roles
`Center`
### Response
- `200 OK`: Cập nhật thành công.
### Error Cases
- `400 Bad Request`: Thời gian/Dữ liệu không hợp lệ.
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`DELETE /api/center/lessons/{lessonId}`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Xóa một buổi học khỏi hệ thống.
### Roles
`Center`
### Response
- `200 OK`: Xóa thành công.
### Error Cases
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`POST /api/center/lessons/{lessonId}/publish`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Publish buổi học (chuyển trạng thái sang public) để học viên và phụ huynh có thể nhìn thấy lịch học.
### Roles
`Center`
### Response
- `200 OK`: Trạng thái đã được publish.
### Error Cases
- `404 Not Found`: Buổi học không tồn tại.

---

### Endpoint
`GET /api/parent/children/{studentId}/lessons`
### Query Parameters
`studentId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Dành riêng cho Parent: Lấy lịch học/các buổi học sắp tới của con mình.
### Roles
`Parent`
### Response
- `200 OK`: Danh sách lịch học.
### Error Cases
- `403 Forbidden`: Học viên không thuộc quản lý của Parent.
- `404 Not Found`: Không tìm thấy học sinh.
