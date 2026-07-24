# System Sequence Diagrams & Detailed Explanations - PRN232_Gr3

> **Ghi chú chuẩn hóa Visual Paradigm:**
> Tất cả 13 biểu đồ dưới đây được thiết kế theo chuẩn **Visual Paradigm UML Sequence Diagram (Mô hình ECB)**:
> - **Cơ chế Hộp thư liên hệ & Tự động tạo Kênh Chat**:
>   - Ngay khi tài khoản **Phụ huynh (Parent)** hoặc **Giáo viên (Teacher)** được khởi tạo trong hệ thống, Admin/Center sẽ thấy họ trong danh sách "Hộp thư liên hệ" (có 2 tab Phụ huynh & Giáo viên).
>   - Khi Admin/Center click vào bất kỳ Phụ huynh hoặc Giáo viên nào, hệ thống gọi `GetOrCreateChannelAsync`: Nếu chưa có phòng chat $\rightarrow$ Tự động khởi tạo `ChatChannel` mới để sẵn sàng nhắn tin ngay lập tức.
> - **Quy tắc phân quyền Chat (Chat Matrix)**:
>   - ✅ **Admin (Center) $\leftrightarrow$ Phụ huynh (Parent)**: ĐƯỢC PHÉP.
>   - ✅ **Admin (Center) $\leftrightarrow$ Giáo viên (Teacher)**: ĐƯỢC PHÉP.
>   - ❌ **Giáo viên (Teacher) $\leftrightarrow$ Phụ huynh (Parent)**: KHÔNG CHO PHÉP (Hệ thống ngăn cấm giao tiếp trực tiếp).
> - **Phân quyền Đọc Thông Báo**: API Notification (`NotificationController`) hỗ trợ cả **3 Vai Trò (Parent, Center, Teacher)** đọc và đánh dấu thông báo cá nhân là đã đọc.
> - **Nhập Bảng Điểm Định Kỳ (Giữa Kỳ / Cuối Kỳ)**: Do **Giáo viên (`Teacher`)** phụ trách giảng dạy lớp học thực hiện nhập điểm Giữa kỳ (30%), Cuối kỳ (40%) và nhận xét định kỳ.
> - **Ngôn ngữ**: Nội dung mô tả & hành động bằng **Tiếng Việt**, tên hàm C#, câu lệnh SQL, mã HTTP và DTO giữ nguyên **Tiếng Anh**.
> - **Nguyên tắc luồng**: Bắt đầu từ **Actor** tác động vào **View** và **kết thúc phản hồi về chính Actor**.
> - **Ký hiệu thành phần**: `actor` (Hình người), `boundary` (Màn hình/UI), `control` (Xử lý/Controller/Service), `entity` (Kho dữ liệu/Repository), `database` (Cơ sở dữ liệu).

---

## 📋 MỤC LỤC

