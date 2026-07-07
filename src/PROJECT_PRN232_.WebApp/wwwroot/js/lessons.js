// Lấy ClassId từ metadata ẩn của trang HTML
const metadataEl = document.getElementById("lesson-metadata");
const classId = metadataEl ? parseInt(metadataEl.getAttribute("data-class-id")) : 0;

let lessonsData = [];
let searchQuery = "";
let currentPage = 1;
const pageSize = 6; // Hiển thị tối đa 6 buổi học trên một trang để không bị kéo dài

document.addEventListener("DOMContentLoaded", () => {
    if (classId > 0) {
        loadLessonsFromServer();
    }

    // Lắng nghe sự kiện tìm kiếm buổi học
    const searchInput = document.getElementById("searchLessonInput");
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            searchQuery = e.target.value.toLowerCase().trim();
            currentPage = 1; // Reset về trang 1 khi gõ tìm kiếm
            renderLessons();
        });
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

                    await loadLessonsFromServer();
                } else {
                    const error = await response.json();
                    showToast("Thất bại", error.message || "Không thể tạo buổi học mới.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            }
        });
    }
});

// Tải danh sách buổi học thật từ database
async function loadLessonsFromServer() {
    const gridContainer = document.getElementById("lessonsGridContainer");
    if (gridContainer) {
        gridContainer.innerHTML = `
            <div class="col-12 text-center py-5">
                <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span>
                Đang tải danh sách buổi học từ máy chủ...
            </div>`;
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
        console.error("Lỗi:", err);
        showToast("Lỗi kết nối", "Không thể tải lịch học.", "danger");
    }
}

