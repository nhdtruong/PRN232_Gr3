// Dữ liệu mẫu (Mock data) cho các buổi học
let lessonsData = [
    { id: 1, title: "Buổi 1: Giới thiệu khóa học & Cài đặt môi trường", description: "Làm quen với cú pháp C#, cài đặt Visual Studio và viết chương trình Hello World.", date: "2026-06-01T18:00:00" },
    { id: 2, title: "Buổi 2: Biến, Kiểu dữ liệu và Các cấu trúc điều khiển", description: "Học về kiểu dữ liệu số, chuỗi, boolean, câu lệnh if-else và switch-case.", date: "2026-06-04T18:00:00" },
    { id: 3, title: "Buổi 3: Vòng lặp & Mảng trong C#", description: "Học vòng lặp for, while, do-while và làm việc với cấu trúc dữ liệu mảng.", date: "2026-06-08T18:00:00" },
    { id: 4, title: "Buổi 4: Lập trình hướng đối tượng - OOP cơ bản", description: "Học về Class, Object, Constructor và 4 tính chất OOP.", date: "2026-06-11T18:00:00" }
];

document.addEventListener("DOMContentLoaded", () => {
    renderLessons();

    // Xử lý submit Form Thêm buổi học
    const createForm = document.getElementById("createLessonForm");
    if (createForm) {
        createForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const titleInput = createForm.querySelector('input[type="text"]');
            const descInput = createForm.querySelector('textarea');
            const dateInput = createForm.querySelector('input[type="datetime-local"]');

            const newLesson = {
                id: lessonsData.length + 1,
                title: titleInput.value,
                description: descInput.value,
                date: dateInput.value
            };

            lessonsData.push(newLesson);
            renderLessons();

            // Reset form và đóng modal
            createForm.reset();
            const modalEl = document.getElementById("createLessonModal");
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();

            showToast("Thành công", "Đã tạo buổi học mới thành công!", "success");
        });
    }
});

// Render danh sách buổi học ra bảng HTML
function renderLessons() {
    const tbody = document.querySelector("table tbody");
    if (!tbody) return;

    if (lessonsData.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-5 text-muted">
                    <i class="bi bi-calendar-x fs-1 d-block mb-2 text-secondary"></i>
                    Chưa có buổi học nào được tạo cho lớp này.
                </td>
            </tr>`;
        return;
    }

    tbody.innerHTML = lessonsData.map((lesson, idx) => {
        const lessonDate = new Date(lesson.date);
        const formattedDate = lessonDate.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });

        return `
            <tr>
                <td class="px-4 text-muted fw-semibold">${idx + 1}</td>
                <td>
                    <div class="fw-bold text-dark">${lesson.title}</div>
                </td>
                <td class="text-muted" style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                    ${lesson.description || "<i>Không có mô tả</i>"}
                </td>
                <td>
                    <span class="badge bg-light text-dark border"><i class="bi bi-calendar3 me-1"></i>${formattedDate}</span>
                </td>
                <td class="text-center">
                    <div class="d-flex justify-content-center gap-2">
                        <a href="/Center/Lessons/Materials/${lesson.id}" class="btn btn-outline-success btn-sm rounded-pill px-3">
                            <i class="bi bi-folder2-open me-1"></i> Tài liệu
                        </a>
                        <a href="/Center/Lessons/RollCall?LessonId=${lesson.id}" class="btn btn-outline-primary btn-sm rounded-pill px-3">
                            <i class="bi bi-person-check me-1"></i> Điểm danh
                        </a>
                        <button class="btn btn-outline-warning btn-sm rounded-circle" onclick="editLesson(${lesson.id})" title="Sửa">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                        <button class="btn btn-outline-danger btn-sm rounded-circle" onclick="deleteLesson(${lesson.id})" title="Xóa">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }).join("");
}

// Xử lý Xóa buổi học
function deleteLesson(id) {
    if (confirm("Bạn có chắc chắn muốn xóa/hủy buổi học này? Thao tác này sẽ xóa tất cả điểm danh và tài liệu liên quan!")) {
        lessonsData = lessonsData.filter(l => l.id !== id);
        renderLessons();
        showToast("Đã xóa", "Buổi học đã bị xóa khỏi hệ thống.", "danger");
    }
}

// Xử lý Sửa buổi học (Mock)
function editLesson(id) {
    const lesson = lessonsData.find(l => l.id === id);
    if (!lesson) return;
    const newTitle = prompt("Nhập tiêu đề mới cho buổi học:", lesson.title);
    if (newTitle) {
        lesson.title = newTitle;
        renderLessons();
        showToast("Thành công", "Đã cập nhật tiêu đề buổi học.", "info");
    }
}

// Hàm vẽ Toast thông báo góc màn hình
function showToast(title, content, type = "success") {
    let alertClass = "bg-success";
    if (type === "danger") alertClass = "bg-danger";
    if (type === "info") alertClass = "bg-info";

    const toastContainer = document.createElement("div");
    toastContainer.className = "position-fixed bottom-0 end-0 p-3";
    toastContainer.style.zIndex = "1100";
    toastContainer.innerHTML = `
        <div class="toast show align-items-center text-white ${alertClass} border-0 rounded-3 shadow" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}:</strong> ${content}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    document.body.appendChild(toastContainer);
    setTimeout(() => toastContainer.remove(), 4000);
}
