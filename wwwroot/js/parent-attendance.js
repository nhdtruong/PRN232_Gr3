// Dữ liệu con
const childrenData = [
    { id: 101, name: "Nguyễn Văn A (Lớp C# - PRN232)" },
    { id: 102, name: "Nguyễn Thị B (Lớp Web - PRN211)" }
];

// Dữ liệu điểm danh của từng con
const attendanceDataByChild = {
    101: [
        { id: 1, className: "Lớp C# - PRN232", lessonTitle: "Buổi 1: Giới thiệu khóa học", date: "2026-06-01", status: "Present", note: "Đến đúng giờ" },
        { id: 2, className: "Lớp C# - PRN232", lessonTitle: "Buổi 2: Biến & Kiểu dữ liệu", date: "2026-06-04", status: "Present", note: "Hăng hái phát biểu" },
        { id: 3, className: "Lớp C# - PRN232", lessonTitle: "Buổi 3: Vòng lặp & Mảng", date: "2026-06-08", status: "Late", note: "Trễ 15 phút do hỏng xe" },
        { id: 4, className: "Lớp C# - PRN232", lessonTitle: "Buổi 4: OOP cơ bản", date: "2026-06-11", status: "Absent", note: "Vắng có phép (ốm)" }
    ],
    102: [
        { id: 10, className: "Lớp Web - PRN211", lessonTitle: "Buổi 1: Tổng quan về Web", date: "2026-06-02", status: "Present", note: "Đến đúng giờ" }
    ]
};

document.addEventListener("DOMContentLoaded", () => {
    const select = document.getElementById("selectChild");
    if (!select) return;

    // Load dropdown con
    select.innerHTML = '<option value="">-- Chọn con --</option>' + 
        childrenData.map(c => `<option value="${c.id}">${c.name}</option>`).join("");

    select.addEventListener("change", () => {
        const childId = select.value;
        if (!childId) {
            resetView();
            return;
        }
        renderAttendance(childId);
    });
});

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

function renderAttendance(childId) {
    const records = attendanceDataByChild[childId] || [];
    const tbody = document.getElementById("attendanceTableBody");
    const statsRow = document.getElementById("statsRow");

    if (records.length === 0) {
        statsRow.style.display = "none";
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-5 text-muted">
                    <i class="bi bi-list-nested fs-2 d-block mb-2"></i>
                    Không có thông tin điểm danh của học sinh này.
                </td>
            </tr>`;
        return;
    }

    // Tính toán thống kê chuyên cần
    const total = records.length;
    const present = records.filter(r => r.status === "Present").length;
    const absent = records.filter(r => r.status === "Absent").length;
    const others = total - present - absent;

    document.getElementById("statTotal").textContent = total;
    document.getElementById("statPresent").textContent = present;
    document.getElementById("statAbsent").textContent = absent;
    document.getElementById("statOther").textContent = others;

    statsRow.style.display = "flex";

    // Vẽ bảng
    tbody.innerHTML = records.map((r, idx) => {
        let badgeHtml = "";
        if (r.status === "Present") {
            badgeHtml = `<span class="badge bg-success status-badge rounded-pill">✅ Có mặt</span>`;
        } else if (r.status === "Absent") {
            badgeHtml = `<span class="badge bg-danger status-badge rounded-pill">❌ Vắng mặt</span>`;
        } else if (r.status === "Late") {
            badgeHtml = `<span class="badge bg-warning text-dark status-badge rounded-pill">⏰ Đi trễ</span>`;
        } else {
            badgeHtml = `<span class="badge bg-secondary status-badge rounded-pill">📋 Có phép</span>`;
        }

        const formattedDate = new Date(r.date).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });

        return `
            <tr>
                <td class="px-4 text-muted fw-semibold">${idx + 1}</td>
                <td class="fw-bold text-dark">${r.className}</td>
                <td>${r.lessonTitle}</td>
                <td>${formattedDate}</td>
                <td class="text-center">${badgeHtml}</td>
                <td class="text-muted small">${r.note || "—"}</td>
            </tr>
        `;
    }).join("");
}
