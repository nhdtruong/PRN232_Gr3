# HƯỚNG DẪN BÁO VỆ ĐỒ ÁN MÔN PRN232 (C# & .NET)
## Dành cho Thành viên 5: Assessment + Notification + Realtime Chat

Tài liệu này tổng hợp toàn bộ kiến thức chuyên môn của **Người 5** theo đúng chuẩn học phần môn **PRN232 (C# và ASP.NET Core)**. Tài liệu bao gồm kiến trúc phân tầng, chi tiết API, các Class C# tương ứng, cơ chế Realtime sử dụng SignalR và mã nguồn JavaScript ở Frontend để bạn tự tin trả lời vấn đề và phản biện trước Hội đồng bảo vệ.

---

## 1. Kiến trúc phân tầng của Dự án (Architecture Layers)
Dự án được phát triển theo mô hình **Layered Architecture (Kiến trúc phân tầng)** chuẩn doanh nghiệp, chia nhỏ trách nhiệm giữa các Layer:

*   **PROJECT_PRN232_.Domain**: Chứa các Entity lớp thực thể ánh xạ vào database thông qua EF Core (`Assessment`, `Notification`, `ChatChannel`, `ChatMessage`) và các Enum (`AttendanceStatus`, `MessageType`). Layer này hoàn toàn độc lập và không phụ thuộc vào bất kỳ thư viện ngoài nào.
*   **PROJECT_PRN232_.Infrastructure**: Chứa DbContext (`EduBridgeDbContext`) và các Repository thực tế kế thừa từ interface để trực tiếp thực hiện truy vấn LINQ/SQL (`AssessmentRepository`, `NotificationRepository`, `ChatRepository`).
*   **PROJECT_PRN232_.Application**: Chứa tầng logic xử lý nghiệp vụ (Business Logic). Chứa các Interface/Class Service (`NotificationService`, `LessonRollCallService`, `ChatService`) và các **DTO (Data Transfer Object)** để vận chuyển dữ liệu giữa WebApp và API.
*   **PROJECT_PRN232_.Api**: Chứa các API Controller (`AssessmentController`, `ChatController`, `NotificationController`) cung cấp các đầu Endpoints JSON phục vụ Client.
*   **PROJECT_PRN232_.WebApp**: Chứa các giao diện Razor Pages (`GradeSheet.cshtml`, `RollCall.cshtml`, `Chat/Index.cshtml`) và mã nguồn JS tương tác với người dùng.

---

## 2. Phần 1: Assessment (Nhập điểm số & Nhận xét)

### 2.1. Cấu trúc Entity & Database (EF Core)
Lớp thực thể đại diện cho điểm số và nhận xét học sinh:
```csharp
public class Assessment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int LessonId { get; set; }
    public decimal? Score { get; set; } // Điểm số (0-10)
    public string? TeacherComment { get; set; } // Nhận xét giáo viên
    public DateTime DateAssessed { get; set; }

    // Relationship navigation properties
    public Student Student { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
```

### 2.2. DTO dùng để cập nhật (Data Transfer Object)
Để đóng gói dữ liệu và phân tách tính năng điểm danh với điểm số, chúng ta dùng cờ hiệu `IsGradeOnly` để tránh lỗi ghi đè dữ liệu điểm danh khi nhập điểm:
```csharp
public class LessonRollCallBulkUpsertDto
{
    [Required]
    public List<LessonRollCallRowDto> Rows { get; set; } = new();
    public bool IsAttendanceOnly { get; set; } = false;
    public bool IsGradeOnly { get; set; } = false; // Set thành true từ gradesheet.js
}
```

### 2.3. Logic xử lý nghiệp vụ (Application Service)
Trong `LessonRollCallService.cs`, khi giáo viên nhập điểm (ở trang Kết quả học tập), cờ `IsGradeOnly` được kích hoạt giúp bỏ qua việc cập nhật bảng điểm danh để tránh ghi đè dữ liệu cũ:
```csharp
if (dto.IsGradeOnly)
{
    // 1. Kiểm tra tính hợp lệ của điểm số (0 - 10)
    if (!AssessmentService.ValidateScores(dto.Rows.Select(r => new AssessmentUpsertDto { ... })))
        return false;

    // 2. Chuyển đổi DTO sang Model thực thể
    var assessments = dto.Rows.Select(r => new Assessment {
        StudentId = r.StudentId,
        Score = r.Score,
        TeacherComment = r.TeacherComment
    });

    // 3. Gọi repository thực thi câu lệnh SQL Upsert
    await _assessmentRepository.UpsertBulkAsync(lessonId, assessments);
    
    // 4. Bắn thông báo kết quả học tập tức thời cho Phụ huynh
    await SendGradeNotificationsAsync(lesson, dto.Rows);
}
```

### 2.4. Tương tác Frontend (JavaScript & Razor Pages)
*   **Trang UI**: `Pages/Center/Lessons/GradeSheet.cshtml` (Trang Kết quả học tập mới được tách độc lập để GV nhập điểm).
*   **Tệp JS**: `wwwroot/js/gradesheet.js`
    *   Tự động validate điểm số thực tế (nằm trong khoảng `0 - 10`).
    *   Gửi yêu cầu AJAX `PUT` lên API `/api/lessons/{lessonId}/rollcall` kèm payload chứa `isGradeOnly: true`.

---

## 3. Phần 2: Notification (Hệ thống thông báo Realtime)

### 3.1. Entity & Database
Lớp lưu trữ trạng thái thông báo của phụ huynh:
```csharp
public class Notification
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int? ClassId { get; set; }
    public string Title { get; set; } = string.Empty; // Tiêu đề hiển thị
    public string Message { get; set; } = string.Empty; // Nội dung HTML của thông báo
    public bool IsRead { get; set; } = false; // Trạng thái đã đọc hay chưa
    public DateTime CreatedAt { get; set; }
}
```

### 3.2. Cơ chế gửi Realtime (SignalR Hub)
Dự án sử dụng **ASP.NET Core SignalR** để tạo kết nối song hướng (full-duplex) thời gian thực.
*   **Hub định nghĩa**: `PROJECT_PRN232_.Api/Hubs/NotificationHub.cs`
*   Khi có sự kiện điểm danh hoặc có điểm số mới, hệ thống sẽ thực hiện đồng thời:
    1.  Lưu thông báo vào Database (đảm bảo tính toàn vẹn dữ liệu).
    2.  Đếm số tin nhắn chưa đọc của phụ huynh đó qua `CountUnreadByParentAsync`.
    3.  Bắn thông tin Realtime qua WebSocket đến phụ huynh đang online thông qua Hub Connection Client.

### 3.3. Tách biệt 2 loại thông báo (Yêu cầu quan trọng)
Chúng ta thiết kế tách biệt 2 phương thức gửi thông báo để tránh gây nhầm lẫn cho phụ huynh, nhưng vẫn giữ chung cấu trúc Header (`Lớp học`, `Buổi học`) như sau:

*   **NotifyAttendanceUpdatedAsync (Thông báo Điểm danh)**:
    *   **Tiêu đề**: `"Điểm danh - [Tên học sinh]"`
    *   **Nội dung**: Lớp học, Buổi học, trạng thái điểm danh (Có mặt, Đi trễ, Vắng mặt), ghi chú của giáo viên.
*   **NotifyGradeUpdatedAsync (Thông báo Kết quả học tập)**:
    *   **Tiêu đề**: `"Kết quả học tập - [Tên học sinh]"`
    *   **Nội dung**: Lớp học, Buổi học, điểm số, và phần nhận xét chi tiết của GV.

### 3.4. Xử lý phía Client (JavaScript & Modal Bootstrap 5)
*   **Tệp JS**: `wwwroot/js/notification-client.js`
*   Lắng nghe sự kiện SignalR `"ReceiveNotification"`:
    *   Tự động tăng badge đếm số thông báo chưa đọc trên thanh Topbar ngay lập tức.
    *   Khi người dùng click vào thông báo trong danh sách thả xuống (Dropdown), nó sẽ gọi API `/api/notifications/{id}/mark-read` để đổi trạng thái sang đã đọc (IsRead = true), đồng thời mở Modal Bootstrap 5 hiển thị chi tiết nội dung HTML cấu trúc.

---

## 4. Phần 3: Realtime Chat (Trò chuyện thời gian thực)

### 4.1. Thực thể cơ sở dữ liệu
*   **ChatChannel**: Đại diện cho phòng chat giữa 1 Phụ huynh và Trung tâm.
*   **ChatMessage**: Tin nhắn chi tiết lưu nội dung hoặc đường dẫn file tải lên.
```csharp
public class ChatMessage
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public int SenderId { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; } = false;

    // Tính năng đính kèm file
    public MessageType MessageType { get; set; } // 0: Text, 1: Image, 2: Video, 3: Document
    public string? FileUrl { get; set; }
}
```

### 4.2. Cơ chế truyền tải SignalR Chat
*   **Hub định nghĩa**: `PROJECT_PRN232_.Api/Hubs/ChatHub.cs`
*   **Luồng hoạt động**:
    1.  Khi Client mở trang chat, JS sẽ gọi kết nối SignalR và gia nhập Group chat của Channel thông qua hàm `JoinChannel(channelId)`.
    2.  Khi gửi tin nhắn, Client gọi hàm `SendMessage(channelId, content, messageType, fileUrl)`.
    3.  SignalR Hub nhận tin nhắn, gọi `ChatService` để lưu vào DB và phát tín hiệu `ReceiveMessage` cho người dùng còn lại cùng phòng chat đó.
    4.  Đồng thời, Hub sẽ kích hoạt sự kiện `ReceiveChatNotification` cho người dùng ở bên ngoài để họ nhận được Toast thông báo và cập nhật số lượng badge tin nhắn chưa đọc ở Navbar.

### 4.3. Các điểm cải tiến và sửa lỗi của bạn (Cực kỳ quan trọng để bảo vệ)
Khi thầy cô hỏi bạn đã giải quyết những khó khăn kỹ thuật gì trong phần Chat này, bạn trả lời:

1.  **Sửa lỗi Race Condition (Bất đồng bộ khi đọc tin nhắn)**: 
    *   *Hiện tượng cũ*: Ấn vào biểu tượng hộp thư nhưng số lượng chưa đọc không biến mất ngay, phải ấn lại lần nữa mới mất.
    *   *Nguyên nhân*: API lấy tin nhắn (`loadMessages`) chạy song song bất đồng bộ với API lấy số lượng chưa đọc (`updateNavbarChatCount`) nên số lượng chưa đọc được tính khi database chưa kịp cập nhật trạng thái `IsRead = true`.
    *   *Cách giải quyết*: Sử dụng cơ chế `await` tuần tự trong JS. Gọi hàm tải tin nhắn trước, chờ API phản hồi thành công và đánh dấu đã đọc xong mới gọi hàm cập nhật lại badge trên Navbar. Đồng thời thêm cache-buster (`t=Date.now()`) để ép trình duyệt không lấy kết quả cache API cũ.
2.  **Sửa lỗi Case-Insensitivity (Phân biệt chữ hoa/thường)**:
    *   *Hiện tượng*: Một số tài khoản phụ huynh không ẩn được badge hoặc bị lỗi logic role.
    *   *Nguyên nhân*: Cơ sở dữ liệu lưu chuỗi phân quyền dạng `"parent"`, `"center"` (chữ thường) hoặc `"Parent"`, `"Center"` (chữ hoa) dẫn đến code so sánh chuỗi trực tiếp bị sai lệch.
    *   *Cách giải quyết*: Chuẩn hóa chuỗi phân quyền bằng `string.Equals(..., StringComparison.OrdinalIgnoreCase)` ở Backend C# và gọi `.toLowerCase()` ở Frontend Javascript.
3.  **Hỗ trợ nhiều Layout Badge (Responsive)**:
    *   Thay vì dùng `document.getElementById` (chỉ lấy được 1 phần tử đầu tiên nếu trùng ID do giao diện Responsive Desktop và Mobile có chung thẻ), chúng ta sử dụng `document.querySelectorAll` để ẩn/hiện hoặc cập nhật giá trị badge đồng bộ cho cả giao diện máy tính và điện thoại.

---
*Chúc bạn bảo vệ đồ án thành công đạt kết quả cao nhất!*
