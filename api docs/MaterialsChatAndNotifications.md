# Materials, Chat & Notifications API

Tài liệu này nhóm các API bổ trợ cho hệ thống: Quản lý tài liệu (MaterialController), Nhắn tin (ChatController), và Thông báo (NotificationController).

---

## MaterialController (Tài liệu bài giảng)

### Endpoint
`GET /api/center/lessons/{lessonId}/materials`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách tài liệu của buổi học.
### Roles
`Center`, `Teacher`, `Parent`
### Response
- `200 OK`: `List<MaterialResponseDto>`

### Endpoint
`POST /api/center/lessons/{lessonId}/materials`
### Query Parameters
`lessonId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: `MaterialCreateDto` (chứa tên tài liệu, đường dẫn file).
### Purpose
Thêm metadata tài liệu (lưu DB) vào buổi học.
### Roles
`Center`, `Teacher`
### Response
- `201 Created`

### Endpoint
`POST /api/center/materials/upload-file`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`, `Content-Type: multipart/form-data`
- Body: File (IFormFile)
### Purpose
Upload file vật lý lên server (vào folder wwwroot/uploads).
### Roles
`Center`, `Teacher`
### Response
- `200 OK`: Trả về URL đường dẫn file đã lưu.

### Endpoint
`DELETE /api/center/materials/{materialId}`
### Query Parameters
`materialId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Xóa tài liệu khỏi hệ thống.
### Roles
`Center`, `Teacher`
### Response
- `200 OK`

---

## ChatController (Nhắn tin)

### Endpoint
`GET /api/Chat/channels`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách các kênh nhắn tin (Chat rooms) của người dùng hiện tại (Phụ huynh chat với Trung tâm).
### Roles
`All Authenticated Users`
### Response
- `200 OK`: Danh sách Channel.

### Endpoint
`GET /api/Chat/channels/{channelId}/messages`
### Query Parameters
`channelId` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách lịch sử tin nhắn trong một kênh cụ thể.
### Roles
`All Authenticated Users`
### Response
- `200 OK`: Danh sách Messages.

### Endpoint
`POST /api/Chat/upload`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`, `Content-Type: multipart/form-data`
- Body: File đính kèm.
### Purpose
Upload file/ảnh trong khung chat.
### Roles
`All Authenticated Users`
### Response
- `200 OK`: URL của file.

---

## NotificationController (Thông báo)

### Endpoint
`GET /api/notifications`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Lấy danh sách thông báo cá nhân của người dùng.
### Roles
`All Authenticated Users`
### Response
- `200 OK`: Danh sách Notifications (chưa đọc và đã đọc).

### Endpoint
`PUT /api/notifications/mark-all-read`
### Query Parameters
None
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Đánh dấu đã đọc tất cả thông báo của người dùng.
### Roles
`All Authenticated Users`
### Response
- `200 OK`

### Endpoint
`PUT /api/notifications/{id:int}/mark-read`
### Query Parameters
`id` (Path parameter)
### Request: (Header, Body)
- Header: `Authorization: Bearer <token>`
- Body: None
### Purpose
Đánh dấu đã đọc một thông báo cụ thể.
### Roles
`All Authenticated Users`
### Response
- `200 OK`
