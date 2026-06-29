# Classes API

## CenterClassController
Quản lý Lớp học và Học viên dành cho Trung tâm.

---

### Endpoint
`GET /api/center/classes`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách toàn bộ lớp học của trung tâm.
### Roles
`Center`
### Response
- `200 OK`: Danh sách các lớp học (`List<ClassResponseDto>`).
### Error Cases
- `401 Unauthorized`: Lỗi xác thực.
- `403 Forbidden`: Sai Role.

---

### Endpoint
`GET /api/center/classes/{classId}`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Xem chi tiết thông tin một lớp học.
### Roles
`Center`
### Response
- `200 OK`: `ClassResponseDto`.
### Error Cases
- `404 Not Found`: Không tìm thấy lớp học.

---

### Endpoint
`POST /api/center/classes`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `ClassCreateDto` (thông tin lớp học mới)
### Purpose
Tạo mới một lớp học.
### Roles
`Center`
### Response
- `201 Created`: Tạo thành công.
### Error Cases
- `400 Bad Request`: Dữ liệu gửi lên không hợp lệ.

---

### Endpoint
`PUT /api/center/classes/{classId}`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `ClassUpdateDto`
### Purpose
Cập nhật thông tin cơ bản của lớp học.
### Roles
`Center`
### Response
- `200 OK`: Cập nhật thành công.
### Error Cases
- `400 Bad Request`: Lỗi dữ liệu.
- `404 Not Found`: Lớp học không tồn tại.

---

### Endpoint
`PATCH /api/center/classes/{classId}/status`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `StatusUpdateDto` (ví dụ: Active, Inactive)
### Purpose
Cập nhật trạng thái lớp học.
### Roles
`Center`
### Response
- `200 OK`: Cập nhật trạng thái thành công.
### Error Cases
- `404 Not Found`: Không tìm thấy lớp.

---

### Endpoint
`GET /api/center/classes/{classId}/students`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách học viên trong một lớp.
### Roles
`Center`
### Response
- `200 OK`: Danh sách `StudentResponseDto`.
### Error Cases
- `404 Not Found`: Lớp không tồn tại.

---

### Endpoint
`POST /api/center/classes/{classId}/students`
### Query Parameters
`classId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `StudentId` hoặc danh sách ID học viên.
### Purpose
Thêm học viên vào lớp.
### Roles
`Center`
### Response
- `200 OK`: Đã thêm học viên vào lớp.
### Error Cases
- `400 Bad Request`: Học viên đã ở trong lớp.

---

### Endpoint
`DELETE /api/center/classes/{classId}/students/{studentId}`
### Query Parameters
`classId`, `studentId` (Path parameters)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Xóa/rút học viên khỏi lớp.
### Roles
`Center`
### Response
- `200 OK`: Đã xóa thành công.
### Error Cases
- `404 Not Found`: Không tìm thấy lớp hoặc học viên trong lớp này.

---

### Endpoint
`POST /api/center/students/{studentId}/transfer-class`
### Query Parameters
`studentId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `TransferClassDto` (chứa `FromClassId`, `ToClassId`)
### Purpose
Chuyển lớp cho một học viên.
### Roles
`Center`
### Response
- `200 OK`: Chuyển lớp thành công.
### Error Cases
- `400 Bad Request`: Không thể chuyển (lớp đích đã đầy, hoặc sai thông tin).

---

## ClassController
API chuẩn mực (CRUD) cho Lớp học (Thường ít dùng hơn CenterClassController).

### Endpoint
Các HTTP Methods tương ứng (`GET`, `POST`, `PUT/{id}`, `DELETE/{id}`) cho route `/api/Class`.
- Response, Request & Error Cases tương đồng với CenterClassController.

---

## ParentChildController / ParentController
API cho phép phụ huynh xem các lớp học của con mình.

### Endpoint
`GET /api/parent/my-children`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách các con mà phụ huynh đang quản lý.
### Roles
`Parent`
### Response
- `200 OK`: Danh sách học viên liên kết với tài khoản Parent.
### Error Cases
- `401 Unauthorized`.

---

### Endpoint
`GET /api/parent/children/{studentId}/classes`
### Query Parameters
`studentId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách các lớp con đang theo học.
### Roles
`Parent`
### Response
- `200 OK`: Danh sách `ClassResponseDto`.
### Error Cases
- `403 Forbidden`: Học viên không thuộc quản lý của Parent.
- `404 Not Found`: Không tìm thấy học viên.
