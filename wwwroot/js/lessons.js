// Lấy ClassId từ metadata ẩn của trang HTML
const metadataEl = document.getElementById("lesson-metadata");
const classId = metadataEl ? parseInt(metadataEl.getAttribute("data-class-id")) : 0;

let lessonsData = [];

document.addEventListener("DOMContentLoaded", () => {
    if (classId > 0) {
        loadLessonsFromServer();
    }

    // Xử lý submit Form Thêm buổi học mới
    const createForm = document.getElementById("createLessonForm");
    if (createForm) {
        createForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const titleInput = createForm.querySelector('input[type="text"]');
            const descInput = createForm.querySelector('textarea');
            const dateInput = createForm.querySelector('input[type="datetime-local"]');

            const payload = {
                title: titleInput.value,
                description: descInput.value,
                lessonDate: new Date(dateInput.value).toISOString()
            };

            try {
                const response = await fetch(`/api/center/classes/${classId}/lessons`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    showToast("Thành công", "Đã tạo buổi học mới và lưu vào cơ sở dữ liệu!", "success");
                    createForm.reset();
                    
                    // Đóng modal
                    const modalEl = document.getElementById("createLessonModal");
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    modal.hide();

                    // Tải lại danh sách thật từ DB
                    await loadLessonsFromServer();
                } else {
                    const error = await response.json();
                    showToast("Thất bại", error.message || "Không thể tạo buổi học mới.", "danger");
                }
            } catch (err) {
                console.error("Lỗi khi kết nối API:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            }
        });
    }
});

// Tải danh sách buổi học thật từ database
async function loadLessonsFromServer() {
    const tbody = document.querySelector("table tbody");
    if (tbody) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-5">
                    <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span>
                    Đang tải danh sách buổi học từ máy chủ...
                </td>
            </tr>`;
    }

    try {
        const response = await fetch(`/api/center/classes/${classId}/lessons`);
        if (response.ok) {
            lessonsData = await response.json();
            renderLessons();
        } else {
            showToast("Lỗi", "Không thể tải dữ liệu buổi học từ máy chủ.", "danger");
        }
    } catch (err) {
        console.error("Lỗi fetch:", err);
        showToast("Lỗi kết nối", "Không thể tải lịch học.", "danger");
    }
}

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
        const lessonDate = new Date(lesson.lessonDate);
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

// Xóa buổi học thật trong Database
async function deleteLesson(id) {
    if (confirm("Bạn có chắc chắn muốn xóa/hủy buổi học này? Thao tác này sẽ xóa tất cả điểm danh và tài liệu liên quan trong cơ sở dữ liệu!")) {
        try {
            const response = await fetch(`/api/center/lessons/${id}`, {
                method: "DELETE"
            });

            if (response.ok) {
                showToast("Thành công", "Đã xóa buổi học thành công khỏi database.", "success");
                await loadLessonsFromServer();
            } else {
                const error = await response.json();
                showToast("Thất bại", error.message || "Không thể xóa buổi học.", "danger");
            }
        } catch (err) {
            console.error("Lỗi khi xóa:", err);
            showToast("Lỗi", "Không thể kết nối máy chủ để xóa.", "danger");
        }
    }
}

// Sửa tiêu đề buổi học thật trong Database
async function editLesson(id) {
    const lesson = lessonsData.find(l => l.id === id);
    if (!lesson) return;

    const newTitle = prompt("Nhập tiêu đề mới cho buổi học:", lesson.title);
    if (!newTitle || newTitle.trim() === "") return;

    const payload = {
        title: newTitle,
        description: lesson.description,
        lessonDate: lesson.lessonDate
    };

    try {
        const response = await fetch(`/api/center/lessons/${id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            showToast("Thành công", "Đã cập nhật thông tin buổi học vào cơ sở dữ liệu!", "success");
            await loadLessonsFromServer();
        } else {
            showToast("Thất bại", "Không thể cập nhật thông tin buổi học.", "danger");
        }
    } catch (err) {
        console.error("Lỗi khi cập nhật:", err);
        showToast("Lỗi", "Không thể kết nối máy chủ để cập nhật.", "danger");
    }
}

// Hàm vẽ Toast thông báo
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
