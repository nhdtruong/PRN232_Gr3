# Assessment, Notification, Chat & Realtime API (Phần Thành Viên)

Tài liệu này tổng hợp toàn bộ các API Controller, SignalR Hubs và các file mã nguồn liên quan đến phần tính năng **Đánh giá (Assessment), Thông báo (Notification), Kênh nhắn tin (Chat) và Thời gian thực (Realtime - SignalR)**.

---

## I. DANH SÁCH FILE LIÊN QUAN TRONG PROJECT
Dưới đây là các file mã nguồn thuộc phạm vi tính năng này để bạn dễ dàng theo dõi và báo cáo:

### 1. Controllers (API Endpoints)
- [AssessmentController.cs](file:///f:/PRN232/PRN232_Gr3/Controllers/AssessmentController.cs) — Quản lý Đánh giá/Nhận xét học viên.
- [ChatController.cs](file:///f:/PRN232/PRN232_Gr3/Controllers/ChatController.cs) — Quản lý lịch sử Kênh Chat và Tải lên tập tin.
- [NotificationController.cs](file:///f:/PRN232/PRN232_Gr3/Controllers/NotificationController.cs) — Quản lý trạng thái thông báo của Phụ huynh.

### 2. SignalR Hubs (Realtime)
- [ChatHub.cs](file:///f:/PRN232/PRN232_Gr3/Hubs/ChatHub.cs) — Hub quản lý gửi/nhận tin nhắn chat thời gian thực.
- [NotificationHub.cs](file:///f:/PRN232/PRN232_Gr3/Hubs/NotificationHub.cs) — Hub quản lý thông báo điểm danh/điểm số real-time.
- [CustomUserIdProvider.cs](file:///f:/PRN232/PRN232_Gr3/Hubs/CustomUserIdProvider.cs) — Provider ánh xạ UserId từ claims để gửi SignalR cá nhân hóa.

### 3. Services & Repositories (Business Logic & DB Access)
- **Services**: [AssessmentService.cs](file:///f:/PRN232/PRN232_Gr3/Services/AssessmentService.cs), [ChatService.cs](file:///f:/PRN232/PRN232_Gr3/Services/ChatService.cs), [NotificationService.cs](file:///f:/PRN232/PRN232_Gr3/Services/NotificationService.cs), [SignalRRealtimeNotifier.cs](file:///f:/PRN232/PRN232_Gr3/Services/Realtime/SignalRRealtimeNotifier.cs)
- **Repositories**: [AssessmentRepository.cs](file:///f:/PRN232/PRN232_Gr3/Repositories/AssessmentRepository.cs), [ChatRepository.cs](file:///f:/PRN232/PRN232_Gr3/Repositories/ChatRepository.cs), [NotificationRepository.cs](file:///f:/PRN232/PRN232_Gr3/Repositories/NotificationRepository.cs)

### 4. Razor Pages & Client Scripts (Frontend)
- **Pages**: 
  - `Center/Chat`: [Index.cshtml](file:///f:/PRN232/PRN232_Gr3/Pages/Center/Chat/Index.cshtml), [Index.cshtml.cs](file:///f:/PRN232/PRN232_Gr3/Pages/Center/Chat/Index.cshtml.cs)
  - `Parent/Chat`: [Index.cshtml](file:///f:/PRN232/PRN232_Gr3/Pages/Parent/Chat/Index.cshtml), [Index.cshtml.cs](file:///f:/PRN232/PRN232_Gr3/Pages/Parent/Chat/Index.cshtml.cs)
- **JS Scripts**: 
  - [chat-client.js](file:///f:/PRN232/PRN232_Gr3/wwwroot/js/chat-client.js) (Real-time chat client, animation, upload handler)
  - [notification-client.js](file:///f:/PRN232/PRN232_Gr3/wwwroot/js/notification-client.js) (Real-time notification listener)

---

## II. CHI TIẾT API ENDPOINTS (HTTP REST)

## 1. Assessment (Nhận xét / Đánh giá học viên)

### Endpoint: Lấy danh sách nhận xét đánh giá của buổi học
- **Route**: `GET /api/lessons/{lessonId}/assessment`
- **Method**: `GET`
- **Headers**: `Authorization: Bearer <JWT_TOKEN>`
- **Path Parameters**:
  - `lessonId` (int): ID của buổi học cần xem nhận xét.
- **Roles**: `Center`, `Parent`, `Teacher`
- **Description**: Lấy danh sách nhận xét của học viên trong buổi học. Phụ huynh khi gọi API này sẽ chỉ nhận được nhận xét của chính con mình.
- **Response**: `200 OK`
  ```json
  [
    {
      "id": 10,
      "lessonId": 1,
      "studentId": 4,
      "studentName": "Nguyễn Văn A",
      "score": 8.5,
      "teacherComment": "Học sinh tiếp thu bài tốt, hăng hái phát biểu.",
      "evaluatedAt": "2026-06-29T10:00:00Z"
    }
  ]
  ```

### Endpoint: Lưu/Cập nhật hàng loạt nhận xét đánh giá
- **Route**: `PUT /api/lessons/{lessonId}/assessment`
- **Method**: `PUT`
- **Headers**: `Authorization: Bearer <JWT_TOKEN>`, `Content-Type: application/json`
- **Path Parameters**:
  - `lessonId` (int): ID của buổi học cần ghi nhận xét.
- **Body**: `LessonAssessmentBulkDto`
  ```json
  {
    "assessments": [
      {
        "studentId": 4,
        "score": 9.0,
        "teacherComment": "Rất xuất sắc."
      },
      {
        "studentId": 5,
        "score": 7.5,
        "teacherComment": "Cần tập trung hơn."
      }
    ]
  }
  ```
- **Roles**: `Center` (hoặc Giáo viên có quyền)
- **Response**: `204 No Content` (Lưu thành công)
- **Errors**: `400 Bad Request` nếu thang điểm không hợp lệ (điểm phải từ 0 - 10) hoặc học sinh không thuộc lớp học.

---

## 2. Real-time Chat (Nhắn tin)

### Endpoint: Lấy danh sách kênh chát (Chat Rooms)
- **Route**: `GET /api/Chat/channels`
- **Method**: `GET`
- **Headers**: `Authorization: Bearer <JWT_TOKEN>`
- **Roles**: Mọi user đã đăng nhập
- **Description**: Trả về toàn bộ danh sách kênh chat của người dùng hiện tại (nếu là Parent: chỉ trả về kênh chat với Trung tâm; nếu là Center: trả về danh sách các kênh chat với từng phụ huynh).
- **Response**: `200 OK`
  ```json
  [
    {
      "id": 1,
      "centerId": 1,
      "centerName": "EduBridge Admin",
      "parentId": 3,
      "parentName": "Nguyễn Văn Phụ Huynh",
      "lastMessage": "Chào trung tâm nhé",
      "lastMessageSentAt": "2026-06-29T04:20:00Z",
      "isLastMessageRead": false,
      "lastMessageSenderId": 3
    }
  ]
  ```

### Endpoint: Lấy tin nhắn trong kênh chat
- **Route**: `GET /api/Chat/channels/{channelId}/messages`
- **Method**: `GET`
- **Headers**: `Authorization: Bearer <JWT_TOKEN>`
- **Query Parameters**:
  - `limit` (int, default=50): Số lượng tin nhắn tối đa cần tải.
- **Roles**: Thành viên của kênh chat đó (Center hoặc Parent sở hữu kênh).
- **Response**: `200 OK`
  ```json
  [
    {
      "id": 15,
      "channelId": 1,
      "senderId": 3,
      "senderName": "Nguyễn Văn Phụ Huynh",
      "messageContent": "Học liệu buổi 3 ở đâu ạ?",
      "messageType": 0,
      "fileUrl": null,
      "sentAt": "2026-06-29T04:19:00Z"
    }
  ]
  ```

### Endpoint: Tải lên file đính kèm trong chat
- **Route**: `POST /api/Chat/upload`
- **Method**: `POST`
- **Headers**: `Authorization: Bearer <JWT_TOKEN>`, `Content-Type: multipart/form-data`
- **Body**: File đính kèm (`IFormFile` dạng multipart).
- **Limit**: Ảnh tối đa 5MB, Video tối đa 20MB, Tài liệu (PDF/DOCX/XLSX) tối đa 10MB.
- **Response**: `200 OK`
  ```json
  {
    "fileUrl": "/uploads/images/guid-name.png",
    "messageType": 1,
    "originalName": "hinhanh.png",
    "sizeBytes": 102450
  }
  ```

---

## 3. Notifications (Thông báo)

### Endpoint: Lấy danh sách thông báo cá nhân
- **Route**: `GET /api/notifications`
- **Method**: `GET`
- **Query Parameters**: `limit` (int, default=20)
- **Response**: `200 OK`
  ```json
  [
    {
      "id": 102,
      "parentId": 3,
      "title": "Thông báo chuyên cần",
      "message": "Con của bạn đã vắng mặt trong buổi học ngày 28/06",
      "isRead": false,
      "createdAt": "2026-06-28T19:00:00Z"
    }
  ]
  ```

### Endpoint: Đánh dấu đã đọc tất cả thông báo
- **Route**: `PUT /api/notifications/mark-all-read`
- **Method**: `PUT`
- **Response**: `200 OK`
  ```json
  {
    "message": "Đã đánh dấu tất cả thông báo là đã đọc.",
    "unreadCount": 0
  }
  ```

### Endpoint: Đánh dấu đọc một thông báo cụ thể
- **Route**: `PUT /api/notifications/{id}/mark-read`
- **Method**: `PUT`
- **Response**: `200 OK` trả về số lượng thông báo chưa đọc còn lại.
  ```json
  {
    "unreadCount": 3
  }
  ```

---

## III. CHI TIẾT SIGNALR REALTIME HUBS

### 1. ChatHub (Kênh kết nối `/hubs/chat`)
Hub quản lý kết nối Socket của các phiên chát thời gian thực.
- **Client Invokes**:
  - `JoinChannel(int channelId)`: Đăng ký kết nối Socket vào phòng chát có ID tương ứng để lắng nghe tin nhắn của kênh đó.
  - `SendMessage(int channelId, string messageContent, int messageType, string fileUrl)`: Gửi tin nhắn mới lên Server qua Socket (đối với tin nhắn chứa File, client sẽ gọi API REST `upload` trước để lấy URL, sau đó truyền URL đó vào Hub).
- **Server Broadcasts**:
  - `ReceiveMessage(object message)`: Phát tín hiệu tin nhắn mới tới toàn bộ thành viên đang kết nối trong phòng chat.
  - `ReceiveChatNotification(object message)`: Phát tín hiệu tin nhắn mới toàn cục tới user đích nếu họ đang ở ngoài phòng chat, để cập nhật số lượng unread chat badge trên Navbar.

### 2. NotificationHub (Kênh kết nối `/notificationHub`)
Hub quản lý phát sóng thông báo điểm danh, nhận xét hoặc tài liệu mới.
- **Server Broadcasts**:
  - `ReceiveNotification(object notification, int newUnreadCount)`: Khi trung tâm thực hiện điểm danh, đánh giá học viên hoặc phát sóng học liệu mới, Server sẽ tự động tính toán số thông báo chưa đọc mới và đẩy trực tiếp xuống phụ huynh đích để nháy chuông thông báo hiển thị thời gian thực.
