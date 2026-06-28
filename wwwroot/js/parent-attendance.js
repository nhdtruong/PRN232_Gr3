let childrenData = [];

document.addEventListener("DOMContentLoaded", () => {
    loadChildrenFromServer();

    const select = document.getElementById("selectChild");
    if (select) {
        select.addEventListener("change", () => {
            const childId = select.value;
            if (!childId) {
                resetView();
                return;
            }
            renderAttendance(childId);
        });
    }
});

// Tải danh sách con
async function loadChildrenFromServer() {
    const select = document.getElementById("selectChild");
    if (!select) return;

    try {
        const response = await fetch("/api/parent/my-children");
        if (response.ok) {
            const data = await response.json();
            childrenData = data;
        } else {
            console.error("Lỗi khi tải danh sách con");
        }
    } catch (err) {
        console.error("Lỗi kết nối API con:", err);
    }

    select.innerHTML = '<option value="">-- Chọn con --</option>' + 
        childrenData.map(c => `<option value="${c.id}">${c.fullName || c.name}</option>`).join("");
}

function resetView() {
    document.getElementById("statsRow").style.display = "none";
    document.getElementById("attendanceTableBody").innerHTML = `
        <tr>
            <td colspan="6" class="text-center py-5 text-muted" id="noAttendanceMessage">
                <i class="bi bi-person-exclamation fs-1 d-block mb-2 text-secondary"></i>
                Vui lòng chọn con ở góc phải để xem lịch sử chuyên cần.
            </td>
        </tr>`;
}

// Gọi API thật của Người 4: GET /api/parent/children/{studentId}/attendance
async function renderAttendance(childId) {
    const tbody = document.getElementById("attendanceTableBody");
    const statsRow = document.getElementById("statsRow");
    if (tbody) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-5">
                    <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span>
                    Đang nạp lịch sử điểm danh chuyên cần của con...
                </td>
            </tr>`;
    }

    try {
        const response = await fetch(`/api/parent/children/${childId}/attendance`);
        if (response.ok) {
            const records = await response.json();

            if (records.length === 0) {
                statsRow.style.display = "none";
                tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center py-5 text-muted">
                            <i class="bi bi-list-nested fs-2 d-block mb-2"></i>
                            Con chưa có lịch sử điểm danh nào trên hệ thống.
                        </td>
                    </tr>`;
                return;
            }

            // Tính toán thống kê chuyên cần thực tế
            const total = records.length;
            
            // Map AttendanceStatus Enum hoặc String (Present = 0, Absent = 1, Late = 2, Excused = 3 tùy thuộc Enum định nghĩa)
            // Nhận diện theo tên String hoặc số tương ứng
            const present = records.filter(r => r.status === "Present" || r.status === 0).length;
            const absent = records.filter(r => r.status === "Absent" || r.status === 1).length;
            const others = total - present - absent;

            document.getElementById("statTotal").textContent = total;
            document.getElementById("statPresent").textContent = present;
            document.getElementById("statAbsent").textContent = absent;
            document.getElementById("statOther").textContent = others;

            statsRow.style.display = "flex";

            // Vẽ bảng dữ liệu thật
            tbody.innerHTML = records.map((r, idx) => {
                let badgeHtml = "";
                let statusName = r.status;
                
                // Chuẩn hóa hiển thị trạng thái
                if (r.status === "Present" || r.status === 0) {
                    badgeHtml = `<span class="badge bg-success status-badge rounded-pill">✅ Có mặt</span>`;
                } else if (r.status === "Absent" || r.status === 1) {
                    badgeHtml = `<span class="badge bg-danger status-badge rounded-pill">❌ Vắng mặt</span>`;
                } else if (r.status === "Late" || r.status === 2) {
                    badgeHtml = `<span class="badge bg-warning text-dark status-badge rounded-pill">⏰ Đi trễ</span>`;
                } else {
                    badgeHtml = `<span class="badge bg-secondary status-badge rounded-pill">📋 Có phép</span>`;
                }

                // Giả lập hoặc lấy tên lớp
                const className = r.className || "Lớp học liên kết";
                const lessonTitle = r.lessonTitle || "Buổi học";
                
                const date = r.updatedAt ? new Date(r.updatedAt).toLocaleDateString("vi-VN", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric"
                }) : "—";

                return `
                    <tr>
                        <td class="px-4 text-muted fw-semibold">${idx + 1}</td>
                        <td class="fw-bold text-dark">${className}</td>
                        <td>${lessonTitle}</td>
                        <td>${date}</td>
                        <td class="text-center">${badgeHtml}</td>
                        <td class="text-muted small">${r.note || "—"}</td>
                    </tr>
                `;
            }).join("");
        } else {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger">Không có quyền xem thông tin học sinh này.</td></tr>`;
        }
    } catch (err) {
        console.error("Lỗi:", err);
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger">Lỗi kết nối máy chủ.</td></tr>`;
    }
}