// Render danh sách buổi học dạng Grid of Cards kèm Phân trang
function renderLessons() {
    const gridContainer = document.getElementById("lessonsGridContainer");
    const countSpan = document.getElementById("lessonsCount");
    const pagination = document.getElementById("lessonsPagination");
    if (!gridContainer) return;

    // Lọc theo từ khóa tìm kiếm
    const filtered = lessonsData.filter(l => 
        l.title.toLowerCase().includes(searchQuery) || 
        (l.description && l.description.toLowerCase().includes(searchQuery))
    );

    if (countSpan) {
        countSpan.textContent = filtered.length;
    }

    if (filtered.length === 0) {
        gridContainer.innerHTML = `
            <div class="col-12 text-center py-5 text-muted">
                <i class="bi bi-calendar-x fs-1 d-block mb-3 text-secondary opacity-50"></i>
                <p class="mb-0 fw-semibold">${searchQuery !== "" ? "Không tìm thấy buổi học nào khớp với từ khóa tìm kiếm." : "Chưa có buổi học nào được tạo cho lớp này."}</p>
            </div>`;
        if (pagination) pagination.innerHTML = "";
        return;
    }

    // Thực hiện phân trang
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const pageItems = filtered.slice(startIndex, endIndex);

    gridContainer.innerHTML = pageItems.map((lesson, idx) => {
        const lessonDate = new Date(lesson.lessonDate);
        const formattedDate = lessonDate.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });

        // Tính toán đúng số thứ tự buổi học thực tế trên toàn bộ danh sách
        const absoluteIndex = startIndex + idx + 1;

        // Trạng thái xuất bản
        const statusBadge = lesson.isPublished 
            ? `<span class="badge rounded-pill px-2 py-1 small fw-semibold" style="font-size: 0.72rem; background-color: rgba(16, 185, 129, 0.15); color: #047857;"><i class="bi bi-broadcast-pin me-1"></i>Đã phát sóng</span>`
            : `<span class="badge rounded-pill px-2 py-1 small fw-semibold" style="font-size: 0.72rem; background-color: rgba(107, 114, 128, 0.15); color: #4B5563;"><i class="bi bi-pencil-fill me-1"></i>Nháp</span>`;

        // Nút phát sóng thông báo (hiện nút Phát sóng lại nếu đã xuất bản)
        const publishButtonHtml = lesson.isPublished 
            ? `
            <button class="btn btn-outline-primary btn-sm rounded-pill w-100 mb-3 fw-bold shadow-sm" 
                    onclick="publishLesson(${lesson.id}, true)" 
                    style="border: 2px solid #4F46E5; color: #4F46E5; background-color: transparent; font-size: 0.8rem; padding: 5px 12px;">
                <i class="bi bi-arrow-repeat me-1"></i> Phát sóng lại thông báo
            </button>
            `
            : `
            <button class="btn btn-primary btn-sm rounded-pill w-100 mb-3 fw-bold shadow-sm" 
                    onclick="publishLesson(${lesson.id}, false)" 
                    style="background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); border: none; font-size: 0.8rem; padding: 6px 12px;">
                <i class="bi bi-broadcast me-1"></i> Phát sóng thông báo
            </button>
            `;

        return `
            <div class="col-lg-4 col-md-6 col-sm-12">
                <div class="admin-lesson-card shadow-sm">
                    <div class="card-body p-4 d-flex flex-column h-100">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <div class="d-flex align-items-center gap-2">
                                <span class="badge rounded-pill px-3 py-1.5 small fw-bold" style="background-color: rgba(79, 70, 229, 0.15); color: #4F46E5;">Buổi ${absoluteIndex}</span>
                                ${statusBadge}
                            </div>
                            <span class="text-muted small" style="font-size: 0.78rem;"><i class="bi bi-clock me-1"></i>${formattedDate}</span>
                        </div>
                        
                        <h5 class="fw-bold text-dark mb-2 text-truncate" title="${lesson.title}">${lesson.title}</h5>
                        <p class="text-muted small flex-grow-1 text-truncate-3" style="font-size: 0.85rem; line-height: 1.5; min-height: 50px;">
                            ${lesson.description || "<i>Không có mô tả nội dung cho buổi học này.</i>"}
                        </p>
                        
                        <!-- Nút phát sóng thông báo nếu có -->
                        ${publishButtonHtml}

                        <!-- Nút hành động -->
                        <div class="card-footer-buttons d-flex justify-content-between align-items-center mt-3">
                            <div class="d-flex gap-2">
                                <a href="/Center/Lessons/Materials/${lesson.id}" class="btn btn-success btn-sm rounded-pill px-3 fw-semibold">
                                    <i class="bi bi-folder2-open me-1"></i> Tài liệu
                                </a>
                                <a href="/Center/Lessons/RollCall?LessonId=${lesson.id}" class="btn btn-primary btn-sm rounded-pill px-3 fw-semibold">
                                    <i class="bi bi-person-check me-1"></i> Điểm danh
                                </a>
                                <a href="/Center/Lessons/GradeSheet?LessonId=${lesson.id}" class="btn btn-warning btn-sm rounded-pill px-3 fw-semibold text-dark">
                                    <i class="bi bi-award me-1"></i> Kết quả
                                </a>
                            </div>
                            <div class="d-flex gap-1.5">
                                <button class="btn btn-outline-warning btn-sm btn-action-circle" onclick="editLesson(${lesson.id})" title="Sửa thông tin">
                                    <i class="bi bi-pencil-square"></i>
                                </button>
                                <button class="btn btn-outline-danger btn-sm btn-action-circle" onclick="deleteLesson(${lesson.id})" title="Xóa buổi học">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }).join("");

    renderLessonsPagination(filtered.length);
}

// Vẽ thanh phân trang cho Buổi học
function renderLessonsPagination(totalItems) {
    const pagination = document.getElementById("lessonsPagination");
    if (!pagination) return;

    const totalPages = Math.ceil(totalItems / pageSize);
    if (totalPages <= 1) {
        pagination.innerHTML = "";
        return;
    }

    let html = `<nav aria-label="Page navigation"><ul class="pagination pagination-sm mb-0 gap-1">`;

    // Nút Prev
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <button class="page-link rounded-circle d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;" data-page="${currentPage - 1}" aria-label="Previous">
                <i class="bi bi-chevron-left"></i>
            </button>
        </li>`;

    // Nút các trang số
    for (let i = 1; i <= totalPages; i++) {
        html += `
            <li class="page-item ${currentPage === i ? 'active' : ''}">
                <button class="page-link rounded-circle d-flex align-items-center justify-content-center fw-semibold ${currentPage === i ? 'bg-primary border-primary text-white' : 'text-primary'}" style="width: 32px; height: 32px;" data-page="${i}">${i}</button>
            </li>`;
    }

    // Nút Next
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <button class="page-link rounded-circle d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;" data-page="${currentPage + 1}" aria-label="Next">
                <i class="bi bi-chevron-right"></i>
            </button>
        </li>`;

    html += `</ul></nav>`;
    pagination.innerHTML = html;

    // Gắn sự kiện click
    pagination.querySelectorAll("button").forEach(btn => {
        btn.addEventListener("click", () => {
            const page = parseInt(btn.getAttribute("data-page"));
            if (page >= 1 && page <= totalPages) {
                currentPage = page;
                renderLessons();
            }
        });
    });
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

// Phát sóng thông báo buổi học tới phụ huynh
async function publishLesson(id, isRebroadcast = false) {
    const titleText = isRebroadcast ? 'Phát sóng lại thông báo?' : 'Phát sóng thông báo?';
    const textMsg = isRebroadcast 
        ? "Tài liệu buổi học đã thay đổi. Hệ thống sẽ gửi thông báo cập nhật mới tới toàn bộ Phụ huynh có con trong lớp."
        : "Hệ thống sẽ gửi thông báo tổng hợp kèm nút xem nhanh tài liệu tới toàn bộ Phụ huynh có con trong lớp này.";
    const successTitle = isRebroadcast ? 'Đã phát sóng lại!' : 'Đã phát sóng!';
    const successText = isRebroadcast 
        ? 'Thông báo cập nhật đã được gửi tới toàn bộ Phụ huynh!'
        : 'Thông báo tổng hợp đã được gửi tới toàn bộ Phụ huynh!';

    Swal.fire({
        title: titleText,
        text: textMsg,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#4F46E5',
        cancelButtonColor: '#6B7280',
        confirmButtonText: 'Đồng ý phát sóng',
        cancelButtonText: 'Hủy'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const response = await fetch(`/api/center/lessons/${id}/publish`, {
                    method: "POST"
                });

                if (response.ok) {
                    Swal.fire({
                        title: successTitle,
                        text: successText,
                        icon: 'success',
                        confirmButtonColor: '#4F46E5'
                    });
                    await loadLessonsFromServer(); // Tải lại danh sách để cập nhật giao diện
                } else {
                    const error = await response.json();
                    Swal.fire('Thất bại', error.message || "Không thể xuất bản buổi học.", 'error');
                }
            } catch (err) {
                console.error("Lỗi khi xuất bản:", err);
                Swal.fire('Lỗi', "Không thể kết nối máy chủ để xuất bản.", 'error');
            }
        }
    });
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
