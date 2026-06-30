// Lấy ClassId từ metadata ẩn của trang HTML
const metadataEl = document.getElementById("lesson-metadata");
const classId = metadataEl ? parseInt(metadataEl.getAttribute("data-class-id")) : 0;

let lessonsData = [];
let searchQuery = "";
let currentPage = 1;
const pageSize = 6;

document.addEventListener("DOMContentLoaded", () => {
    if (classId > 0) {
        loadLessonsFromServer();
    }

    const searchInput = document.getElementById("searchLessonInput");
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            searchQuery = e.target.value.toLowerCase().trim();
            currentPage = 1;
            renderLessons();
        });
    }

    // Xử lý submit Form Thêm buổi học mới
    const createForm = document.getElementById("createLessonForm");
    if (createForm) {
        createForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const titleInput = document.getElementById("createTitle");
            const descInput = document.getElementById("createDescription");
            const dateInput = document.getElementById("createDate");
            const roomIdSelect = document.getElementById("createRoomId");
            const slotIdSelect = document.getElementById("createSlotId");

            const payload = {
                title: titleInput.value,
                description: descInput.value,
                lessonDate: new Date(dateInput.value).toISOString(),
                roomId: roomIdSelect.value ? parseInt(roomIdSelect.value) : null,
                slotId: slotIdSelect.value ? parseInt(slotIdSelect.value) : null
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
                    showToast("Thành công", "Đã tạo buổi học mới và xếp phòng/ca học thành công!", "success");
                    createForm.reset();
                    
                    const modalEl = document.getElementById("createLessonModal");
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    modal.hide();

                    await loadLessonsFromServer();
                } else {
                    const error = await response.json();
                    showToast("Không thể xếp lịch", error.message || "Trùng lịch học hoặc dữ liệu không hợp lệ.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            }
        });
    }

    // Xử lý submit Form Chỉnh sửa buổi học
    const editForm = document.getElementById("editLessonForm");
    if (editForm) {
        editForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const idInput = document.getElementById("editLessonId");
            const titleInput = document.getElementById("editTitle");
            const descInput = document.getElementById("editDescription");
            const dateInput = document.getElementById("editDate");
            const roomIdSelect = document.getElementById("editRoomId");
            const slotIdSelect = document.getElementById("editSlotId");

            const payload = {
                id: parseInt(idInput.value),
                title: titleInput.value,
                description: descInput.value,
                lessonDate: new Date(dateInput.value).toISOString(),
                roomId: roomIdSelect.value ? parseInt(roomIdSelect.value) : null,
                slotId: slotIdSelect.value ? parseInt(slotIdSelect.value) : null
            };

            try {
                const response = await fetch(`/api/center/lessons/${payload.id}`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    showToast("Thành công", "Đã cập nhật thông tin và lịch học của buổi học thành công!", "success");
                    
                    const modalEl = document.getElementById("editLessonModal");
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    modal.hide();

                    await loadLessonsFromServer();
                } else {
                    const error = await response.json();
                    showToast("Không thể cập nhật", error.message || "Trùng lịch học hoặc dữ liệu không hợp lệ.", "danger");
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

    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const pageItems = filtered.slice(startIndex, endIndex);

    gridContainer.innerHTML = pageItems.map((lesson, idx) => {
        const lessonDate = new Date(lesson.lessonDate);
        const formattedDate = lessonDate.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });

        const absoluteIndex = startIndex + idx + 1;

        const statusBadge = lesson.isPublished 
            ? `<span class="badge rounded-pill px-2 py-1 small fw-semibold" style="font-size: 0.72rem; background-color: rgba(16, 185, 129, 0.15); color: #047857;"><i class="bi bi-broadcast-pin me-1"></i>Đã phát sóng</span>`
            : `<span class="badge rounded-pill px-2 py-1 small fw-semibold" style="font-size: 0.72rem; background-color: rgba(107, 114, 128, 0.15); color: #4B5563;"><i class="bi bi-pencil-fill me-1"></i>Nháp</span>`;

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

        const roomAndSlotInfo = (lesson.roomName && lesson.slotName)
            ? `<div class="mt-2 text-primary small fw-semibold">
                <i class="bi bi-geo-alt-fill me-1"></i> ${lesson.roomName} | <i class="bi bi-clock-fill me-1"></i> ${lesson.slotName} (${lesson.startTime.substring(0, 5)} - ${lesson.endTime.substring(0, 5)})
               </div>`
            : `<div class="mt-2 text-danger small fw-semibold"><i class="bi bi-exclamation-triangle-fill me-1"></i> Chưa xếp lịch phòng/ca</div>`;

        return `
            <div class="col-lg-4 col-md-6 col-sm-12">
                <div class="admin-lesson-card shadow-sm">
                    <div class="card-body p-4 d-flex flex-column h-100">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <div class="d-flex align-items-center gap-2">
                                <span class="badge rounded-pill px-3 py-1.5 small fw-bold" style="background-color: rgba(79, 70, 229, 0.15); color: #4F46E5;">Buổi ${absoluteIndex}</span>
                                ${statusBadge}
                            </div>
                            <span class="text-muted small" style="font-size: 0.78rem;"><i class="bi bi-calendar me-1"></i>${formattedDate}</span>
                        </div>
                        
                        <h5 class="fw-bold text-dark mb-2 text-truncate" title="${lesson.title}">${lesson.title}</h5>
                        <p class="text-muted small flex-grow-1 text-truncate-3" style="font-size: 0.85rem; line-height: 1.5; min-height: 50px;">
                            ${lesson.description || "<i>Không có mô tả nội dung cho buổi học này.</i>"}
                        </p>
                        
                        ${roomAndSlotInfo}

                        <div class="mt-3">
                            ${publishButtonHtml}
                        </div>

                        <!-- Nút hành động -->
                        <div class="card-footer-buttons d-flex justify-content-between align-items-center mt-3">
                            <div class="d-flex gap-2">
                                <a href="/Center/Lessons/Materials/${lesson.id}" class="btn btn-success btn-sm rounded-pill px-3 fw-semibold">
                                    <i class="bi bi-folder2-open me-1"></i> Tài liệu
                                </a>
                                <a href="/Center/Lessons/RollCall?LessonId=${lesson.id}" class="btn btn-primary btn-sm rounded-pill px-3 fw-semibold">
                                    <i class="bi bi-person-check me-1"></i> Điểm danh
                                </a>
                            </div>
                            <div class="d-flex gap-1.5">
                                <button class="btn btn-outline-warning btn-sm btn-action-circle" onclick="triggerEditModal(${lesson.id})" title="Sửa thông tin">
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

    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <button class="page-link rounded-circle d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;" data-page="${currentPage - 1}" aria-label="Previous">
                <i class="bi bi-chevron-left"></i>
            </button>
        </li>`;

    for (let i = 1; i <= totalPages; i++) {
        html += `
            <li class="page-item ${currentPage === i ? 'active' : ''}">
                <button class="page-link rounded-circle d-flex align-items-center justify-content-center fw-semibold ${currentPage === i ? 'bg-primary border-primary text-white' : 'text-primary'}" style="width: 32px; height: 32px;" data-page="${i}">${i}</button>
            </li>`;
    }

    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <button class="page-link rounded-circle d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;" data-page="${currentPage + 1}" aria-label="Next">
                <i class="bi bi-chevron-right"></i>
            </button>
        </li>`;

    html += `</ul></nav>`;
    pagination.innerHTML = html;

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

// Mở Modal Sửa buổi học
function triggerEditModal(id) {
    const lesson = lessonsData.find(l => l.id === id);
    if (!lesson) return;

    document.getElementById("editLessonId").value = lesson.id;
    document.getElementById("editTitle").value = lesson.title;
    document.getElementById("editDescription").value = lesson.description || "";
    
    // Convert ISO string/date to YYYY-MM-DD
    const rawDate = new Date(lesson.lessonDate);
    const yyyy = rawDate.getFullYear();
    const mm = String(rawDate.getMonth() + 1).padStart(2, '0');
    const dd = String(rawDate.getDate()).padStart(2, '0');
    document.getElementById("editDate").value = `${yyyy}-${mm}-${dd}`;

    document.getElementById("editRoomId").value = lesson.roomId || "";
    document.getElementById("editSlotId").value = lesson.slotId || "";

    const modalEl = document.getElementById("editLessonModal");
    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

// Phát sóng thông báo buổi học tới phụ huynh
async function publishLesson(lessonId, isRebroadcast = false) {
    const confirmMsg = isRebroadcast 
        ? "Bạn muốn phát sóng LẠI thông báo cho buổi học này tới phụ huynh? Tin nhắn cũ sẽ được cập nhật."
        : "Bạn muốn phát sóng buổi học này? Hệ thống sẽ gửi thông báo và tài liệu đi kèm tới toàn bộ phụ huynh trong lớp.";
        
    if (!confirm(confirmMsg)) return;

    try {
        const response = await fetch(`/api/center/lessons/${lessonId}/publish`, {
            method: "POST"
        });

        if (response.ok) {
            const data = await response.json();
            showToast("Đã phát sóng", data.message, "success");
            await loadLessonsFromServer();
        } else {
            const err = await response.json();
            showToast("Không thể phát sóng", err.message || "Yêu cầu thất bại.", "danger");
        }
    } catch (e) {
        console.error(e);
        showToast("Lỗi kết nối", "Không thể kết nối tới máy chủ.", "danger");
    }
}

// Helper Toast thông báo đẹp mắt góc màn hình
function showToast(title, message, type = "success") {
    let toastContainer = document.getElementById("toast-container-box");
    if (!toastContainer) {
        toastContainer = document.createElement("div");
        toastContainer.id = "toast-container-box";
        toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3";
        toastContainer.style.zIndex = "1090";
        document.body.appendChild(toastContainer);
    }

    const toastId = "toast_" + Date.now();
    const iconClass = type === "success" 
        ? "bi-check-circle-fill text-success" 
        : (type === "danger" ? "bi-exclamation-triangle-fill text-danger" : "bi-info-circle-fill text-info");

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center border-0 shadow rounded-4" role="alert" aria-live="assertive" aria-atomic="true" style="background-color: #ffffff;">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <i class="bi ${iconClass} fs-5"></i>
                    <div>
                        <strong class="text-dark d-block">${title}</strong>
                        <span class="text-muted small">${message}</span>
                    </div>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML("beforeend", toastHtml);
    const toastEl = document.getElementById(toastId);
    const bsToast = new bootstrap.Toast(toastEl, { delay: 4000 });
    bsToast.show();

    toastEl.addEventListener("hidden.bs.toast", () => {
        toastEl.remove();
    });
}
