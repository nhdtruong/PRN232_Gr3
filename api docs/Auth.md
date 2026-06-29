# Authentication & Profile API

## AuthController
Quản lý đăng ký, đăng nhập và bảo mật người dùng.

---


### Endpoint
`POST /api/Auth/login`
### Query Parameters
None
### Request: (Header, Body)
- Header: None
- Body: `LoginDto` (chứa Username, Password)
### Purpose
Đăng nhập, trả về JWT Token.
### Roles
All (Không yêu cầu đăng nhập)
### Response
- `200 OK`: `{ "token": "<chuỗi JWT>" }`
### Error Cases
- `400 Bad Request`: Model state không hợp lệ.
- `401 Unauthorized`: `{"message": "Tài khoản hoặc mật khẩu không chính xác!"}` hoặc tài khoản đã bị vô hiệu hóa.

---

### Endpoint
`POST /api/Auth/logout`
### Query Parameters
None
### Request: (Header, Body)
- Header: Không yêu cầu bắt buộc (chỉ gọi để client dọn dẹp)
- Body: None
### Purpose
Đăng xuất khỏi hệ thống. Do sử dụng JWT nên Backend không lưu trạng thái session, API chỉ trả về message để client xóa token/cookie.
### Roles
All
### Response
- `200 OK`: `{ "message": "Đăng xuất thành công!" }`
### Error Cases
None

---

### Endpoint
`POST /api/Auth/change-password`
### Query Parameters
None
### Request: (Header, Body)
- Header: Bắt buộc gửi JWT Token (`Authorization: Bearer <token>`) hoặc Cookie.
- Body: `ChangePasswordDto` (chứa OldPassword, NewPassword)
### Purpose
Đổi mật khẩu cho người dùng hiện đang đăng nhập.
### Roles
Authenticated Users (Tất cả người dùng đã đăng nhập)
### Response
- `200 OK`: `{ "message": "Đổi mật khẩu thành công!" }`
### Error Cases
- `400 Bad Request`: Mật khẩu cũ không chính xác, lỗi hệ thống, hoặc dữ liệu nhập không hợp lệ (VD: mật khẩu quá ngắn).
- `401 Unauthorized`: `{"message": "Không xác định được người dùng."}` (Token hết hạn hoặc không hợp lệ).

---

## ParentProfileController
Quản lý thông tin hồ sơ dành riêng cho người dùng có vai trò Phụ huynh (Parent).

---

### Endpoint
`GET /api/parent-profile`
### Query Parameters
None
### Request: (Header, Body)
- Header: Bắt buộc gửi JWT Token (`Authorization: Bearer <token>`) hoặc Cookie.
- Body: None
### Purpose
Lấy thông tin cá nhân (Hồ sơ) của phụ huynh hiện tại.
### Roles
`Parent`
### Response
- `200 OK`: Trả về object `ParentProfileDto` (bao gồm FullName, Email, Phone).
### Error Cases
- `401 Unauthorized`: Không xác định được người dùng hoặc User không phải là Parent.
- `404 Not Found`: `{"message": "Không tìm thấy thông tin hồ sơ."}` (Không có trong DB).

---

### Endpoint
`PUT /api/parent-profile`
### Query Parameters
None
### Request: (Header, Body)
- Header: Bắt buộc gửi JWT Token (`Authorization: Bearer <token>`) hoặc Cookie.
- Body: `ParentProfileUpdateDto` (chứa FullName, Email, Phone)
### Purpose
Cập nhật thông tin cá nhân của phụ huynh hiện tại.
### Roles
`Parent`
### Response
- `200 OK`: `{ "message": "Cập nhật hồ sơ thành công!" }`
### Error Cases
- `400 Bad Request`: Dữ liệu gửi lên không hợp lệ, hoặc quá trình lưu DB thất bại.
- `401 Unauthorized`: Không xác định được người dùng hoặc User không phải là Parent.