1. [Phân Hệ Chat (5 Sơ đồ & Giải thích)](#1-phân-hệ-chat)
   - [1.1. Khởi Tạo / Lấy Kênh Chat Khi Bấm Chọn Liên Hệ](#11-khởi-tạo--lấy-kênh-chat-khi-bấm-chọn-liên-hệ)
   - [1.2. Kết Nối SignalR ChatHub & Gia Nhập Channel](#12-kết-nối-signalr-chathub--gia-nhập-channel)
   - [1.3. Gửi Tin Nhắn Văn Bản (Text Message)](#13-gửi-tin-nhắn-văn-bản-text-message)
   - [1.4. Upload File Học Liệu/Ảnh/Video & Gửi Qua SignalR](#14-upload-file-học-liệuảnhvideo--gửi-qua-signalr)
   - [1.5. Xem Danh Sách Channel & Đánh Dấu Tự Động Đã Đọc](#15-xem-danh-sách-channel--đánh-dấu-tự-động-đã-đọc)
2. [Phân Hệ Thông Báo (5 Sơ đồ & Giải thích)](#2-phân-hệ-thông-báo)
   - [2.1. Kết Nối NotificationHub & Lấy Số Thông Báo Chưa Đọc](#21-kết-nối-notificationhub--lấy-số-thông-báo-chưa-đọc)
   - [2.2. Thông Báo Điểm Danh & Nhận Xét Bài Học](#22-thông-báo-điểm-danh--nhận-xét-bài-học)
   - [2.3. Xuất Bản Buổi Học & Gửi Báo Cáo Tổng Hợp](#23-xuất-bản-buổi-học--gửi-báo-cáo-tổng-hợp)
   - [2.4. Thông Báo Xếp Lớp & Chuyển Lớp Cho Phụ Huynh](#24-thông-báo-xếp-lớp--chuyển-lớp-cho-phụ-huynh)
   - [2.5. Đọc Thông Báo & Đánh Dấu Đã Đọc](#25-đọc-thông-báo--đánh-dấu-đã-đọc)
3. [Phân Hệ Quản Lý Bảng Điểm & Điểm Danh (3 Sơ đồ & Giải thích)](#3-phân-hệ-quản-lý-bảng-điểm--điểm-danh)
   - [3.1. Giáo Viên Thực Hiện Điểm Danh & Nhập Điểm Buổi Học](#31-giáo-viên-thực-hiện-điểm-danh--nhập-điểm-buổi-học)
   - [3.2. Giáo Viên Cập Nhật Bảng Điểm Định Kỳ Giữa Kỳ & Cuối Kỳ](#32-giáo-viên-cập-nhật-bảng-điểm-định-kỳ-giữa-kỳ--cuối-kỳ)
   - [3.3. Phụ Huynh Tra Cứu Kết Quả Học Tập & Bảng Điểm Con](#33-phụ-huynh-tra-cứu-kết-quả-học-tập--bảng-điểm-con)

---

## 1. PHÂN HỆ CHAT

### 1.1. Khởi Tạo / Lấy Kênh Chat Khi Bấm Chọn Liên Hệ (Hộp Thư Liên Hệ)

```plantuml
@startuml
actor Admin as "Trung tâm / Admin"
boundary ContactListView as ":ContactListView (Hộp thư liên hệ)"
control ChatController as ":ChatController"
control ChatService as ":ChatService"
entity ChatRepository as ":ChatRepository"
database Database as ":Database"

Admin -> ContactListView: 1: Mở Hộp thư liên hệ & Chọn Tab (Phụ huynh / Giáo viên)
activate Admin
activate ContactListView

ContactListView -> Database: 1.1: SELECT Users WHERE Role In ('Parent', 'Teacher')
activate Database
Database --> ContactListView: 1.1.1: Danh sách tài khoản Phụ huynh / Giáo viên
deactivate Database

Admin -> ContactListView: 1.2: Click vào 1 Phụ huynh hoặc Giáo viên cụ thể
ContactListView -> ChatController: 1.2.1: GetOrCreateChannel(targetUserId)
activate ChatController

ChatController -> ChatService: 1.2.1.1: GetOrCreateChannelAsync(centerId, targetUserId)
activate ChatService

ChatService -> ChatRepository: 1.2.1.1.1: GetChannelByMembersAsync(centerId, targetUserId)
activate ChatRepository

ChatRepository -> Database: 1.2.1.1.1.1: SELECT * FROM ChatChannels WHERE CenterId = centerId AND TargetId = targetUserId
activate Database
Database --> ChatRepository: 1.2.1.1.1.2: Kết quả kênh chat (channel / null)
deactivate Database

ChatRepository --> ChatService: 1.2.1.1.2: Kênh chat tồn tại hoặc null
deactivate ChatRepository

alt existingChannel != null (Đã từng tạo phòng chat trước đây)
    ChatService --> ChatController: 1.2.1.2: DTO thông tin phòng chat hiện tại
else existingChannel == null (Chưa có phòng chat)
    ChatService -> ChatRepository: 1.2.1.3: CreateChannelAsync(new ChatChannel { CenterId, TargetId })
    activate ChatRepository
    
    ChatRepository -> Database: 1.2.1.3.1: INSERT INTO ChatChannels (CenterId, TargetId, CreatedAt = Now)
    activate Database
    Database --> ChatRepository: 1.2.1.3.2: Khởi tạo ChatChannel mới thành công
    deactivate Database
    
    ChatRepository --> ChatService: 1.2.1.3.3: Bản ghi ChatChannel vừa tạo
    deactivate ChatRepository
    
    ChatService --> ChatController: 1.2.1.4: DTO thông tin phòng chat mới tạo
end

deactivate ChatService
ChatController --> ContactListView: 1.2.2: 200 OK (ChatChannelResponseDto)
deactivate ChatController

ContactListView --> Admin: 1.3: Mở khung chat sẵn sàng nhắn tin ngay lập tức
deactivate ContactListView
deactivate Admin
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 1.1:
- **Mục đích**: Giải thích chính xác luồng trên giao diện **Hộp thư liên hệ**: Khi tài khoản Phụ huynh hoặc Giáo viên vừa được đăng ký/khởi tạo trong hệ thống, Admin sẽ nhìn thấy họ trên danh sách danh bạ "Hộp thư liên hệ". Khi Admin bấm chọn vào bất kỳ ai, hệ thống tự động kiểm tra xem 2 người đã từng nhắn tin chưa (`GetChannelByMembersAsync`). Nếu chưa từng nhắn tin, hệ thống tự động INSERT một bản ghi phòng chat mới (`CreateChannelAsync`) trong CSDL và bật màn hình chat lên để nhắn tin ngay.

---

### 1.2. Kết Nối SignalR ChatHub & Gia Nhập Channel

```plantuml
@startuml
actor Client as "Người dùng (Center / Parent / Teacher)"
boundary ChatView as ":ChatView"
control AuthMiddleware as ":JwtAuthMiddleware"
control UserIdProvider as ":CustomUserIdProvider"
control ChatHub as ":ChatHub"
control ChatService as ":ChatService"
entity ChatRepository as ":ChatRepository"
database Database as ":Database"

Client -> ChatView: 1: Mở phòng chat & Kết nối WebSocket
activate Client
activate ChatView

ChatView -> AuthMiddleware: 1.1: GET /hubs/chat?access_token={JWT}
activate AuthMiddleware

alt tokenIsInvalid == true (Mã Token không hợp lệ)
    AuthMiddleware --> ChatView: 1.1.1: 401 Unauthorized
    ChatView --> Client: 1.1.1.1: Hiển thị lỗi xác thực tài khoản
else tokenIsValid == true (Mã Token hợp lệ)
    AuthMiddleware -> UserIdProvider: 1.1.2: GetUserId(connectionContext)
    activate UserIdProvider
    UserIdProvider --> AuthMiddleware: 1.1.2.1: userId (ClaimTypes.NameIdentifier)
    deactivate UserIdProvider
    
    AuthMiddleware -> ChatHub: 1.1.3: OnConnectedAsync()
    activate ChatHub
    ChatHub --> ChatView: 1.1.3.1: Kết nối SignalR thành công (ConnectionId)
    deactivate AuthMiddleware
end

ChatView -> ChatHub: 1.2: JoinChannel(channelId)
ChatHub -> ChatService: 1.2.1: IsChannelMemberAsync(channelId, userId)
activate ChatService

ChatService -> ChatRepository: 1.2.1.1: GetChannelByIdAsync(channelId)
activate ChatRepository

ChatRepository -> Database: 1.2.1.1.1: SELECT * FROM ChatChannels WHERE Id = channelId
activate Database
Database --> ChatRepository: 1.2.1.1.2: Thông tin kênh ChatChannel
deactivate Database

ChatRepository --> ChatService: 1.2.1.2: Kênh ChatChannel
deactivate ChatRepository

ChatService --> ChatHub: 1.2.1.3: Kết quả isMember (Có phải thành viên)
deactivate ChatService

alt isMember == true (Center-Parent hoặc Center-Teacher)
    ChatHub -> ChatHub: 1.2.2: Groups.AddToGroupAsync(ConnectionId, channelId)
    ChatHub --> ChatView: 1.2.3: Gia nhập nhóm SignalR thành công
    ChatView --> Client: 1.3: Hiển thị giao diện phòng chat sẵn sàng
else isMember == false (Teacher-Parent bị cấm giao tiếp)
    ChatHub --> ChatView: 1.2.2: Từ chối gia nhập nhóm SignalR
    ChatView --> Client: 1.3: Hiển thị lỗi không cho phép giao tiếp trực tiếp
end

deactivate ChatHub
deactivate ChatView
deactivate Client
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 1.2:
- **Mục đích**: Xác thực kết nối WebSocket real-time qua JWT Token và phân quyền tham gia phòng chat. Hệ thống kiểm tra xem `userId` có phải là thành viên hợp lệ của phòng chat (`Center-Parent` hoặc `Center-Teacher`). Nếu là cặp `Teacher-Parent` tương tác trực tiếp, hệ thống sẽ ngăn cấm không cho gia nhập nhóm.

---

### 1.3. Gửi Tin Nhắn Văn Bản (Text Message)

```plantuml
@startuml
actor Sender as "Người gửi (Center / Parent / Teacher)"
boundary ChatView as ":ChatView"
control ChatHub as ":ChatHub"
control ChatService as ":ChatService"
entity ChatRepository as ":ChatRepository"
database Database as ":Database"
actor Recipient as "Người nhận (Parent / Teacher / Center)"

Sender -> ChatView: 1: Nhấn "Gửi tin nhắn"
activate Sender
activate ChatView

ChatView -> ChatHub: 1.1: SendMessage(channelId, messageContent, messageType = 0, fileUrl = null)
activate ChatHub

ChatHub -> ChatService: 1.1.1: IsChannelMemberAsync(channelId, senderId)
activate ChatService

ChatService -> ChatRepository: 1.1.1.1: GetChannelByIdAsync(channelId)
activate ChatRepository

ChatRepository -> Database: 1.1.1.1.1: SELECT * FROM ChatChannels WHERE Id = channelId
activate Database
Database --> ChatRepository: 1.1.1.1.2: Thông tin kênh ChatChannel
deactivate Database

ChatRepository --> ChatService: 1.1.1.2: Kênh ChatChannel
deactivate ChatRepository

ChatService --> ChatHub: 1.1.1.3: Kết quả isMember
deactivate ChatService

alt isMember == true && messageContent != null (Cặp đối tác hợp lệ: Center-Parent / Center-Teacher)
    ChatHub -> ChatService: 1.1.2: SendMessageAsync(channelId, senderId, messageContent, Text, null)
    activate ChatService
    
    ChatService -> ChatRepository: 1.1.2.1: AddMessageAsync(chatMessage)
    activate ChatRepository
    
    ChatRepository -> Database: 1.1.2.1.1: INSERT INTO ChatMessages (IsRead = 0, SentAt = Now)
    activate Database
    Database --> ChatRepository: 1.1.2.1.2: Bản ghi tin nhắn đã lưu (savedChatMessage)
    deactivate Database
    
    ChatRepository --> ChatService: 1.1.2.2: Bản ghi tin nhắn
    deactivate ChatRepository
    
    ChatService --> ChatHub: 1.1.2.3: DTO thông tin tin nhắn (chatMessageDto)
    deactivate ChatService
    
    ChatHub -> ChatView: 1.1.3: SendAsync("ReceiveMessage", chatMessageDto)
    ChatHub -> Recipient: 1.1.4: SendAsync("ReceiveChatNotification", chatMessageDto)
    activate Recipient
    Recipient --> ChatHub: 1.1.4.1: Nhận thông báo tin nhắn thành công (ack)
    deactivate Recipient
    
    ChatHub --> ChatView: 1.1.5: Trạng thái SUCCESS
    ChatView --> Sender: 1.2: Hiển thị tin nhắn đã gửi lên khung chat
else isMember == false || messageContent == null (Teacher-Parent hoặc Dữ liệu rỗng)
    ChatHub --> ChatView: 1.1.2: Trạng thái BAD_REQUEST / FORBIDDEN
    ChatView --> Sender: 1.2: Hiển thị thông báo cấm giao tiếp trực tiếp
end

deactivate ChatHub
deactivate ChatView
deactivate Sender
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 1.3:
- **Mục đích**: Xử lý việc gửi tin nhắn text, lưu trữ vào bảng `ChatMessages` với `IsRead = 0` và phát thông điệp tức thì qua 2 luồng: `ReceiveMessage` cho phòng chat hiện tại và `ReceiveChatNotification` để nổ Toast thông báo trên thiết bị của người nhận.

---

### 1.4. Upload File Học Liệu/Ảnh/Video & Gửi Qua SignalR

```plantuml
@startuml
actor Client as "Người dùng (Center / Parent / Teacher)"
boundary ChatView as ":ChatView"
control ChatController as ":ChatController"
control LocalFileSystem as ":Thư mục lưu trữ đĩa"
control ChatHub as ":ChatHub"
database Database as ":Database"

Client -> ChatView: 1: Chọn file & Nhấn "Tải lên & Gửi"
activate Client
activate ChatView

ChatView -> ChatController: 1.1: UploadFile(IFormFile file)
activate ChatController

alt file == null || file.Length == 0 (File rỗng)
    ChatController --> ChatView: 1.1.1: 400 BadRequest ("File rỗng")
    ChatView --> Client: 1.1.1.1: Hiển thị lỗi file rỗng
else extension || size invalid (Định dạng hoặc dung lượng không cho phép)
    ChatController --> ChatView: 1.1.1: 400 BadRequest ("Định dạng hoặc dung lượng vượt quá giới hạn")
    ChatView --> Client: 1.1.1.1: Hiển thị lỗi sai định dạng hoặc quá dung lượng
else fileIsValid == true (File hợp lệ)
    ChatController -> LocalFileSystem: 1.1.2: Save FileStream (/wwwroot/uploads/{folder}/{guid}.ext)
    activate LocalFileSystem
    LocalFileSystem --> ChatController: 1.1.2.1: Lưu tập tin thành công
    deactivate LocalFileSystem
    
    ChatController --> ChatView: 1.1.3: 200 OK { fileUrl, messageType, originalName }
    
    ChatView -> ChatHub: 1.2: SendMessage(channelId, originalName, messageType, fileUrl)
    activate ChatHub

    ChatHub -> Database: 1.2.1: INSERT INTO ChatMessages (MessageType = Image/Video/Doc, FileUrl = fileUrl)
    activate Database
    Database --> ChatHub: 1.2.1.1: Bản ghi tin nhắn file đã lưu
    deactivate Database

    ChatHub -> ChatView: 1.2.2: SendAsync("ReceiveMessage", chatMessageDto)
    ChatHub --> ChatView: 1.2.3: Gửi file qua SignalR thành công
    deactivate ChatHub

    ChatView --> Client: 1.3: Hiển thị tin nhắn chứa file đính kèm trên khung chat
end

deactivate ChatController
deactivate ChatView
deactivate Client
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 1.4:
- **Mục đích**: Upload file phương tiện (Ảnh <= 5MB, Tài liệu <= 10MB, Video <= 20MB) lên thư mục server vật lý qua REST API HTTP trước để tối ưu hiệu năng băng thông WebSocket, sau đó mới gửi tin nhắn chứa đường dẫn file qua SignalR Hub.

---

### 1.5. Xem Danh Sách Channel & Đánh Dấu Tự Động Đã Đọc

```plantuml
@startuml
actor User as "Người dùng (Center / Parent / Teacher)"
boundary ChatView as ":ChatView"
control ChatController as ":ChatController"
control ChatService as ":ChatService"
entity ChatRepository as ":ChatRepository"
database Database as ":Database"

User -> ChatView: 1: Mở danh sách Chat
activate User
activate ChatView

ChatView -> ChatController: 1.1: GetChannels()
activate ChatController

ChatController -> ChatService: 1.1.1: GetUserChannelsAsync(userId, role)
activate ChatService

ChatService -> ChatRepository: 1.1.1.1: GetChannelsByUserIdAsync(userId, role)
activate ChatRepository

ChatRepository -> Database: 1.1.1.1.1: SELECT Channels JOIN ChatMessages JOIN Users
activate Database
Database --> ChatRepository: 1.1.1.1.2: Danh sách kênh chat hợp lệ
deactivate Database

ChatRepository --> ChatService: 1.1.1.2: Danh sách kênh chat
deactivate ChatRepository

ChatService --> ChatController: 1.1.2: Danh sách DTO kênh chat
deactivate ChatService

ChatController --> ChatView: 1.1.3: 200 OK (List<ChatChannelResponseDto>)
deactivate ChatController

User -> ChatView: 1.2: Chọn 1 kênh chat cụ thể
ChatView -> ChatController: 1.2.1: GetChannelMessages(channelId, limit = 50)
activate ChatController

ChatController -> ChatService: 1.2.1.1: GetChannelMessagesAsync(channelId, userId, limit)
activate ChatService

ChatService -> ChatRepository: 1.2.1.1.1: MarkMessagesAsReadAsync(channelId, userId)
activate ChatRepository

ChatRepository -> Database: 1.2.1.1.1.1: UPDATE ChatMessages SET IsRead = 1 WHERE ChannelId = channelId AND SenderId != userId
activate Database
Database --> ChatRepository: 1.2.1.1.1.2: Cập nhật thành công
deactivate Database
deactivate ChatRepository

ChatService -> ChatRepository: 1.2.1.1.2: GetMessagesByChannelIdAsync(channelId, limit)
activate ChatRepository

ChatRepository -> Database: 1.2.1.1.2.1: SELECT TOP(limit) FROM ChatMessages WHERE ChannelId = channelId
activate Database
Database --> ChatRepository: 1.2.1.1.2.2: Danh sách tin nhắn
deactivate Database

ChatRepository --> ChatService: 1.2.1.1.3: Danh sách tin nhắn
deactivate ChatRepository

ChatService --> ChatController: 1.2.1.2: Danh sách DTO tin nhắn
deactivate ChatService

ChatController --> ChatView: 1.2.1.3: 200 OK (List<ChatMessageResponseDto>)
deactivate ChatController

ChatView --> User: 1.3: Hiển thị nội dung tin nhắn & Reset thông báo chưa đọc về 0
deactivate ChatView
deactivate User
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 1.5:
- **Mục đích**: Lấy danh sách hội thoại của người dùng và tự động chạy lệnh UPDATE `IsRead = 1` đối với tất cả các tin nhắn đối phương gửi ngay thời điểm người dùng nhấn chọn mở xem kênh chat đó.

---

## 2. PHÂN HỆ THÔNG BÁO

### 2.1. Kết Nối NotificationHub & Lấy Số Thông Báo Chưa Đọc

```plantuml
@startuml
actor Parent as "Phụ huynh"
boundary NotificationView as ":NotificationView"
control AuthMiddleware as ":JwtAuthMiddleware"
control NotificationHub as ":NotificationHub"
entity NotificationRepository as ":NotificationRepository"
database Database as ":Database"

Parent -> NotificationView: 1: Mở ứng dụng & Đăng nhập
activate Parent
activate NotificationView

NotificationView -> AuthMiddleware: 1.1: GET /notificationHub?access_token={JWT}
activate AuthMiddleware

AuthMiddleware -> NotificationHub: 1.1.1: OnConnectedAsync()
activate NotificationHub
deactivate AuthMiddleware

alt role == "Parent" (Tài khoản Phụ huynh)
    NotificationHub -> NotificationRepository: 1.1.1.1: CountUnreadByParentAsync(parentId)
    activate NotificationRepository
    
    NotificationRepository -> Database: 1.1.1.1.1: SELECT COUNT(*) FROM Notifications WHERE ParentId = parentId AND IsRead = 0
    activate Database
    Database --> NotificationRepository: 1.1.1.1.2: Số thông báo chưa đọc (unreadCount)
    deactivate Database
    
    NotificationRepository --> NotificationHub: 1.1.1.2: Số unreadCount
    deactivate NotificationRepository
    
    NotificationHub -> NotificationView: 1.1.1.3: SendAsync("UpdateUnreadCount", unreadCount)
end

NotificationHub --> NotificationView: 1.1.2: Kết nối SignalR NotificationHub thành công
deactivate NotificationHub
NotificationView --> Parent: 1.2: Hiển thị biểu tượng quả chuông với số thông báo chưa đọc
deactivate NotificationView
deactivate Parent
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 2.1:
- **Mục đích**: Khi Phụ huynh vừa mở app hoặc đăng nhập, kết nối WebSocket NotificationHub được thiết lập. Hệ thống tự động đếm con số thông báo chưa đọc (`IsRead = 0`) trong CSDL và phát lệnh `UpdateUnreadCount` để cập nhật biểu tượng quả chuông màu đỏ trên giao diện ứng dụng.

---

### 2.2. Thông Báo Điểm Danh & Nhận Xét Bài Học (RollCall Notification)

```plantuml
@startuml
actor Teacher as "Giáo viên"
boundary RollCallView as ":RollCallView"
control RollCallController as ":LessonRollCallController"
control RollCallService as ":LessonRollCallService"
control NotificationService as ":NotificationService"
entity NotificationRepository as ":NotificationRepository"
control RealtimeNotifier as ":SignalRRealtimeNotifier"
control NotificationHub as ":NotificationHub"
database Database as ":Database"
actor Parent as "Phụ huynh"

Teacher -> RollCallView: 1: Nhấn "Lưu điểm danh & điểm số"
activate Teacher
activate RollCallView

RollCallView -> RollCallController: 1.1: Save(lessonId, dto)
activate RollCallController

RollCallController -> RollCallService: 1.1.1: SaveRollCallAsync(lessonId, dto, teacherUserId)
activate RollCallService

RollCallService -> Database: 1.1.1.1: GetLessonWithClass & GetEnrolledStudentIds
activate Database
Database --> RollCallService: 1.1.1.2: Thông tin buổi học & Danh sách ID học sinh trong lớp
deactivate Database

alt isValid == true (Giáo viên giảng dạy lớp & Học sinh hợp lệ)
    RollCallService -> Database: 1.1.1.3: MERGE INTO Attendances & DailyAssessments
    activate Database
    Database --> RollCallService: 1.1.1.4: Lưu CSDL thành công
    deactivate Database
    
    RollCallService -> NotificationService: 1.1.1.5: NotifyRollCallUpdatedAsync(parentId, classId, className, ...)
    activate NotificationService
    
    NotificationService -> NotificationRepository: 1.1.1.5.1: AddAsync(notification)
    activate NotificationRepository
    NotificationRepository -> Database: 1.1.1.5.1.1: INSERT INTO Notifications (...)
    activate Database
    Database --> NotificationRepository: 1.1.1.5.1.2: Lưu thông báo thành công
    deactivate Database
    NotificationRepository --> NotificationService: 1.1.1.5.2: Bản ghi thông báo
    deactivate NotificationRepository
    
    NotificationService -> RealtimeNotifier: 1.1.1.5.3: PushNotificationToParentAsync(parentId, dto, unreadCount)
    activate RealtimeNotifier
    
    RealtimeNotifier -> NotificationHub: 1.1.1.5.3.1: SendAsync("ReceiveNotification", dto, unreadCount)
    activate NotificationHub
    
    NotificationHub -> Parent: 1.1.1.5.3.1.1: Đẩy thông báo Toast Real-time & Cập nhật số chưa đọc
    activate Parent
    Parent --> NotificationHub: 1.1.1.5.3.1.2: Nhận thông báo thành công (ack)
    deactivate Parent
    
    NotificationHub --> RealtimeNotifier: 1.1.1.5.3.2: Đẩy thông báo hoàn tất
    deactivate NotificationHub
    
    RealtimeNotifier --> NotificationService: 1.1.1.5.4: Đẩy thành công
    deactivate RealtimeNotifier
    
    NotificationService --> RollCallService: 1.1.1.6: Hoàn tất gửi thông báo
    deactivate NotificationService
    
    RollCallService --> RollCallController: 1.1.2: Trạng thái SUCCESS
    RollCallController -> RollCallView: 1.1.3: renderResult(status)
    RollCallView --> Teacher: 1.2: Hiển thị thông báo lưu điểm danh thành công
else invalid permissions / incorrect data (Không có quyền hoặc sai dữ liệu)
    RollCallService --> RollCallController: 1.1.2: Trạng thái FAILED
    RollCallController -> RollCallView: 1.1.3: renderResult(status)
    RollCallView --> Teacher: 1.2: Hiển thị thông báo lỗi lưu điểm danh
end

deactivate RollCallController
deactivate RollCallView
deactivate Teacher
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 2.2:
- **Mục đích**: Ngay sau khi Giáo viên lưu điểm danh hoặc nhập điểm bài học, `NotificationService` xây dựng thông báo định dạng HTML đẹp mắt (chứa badge Có mặt/Vắng mặt, điểm số và lời nhận xét) $\rightarrow$ Lưu vào bảng `Notifications` $\rightarrow$ Phát sự kiện `ReceiveNotification` để đẩy Toast thông báo trực tiếp đến Phụ huynh của từng học sinh.

---

### 2.3. Xuất Bản Buổi Học & Gửi Báo Cáo Tổng Hợp (Publish Lesson Report)

```plantuml
@startuml
actor Teacher as "Giáo viên"
boundary LessonView as ":LessonView"
control LessonController as ":LessonController"
control LessonService as ":LessonService"
entity LessonRepository as ":LessonRepository"
control NotificationService as ":NotificationService"
entity NotificationRepository as ":NotificationRepository"
control RealtimeNotifier as ":SignalRRealtimeNotifier"
control NotificationHub as ":NotificationHub"
database Database as ":Database"
actor Parent as "Phụ huynh"

Teacher -> LessonView: 1: Nhấn "Xuất bản buổi học"
activate Teacher
activate LessonView

LessonView -> LessonController: 1.1: Publish(lessonId)
activate LessonController

LessonController -> LessonService: 1.1.1: PublishAsync(lessonId, teacherUserId)
activate LessonService

LessonService -> LessonRepository: 1.1.1.1: GetLessonWithMaterialsAsync(lessonId)
activate LessonRepository

LessonRepository -> Database: 1.1.1.1.1: SELECT Lesson JOIN Class JOIN Materials WHERE Id = lessonId
activate Database
Database --> LessonRepository: 1.1.1.1.2: Thông tin buổi học
deactivate Database

LessonRepository --> LessonService: 1.1.1.2: Thông tin buổi học
deactivate LessonRepository

alt lesson != null && lesson.Class.TeacherId == teacherUserId (Giáo viên phụ trách bài học)
    LessonService -> LessonRepository: 1.1.1.3: PublishLessonAsync(lessonId)
    activate LessonRepository
    LessonRepository -> Database: 1.1.1.3.1: UPDATE Lessons SET IsPublished = 1 WHERE Id = lessonId
    activate Database
    Database --> LessonRepository: 1.1.1.3.2: Cập nhật thành công
    deactivate Database
    deactivate LessonRepository
    
    LessonService -> NotificationService: 1.1.1.4: NotifyPublishedLessonAsync(lessonId, classId, className, ...)
    activate NotificationService
    
    NotificationService -> Database: 1.1.1.4.1: SELECT ParentIds FROM ClassStudents WHERE ClassId = classId
    activate Database
    Database --> NotificationService: 1.1.1.4.2: Danh sách ID Phụ huynh trong lớp
    deactivate Database
    
    loop Lặp qua từng Phụ huynh trong lớp
        NotificationService -> Database: 1.1.1.4.3: SELECT Student, Attendance, Assessment, Transcript
        activate Database
        Database --> NotificationService: 1.1.1.4.4: Dữ liệu báo cáo buổi học
        deactivate Database
        
        NotificationService -> NotificationRepository: 1.1.1.4.5: AddAsync(notification)
        activate NotificationRepository
        NotificationRepository -> Database: 1.1.1.4.5.1: INSERT INTO Notifications (...)
        activate Database
        Database --> NotificationRepository: 1.1.1.4.5.2: Lưu thông báo thành công
        deactivate Database
        deactivate NotificationRepository
        
        NotificationService -> RealtimeNotifier: 1.1.1.4.6: PushNotificationToParentAsync(parentId, dto, unreadCount)
        activate RealtimeNotifier
        RealtimeNotifier -> NotificationHub: 1.1.1.4.6.1: SendAsync("ReceiveNotification", dto, unreadCount)
        activate NotificationHub
        NotificationHub -> Parent: 1.1.1.4.6.1.1: Đẩy thông báo Báo cáo bài học Real-time
        activate Parent
        Parent --> NotificationHub: 1.1.1.4.6.1.2: Nhận báo cáo thành công (ack)
        deactivate Parent
        deactivate NotificationHub
        deactivate RealtimeNotifier
    end
    
    NotificationService --> LessonService: 1.1.1.5: Hoàn tất gửi báo cáo bài học
    deactivate NotificationService
    
    LessonService --> LessonController: 1.1.2: Trạng thái SUCCESS
    LessonController -> LessonView: 1.1.3: renderPublishResult(status)
    LessonView --> Teacher: 1.2: Hiển thị thông báo xuất bản bài học thành công
else lesson == null || TeacherId mismatch (Không có quyền hoặc bài học không tồn tại)
    LessonService --> LessonController: 1.1.2: Trạng thái FAILED
    LessonController -> LessonView: 1.1.3: renderPublishResult(status)
    LessonView --> Teacher: 1.2: Hiển thị thông báo lỗi không có quyền xuất bản
end

deactivate LessonController
deactivate LessonView
deactivate Teacher
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 2.3:
- **Mục đích**: Cho phép Giáo viên nhấn "Xuất bản buổi học" (`IsPublished = 1`). Hệ thống tự động gom dữ liệu chuyên cần, điểm thường xuyên của buổi học, điểm tổng kết định kỳ (`ClassTranscript`) và danh sách bài giảng/slide `Materials` để gửi bản báo cáo tổng hợp chất lượng buổi học tới tất cả Phụ huynh trong lớp.

---

### 2.4. Thông Báo Xếp Lớp & Chuyển Lớp Cho Phụ Huynh

```plantuml
@startuml
actor Center as "Trung tâm (Quản trị)"
boundary EnrollmentView as ":EnrollmentView"
control CenterClassController as ":CenterClassController"
control EnrollmentService as ":EnrollmentService"
control NotificationService as ":NotificationService"
entity NotificationRepository as ":NotificationRepository"
control RealtimeNotifier as ":SignalRRealtimeNotifier"
control NotificationHub as ":NotificationHub"
database Database as ":Database"
actor Parent as "Phụ huynh"

Center -> EnrollmentView: 1: Nhấn "Xếp lớp / Chuyển lớp học sinh"
activate Center
activate EnrollmentView

EnrollmentView -> CenterClassController: 1.1: EnrollStudent(classId, dto) / TransferStudentClass(studentId, dto)
activate CenterClassController

CenterClassController -> EnrollmentService: 1.1.1: EnrollStudentAsync(classId, studentId)
activate EnrollmentService

EnrollmentService -> Database: 1.1.1.1: Kiểm tra trạng thái lớp, sức chứa & trùng lịch học
activate Database
Database --> EnrollmentService: 1.1.1.2: Kết quả kiểm tra (Hợp lệ)
deactivate Database

EnrollmentService -> Database: 1.1.1.3: INSERT INTO ClassStudents (ClassId, StudentId, EnrolledAt)
activate Database
Database --> EnrollmentService: 1.1.1.4: Xếp lớp thành công
deactivate Database

EnrollmentService -> NotificationService: 1.1.1.5: NotifyStudentEnrolledAsync(parentId, studentName, classId, className)
activate NotificationService

NotificationService -> NotificationRepository: 1.1.1.5.1: AddAsync(notification)
activate NotificationRepository
NotificationRepository -> Database: 1.1.1.5.1.1: INSERT INTO Notifications (...)
activate Database
Database --> NotificationRepository: 1.1.1.5.1.2: Lưu thông báo thành công
deactivate Database
deactivate NotificationRepository

NotificationService -> RealtimeNotifier: 1.1.1.5.2: PushNotificationToParentAsync(parentId, dto, unreadCount)
activate RealtimeNotifier
RealtimeNotifier -> NotificationHub: 1.1.1.5.2.1: SendAsync("ReceiveNotification", dto, unreadCount)
activate NotificationHub
NotificationHub -> Parent: 1.1.1.5.2.1.1: Đẩy thông báo nhập học Real-time
activate Parent
Parent --> NotificationHub: 1.1.1.5.2.1.2: Nhận thông báo thành công (ack)
deactivate Parent
deactivate NotificationHub
deactivate RealtimeNotifier

NotificationService --> EnrollmentService: 1.1.1.6: Hoàn tất gửi thông báo
deactivate NotificationService

EnrollmentService --> CenterClassController: 1.1.2: Trạng thái SUCCESS
deactivate EnrollmentService

CenterClassController -> EnrollmentView: 1.1.3: renderResult(status)
EnrollmentView --> Center: 1.2: Hiển thị thông báo xếp lớp thành công
deactivate CenterClassController
deactivate EnrollmentView
deactivate Center
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 2.4:
- **Mục đích**: Khi Quản trị viên Trung tâm thực hiện xếp lớp (`EnrollStudentAsync`) hoặc chuyển lớp (`TransferStudentClassAsync`) cho học sinh, hệ thống sẽ tự động sinh thông báo báo thông thông tin lớp mới và đẩy thông báo real-time tới Phụ huynh của học sinh đó.

---

### 2.5. Đọc Thông Báo & Đánh Dấu Đã Đọc (Dành Cho 3 Vai Trò: Parent, Center, Teacher)

```plantuml
@startuml
actor User as "Người dùng (Phụ huynh / Trung tâm / Giáo viên)"
boundary NotificationView as ":NotificationView"
control NotificationController as ":NotificationController"
control NotificationService as ":NotificationService"
entity NotificationRepository as ":NotificationRepository"
database Database as ":Database"

User -> NotificationView: 1: Nhấn "Đã đọc" / "Đánh dấu tất cả đã đọc"
activate User
activate NotificationView

alt Mark Single Notification Read (Đánh dấu 1 thông báo)
    NotificationView -> NotificationController: 1.1: MarkSingleRead(id)
    activate NotificationController
    
    NotificationController -> NotificationService: 1.1.1: MarkSingleAsReadAsync(id, userId)
    activate NotificationService
    
    NotificationService -> NotificationRepository: 1.1.1.1: MarkSingleAsReadAsync(id, userId)
    activate NotificationRepository
    NotificationRepository -> Database: 1.1.1.1.1: UPDATE Notifications SET IsRead = 1 WHERE Id = id AND ParentId = userId
    activate Database
    Database --> NotificationRepository: 1.1.1.1.2: Cập nhật thành công
    deactivate Database
    deactivate NotificationRepository
    
    NotificationService -> NotificationRepository: 1.1.1.2: CountUnreadByParentAsync(userId)
    activate NotificationRepository
    NotificationRepository -> Database: 1.1.1.2.1: SELECT COUNT(*) FROM Notifications WHERE ParentId = userId AND IsRead = 0
    activate Database
    Database --> NotificationRepository: 1.1.1.2.2: Số thông báo chưa đọc mới (newUnreadCount)
    deactivate Database
    deactivate NotificationRepository
    
    NotificationService --> NotificationController: 1.1.2: Số newUnreadCount
    deactivate NotificationService
    
    NotificationController --> NotificationView: 1.1.3: 200 OK { unreadCount: newUnreadCount }
    deactivate NotificationController
    NotificationView --> User: 1.3: Cập nhật lại số thông báo chưa đọc trên quả chuông UI
else Mark All Notifications Read (Đánh dấu tất cả)
    NotificationView -> NotificationController: 1.2: MarkAllRead()
    activate NotificationController
    
    NotificationController -> NotificationService: 1.2.1: MarkAllAsReadByParentAsync(userId)
    activate NotificationService
    
    NotificationService -> NotificationRepository: 1.2.1.1: MarkAllAsReadByParentAsync(userId)
    activate NotificationRepository
    NotificationRepository -> Database: 1.2.1.1.1: UPDATE Notifications SET IsRead = 1 WHERE ParentId = userId AND IsRead = 0
    activate Database
    Database --> NotificationRepository: 1.2.1.1.2: Cập nhật tất cả thành công
    deactivate Database
    deactivate NotificationRepository
    
    NotificationService --> NotificationController: 1.2.2: Thành công
    deactivate NotificationService
    
    NotificationController --> NotificationView: 1.2.3: 200 OK { unreadCount: 0 }
    deactivate NotificationController
    NotificationView --> User: 1.3: Đặt số thông báo chưa đọc trên quả chuông về 0
end

deactivate NotificationView
deactivate User
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 2.5:
- **Mục đích & Phân quyền 3 Vai Trò**: API `NotificationController` áp dụng thuộc tính `[Authorize(Roles = "Parent,Center,Teacher")]`. Điều này có nghĩa là cả **3 Vai Trò (Phụ huynh, Trung tâm, Giáo viên)** đều có thể sử dụng màn hình này để xem danh sách thông báo của chính mình và cập nhật trạng thái đã đọc (`IsRead = 1`), giúp giao diện tự động trừ số lượng quả chuông màu đỏ về 0.

---

## 3. PHÂN HỆ QUẢN LÝ BẢNG ĐIỂM & ĐIỂM DANH

### 3.1. Giáo Viên Thực Hiện Điểm Danh & Nhập Điểm Buổi Học

```plantuml
@startuml
actor Teacher as "Giáo viên"
boundary RollCallView as ":RollCallView"
control RollCallController as ":LessonRollCallController"
control RollCallService as ":LessonRollCallService"
control AssessmentService as ":AssessmentService"
entity LessonRepository as ":LessonRepository"
entity AttendanceRepository as ":AttendanceRepository"
entity DailyAssessmentRepository as ":DailyAssessmentRepository"
database Database as ":Database"

Teacher -> RollCallView: 1: Nhấn "Lấy danh sách điểm danh"
activate Teacher
activate RollCallView

RollCallView -> RollCallController: 1.1: Get(lessonId)
activate RollCallController

RollCallController -> RollCallService: 1.1.1: GetRollCallByLessonAsync(lessonId)
activate RollCallService

RollCallService -> LessonRepository: 1.1.1.1: GetLessonWithClassAsync(lessonId)
activate LessonRepository
LessonRepository -> Database: 1.1.1.1.1: SELECT Lesson JOIN Class WHERE Id = lessonId
activate Database
Database --> LessonRepository: 1.1.1.1.2: Thông tin buổi học
deactivate Database
LessonRepository --> RollCallService: 1.1.1.2: Buổi học
deactivate LessonRepository

RollCallService -> LessonRepository: 1.1.1.3: GetRollCallDataAsync(lessonId, classId)
activate LessonRepository
LessonRepository -> Database: 1.1.1.3.1: SELECT Students LEFT JOIN Attendances LEFT JOIN DailyAssessments
activate Database
Database --> LessonRepository: 1.1.1.3.2: Danh sách dữ liệu điểm danh học sinh
deactivate Database
LessonRepository --> RollCallService: 1.1.1.4: Danh sách dòng dữ liệu
deactivate LessonRepository

RollCallService --> RollCallController: 1.1.2: DTO dữ liệu điểm danh buổi học
deactivate RollCallService

RollCallController --> RollCallView: 1.1.3: 200 OK (RollCall Data)
RollCallView --> Teacher: 1.2: Hiển thị form danh sách học sinh để nhập điểm danh & điểm số

Teacher -> RollCallView: 1.3: Nhập trạng thái, điểm số & Nhấn "Lưu"
RollCallView -> RollCallController: 1.3.1: Save(lessonId, dto)
activate RollCallController

RollCallController -> RollCallService: 1.3.1.1: SaveRollCallAsync(lessonId, dto, teacherUserId)
activate RollCallService

RollCallService -> LessonRepository: 1.3.1.1.1: GetEnrolledStudentIdsAsync(classId)
activate LessonRepository
LessonRepository -> Database: 1.3.1.1.1.1: SELECT StudentId FROM ClassStudents WHERE ClassId = classId
activate Database
Database --> LessonRepository: 1.3.1.1.1.2: Danh sách ID học sinh trong lớp
deactivate Database
LessonRepository --> RollCallService: 1.3.1.1.2: Danh sách học sinh hợp lệ
deactivate LessonRepository

alt dto.IsGradeOnly == true (Chỉ lưu điểm số bài học)
    RollCallService -> AssessmentService: 1.3.1.1.3: ValidateScores(dto.Rows)
    activate AssessmentService
    AssessmentService --> RollCallService: 1.3.1.1.4: Kết quả kiểm tra điểm (0.0 <= Score <= 10.0)
    deactivate AssessmentService
    
    RollCallService -> DailyAssessmentRepository: 1.3.1.1.5: UpsertBulkAsync(lessonId, assessments)
    activate DailyAssessmentRepository
    DailyAssessmentRepository -> Database: 1.3.1.1.5.1: MERGE INTO DailyAssessments
    activate Database
    Database --> DailyAssessmentRepository: 1.3.1.1.5.2: Lưu điểm số thành công
    deactivate Database
    deactivate DailyAssessmentRepository
else dto.IsAttendanceOnly == true (Chỉ lưu điểm danh)
    RollCallService -> AttendanceRepository: 1.3.1.1.3: UpsertBulkAsync(lessonId, attendances)
    activate AttendanceRepository
    AttendanceRepository -> Database: 1.3.1.1.3.1: MERGE INTO Attendances
    activate Database
    Database --> AttendanceRepository: 1.3.1.1.3.2: Lưu điểm danh thành công
    deactivate Database
    deactivate AttendanceRepository
end

RollCallService --> RollCallController: 1.3.1.2: Trạng thái SUCCESS
deactivate RollCallService

RollCallController --> RollCallView: 1.3.1.3: 204 NoContent
deactivate RollCallController

RollCallView --> Teacher: 1.4: Hiển thị thông báo lưu điểm danh thành công
deactivate RollCallView
deactivate Teacher
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 3.1:
- **Mục đích**: Mô tả 2 giai đoạn làm việc của Giáo viên:
  1. Giai đoạn 1 (`GET`): Lấy danh sách điểm danh và điểm số bài học hiện có của lớp.
  2. Giai đoạn 2 (`PUT`): Nhập trạng thái chuyên cần (Có mặt, Vắng mặt, Đi muộn), nhập điểm số bài học (kiểm tra `0.0 <= Score <= 10.0`) và chạy thủ tục `MERGE` lưu vào CSDL hàng loạt.

---

### 3.2. Giáo Viên Cập Nhật Bảng Điểm Định Kỳ Giữa Kỳ & Cuối Kỳ (Transcript Report)

```plantuml
@startuml
actor Teacher as "Giáo viên"
boundary ClassTranscriptView as ":ClassTranscriptView (Bảng điểm định kỳ)"
control PageModel as ":ClassTranscriptModel"
control NotificationService as ":NotificationService"
entity NotificationRepository as ":NotificationRepository"
control RealtimeNotifier as ":SignalRRealtimeNotifier"
control NotificationHub as ":NotificationHub"
database Database as ":Database"
actor Parent as "Phụ huynh"

Teacher -> ClassTranscriptView: 1: Nhập điểm Giữa kỳ (30%), Cuối kỳ (40%) & Nhấn "Lưu bảng điểm"
activate Teacher
activate ClassTranscriptView

ClassTranscriptView -> PageModel: 1.1: OnPostSaveAsync(classId, studentTranscriptRows)
activate PageModel

PageModel -> Database: 1.1.1: MERGE INTO ClassTranscripts (MidTermScore, MidTermComment, FinalScore, FinalComment, FinalScoreTotal)
activate Database
Database --> PageModel: 1.1.2: Lưu bảng điểm định kỳ thành công
deactivate Database

loop Lặp qua từng học sinh được cập nhật bảng điểm
    PageModel -> NotificationService: 1.1.3: NotifyClassTranscriptUpdatedAsync(parentId, classId, className, ...)
    activate NotificationService

    NotificationService -> NotificationRepository: 1.1.3.1: AddAsync(notification)
    activate NotificationRepository

    NotificationRepository -> Database: 1.1.3.1.1: INSERT INTO Notifications (...)
    activate Database
    Database --> NotificationRepository: 1.1.3.1.2: Lưu thông báo thành công
    deactivate Database
    NotificationRepository --> NotificationService: 1.1.3.2: Bản ghi thông báo
    deactivate NotificationRepository

    NotificationService -> RealtimeNotifier: 1.1.3.3: PushNotificationToParentAsync(parentId, dto, unreadCount)
    activate RealtimeNotifier

    RealtimeNotifier -> NotificationHub: 1.1.3.3.1: SendAsync("ReceiveNotification", dto, unreadCount)
    activate NotificationHub

    NotificationHub -> Parent: 1.1.3.3.1.1: Đẩy thông báo Bảng điểm Giữa kỳ/Cuối kỳ Real-time
    activate Parent
    Parent --> NotificationHub: 1.1.3.3.1.2: Nhận thông báo thành công (ack)
    deactivate Parent

    NotificationHub --> RealtimeNotifier: 1.1.3.3.2: Đẩy thành công
    deactivate NotificationHub

    RealtimeNotifier --> NotificationService: 1.1.3.4: Hoàn tất đẩy thông báo
    deactivate NotificationService

    NotificationService --> PageModel: 1.1.4: Hoàn tất gửi thông báo bảng điểm
    deactivate NotificationService
end

PageModel --> ClassTranscriptView: 1.1.5: Render lại trang với thông báo "Lưu bảng điểm định kỳ thành công"
ClassTranscriptView --> Teacher: 1.2: Hiển thị thông báo lưu bảng điểm định kỳ thành công
deactivate PageModel
deactivate ClassTranscriptView
deactivate Teacher
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 3.2:
- **Mục đích (Khớp 100% với giao diện màn hình Giáo viên nhập bảng điểm định kỳ)**:
  - **Màn hình**: `Pages/Teacher/Lessons/ClassTranscript.cshtml` (*Bảng điểm định kỳ Giữa kỳ / Cuối kỳ*).
  - **Actor**: **Giáo viên (`Teacher`)** phụ trách giảng dạy lớp học.
  - **Quy trình**: Giáo viên nhập điểm Giữa kỳ (hệ số 30%), Nhận xét Giữa kỳ, Điểm Cuối kỳ (hệ số 40%), Nhận xét Cuối kỳ $\rightarrow$ Bấm nút "Lưu bảng điểm" $\rightarrow$ Hệ thống tính Điểm Tổng kết (`0.3 * TX + 0.3 * GK + 0.4 * CK`), lưu vào bảng `ClassTranscripts` trong CSDL $\rightarrow$ Tự động sinh thông báo và bắn thông báo real-time trực tiếp đến Phụ huynh của từng học sinh trong lớp.

---

### 3.3. Phụ Huynh Tra Cứu Kết Quả Học Tập & Bảng Điểm Con (Security & IsPublished Filter)

```plantuml
@startuml
actor Parent as "Phụ huynh"
boundary ParentGradeView as ":ParentGradeView"
control AssessmentController as ":AssessmentController"
control AssessmentService as ":AssessmentService"
entity StudentRepository as ":StudentRepository"
entity DailyAssessmentRepository as ":DailyAssessmentRepository"
database Database as ":Database"

Parent -> ParentGradeView: 1: Nhấn "Xem bảng điểm & đánh giá của con"
activate Parent
activate ParentGradeView

ParentGradeView -> AssessmentController: 1.1: GetForParent(studentId)
activate AssessmentController

AssessmentController -> AssessmentService: 1.1.1: GetByStudentIdAsync(studentId, parentId)
activate AssessmentService

AssessmentService -> StudentRepository: 1.1.1.1: IsOwnChildAsync(studentId, parentId)
activate StudentRepository

StudentRepository -> Database: 1.1.1.1.1: SELECT COUNT(*) FROM Students WHERE Id = studentId AND ParentId = parentId
activate Database
Database --> StudentRepository: 1.1.1.1.2: Số lượng học sinh tương ứng
deactivate Database

StudentRepository --> AssessmentService: 1.1.1.2: Kết quả isOwnChild
deactivate StudentRepository

alt isOwnChild == true (Xác minh mối quan hệ Phụ huynh - Con thành công)
    AssessmentService -> DailyAssessmentRepository: 1.1.1.3: GetByStudentIdAsync(studentId)
    activate DailyAssessmentRepository
    
    DailyAssessmentRepository -> Database: 1.1.1.3.1: SELECT DailyAssessments JOIN Lessons WHERE StudentId = studentId
    activate Database
    Database --> DailyAssessmentRepository: 1.1.1.3.2: Danh sách điểm bài học
    deactivate Database
    
    DailyAssessmentRepository --> AssessmentService: 1.1.1.4: Danh sách điểm bài học
    deactivate DailyAssessmentRepository
    
    AssessmentService -> AssessmentService: 1.1.1.5: Lọc chỉ lấy bài học ĐÃ XUẤT BẢN (IsPublished == true)
    AssessmentService --> AssessmentController: 1.1.2: Danh sách DTO điểm bài học
    AssessmentController -> ParentGradeView: 1.1.3: renderGradeTable(assessmentDtos)
    ParentGradeView --> Parent: 1.2: Hiển thị bảng điểm học tập các bài học đã xuất bản
else isOwnChild == false (Bảo mật: Không phải con của phụ huynh này)
    AssessmentService --> AssessmentController: 1.1.2: Danh sách rỗng []
    AssessmentController -> ParentGradeView: 1.1.3: renderGradeTable([])
    ParentGradeView --> Parent: 1.2: Hiển thị thông báo không có quyền truy cập dữ liệu học sinh này
end

deactivate AssessmentService
deactivate AssessmentController
deactivate ParentGradeView
deactivate Parent
@enduml
```

#### 📖 Giải Thích Ý Nghĩa Sơ Đồ 3.3:
- **Mục đích**: Xử lý việc Phụ huynh xem điểm và nhận xét của con mình.
- **Cơ chế bảo mật**:
  - `IsOwnChildAsync`: Kiểm tra xem học sinh `studentId` có đúng là con của Phụ huynh `parentId` đang đăng nhập hay không để ngăn chặn việc xem trộm điểm học sinh khác.
  - Bộ lọc `IsPublished == true`: Chỉ hiển thị điểm bài học đã được Giáo viên bấm **Xuất bản bài học**.
