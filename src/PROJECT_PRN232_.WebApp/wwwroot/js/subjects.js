// ============================================================
//  subjects.js — Quản lý Môn học (Center)
// ============================================================

let subjectsData = [];
let searchQuery = "";
let currentPage = 1;
const pageSize = 9; // 3×3 grid

// ─── KHỞI TẠO KHI DOM SẴN SÀNG ────────────────────────────
document.addEventListener("DOMContentLoaded", () => {
    loadSubjectsFromServer();

    // Tìm kiếm theo tên/mã môn
    const searchInput = document.getElementById("searchSubjectInput");
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            searchQuery = e.target.value.toLowerCase().trim();
            currentPage = 1;
            renderSubjects();
        });
    }

    // Submit Form Thêm môn học mới
    const createForm = document.getElementById("createSubjectForm");
    if (createForm) {
        createForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const btn = document.getElementById("createSubjectSubmitBtn");
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Đang tạo...`;

            const payload = {
                subjectCode: document.getElementById("createSubjectCode").value.trim(),
                subjectName: document.getElementById("createSubjectName").value.trim(),
                numberOfSessions: parseInt(document.getElementById("createNumberOfSessions").value),
                description: document.getElementById("createDescription").value.trim() || null
            };

            try {
                const response = await fetch("/api/center/subjects", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    showToast("Thành công", `Đã tạo môn học '${payload.subjectName}' thành công!`, "success");
                    createForm.reset();
                    bootstrap.Modal.getInstance(document.getElementById("createSubjectModal")).hide();
                    await loadSubjectsFromServer();
                } else {
                    const err = await response.json();
                    showToast("Không thể tạo môn học", err.message || "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            } finally {
                btn.disabled = false;
                btn.innerHTML = `<i class="bi bi-plus-lg me-1"></i>Tạo môn học`;
            }
        });
    }

    // Submit Form Sửa môn học
    const editForm = document.getElementById("editSubjectForm");
    if (editForm) {
        editForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const btn = document.getElementById("editSubjectSubmitBtn");
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Đang lưu...`;

            const subjectId = parseInt(document.getElementById("editSubjectId").value);
            const payload = {
                subjectCode: document.getElementById("editSubjectCode").value.trim(),
                subjectName: document.getElementById("editSubjectName").value.trim(),
                numberOfSessions: parseInt(document.getElementById("editNumberOfSessions").value),
                description: document.getElementById("editDescription").value.trim() || null
            };

            try {
                const response = await fetch(`/api/center/subjects/${subjectId}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });

                if (response.ok || response.status === 204) {
                    showToast("Cập nhật thành công", `Môn học '${payload.subjectName}' đã được cập nhật.`, "success");
                    bootstrap.Modal.getInstance(document.getElementById("editSubjectModal")).hide();
                    await loadSubjectsFromServer();
                } else {
                    const err = await response.json();
                    showToast("Không thể cập nhật", err.message || "Dữ liệu không hợp lệ.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            } finally {
                btn.disabled = false;
                btn.innerHTML = `<i class="bi bi-save me-1"></i>Lưu thay đổi`;
            }
        });
    }

    // Xác nhận xóa môn học
    const confirmDeleteBtn = document.getElementById("confirmDeleteSubjectBtn");
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener("click", async () => {
            const subjectId = parseInt(document.getElementById("deleteSubjectId").value);
            confirmDeleteBtn.disabled = true;
            confirmDeleteBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Đang xóa...`;

            try {
                const response = await fetch(`/api/center/subjects/${subjectId}`, {
                    method: "DELETE"
                });

                if (response.ok || response.status === 204) {
                    showToast("Đã xóa", "Môn học đã được xóa thành công.", "success");
                    bootstrap.Modal.getInstance(document.getElementById("deleteSubjectModal")).hide();
                    await loadSubjectsFromServer();
                } else {
                    const err = await response.json();
                    showToast("Không thể xóa", err.message || "Có lỗi xảy ra khi xóa môn học.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            } finally {
                confirmDeleteBtn.disabled = false;
                confirmDeleteBtn.innerHTML = `<i class="bi bi-trash me-1"></i>Xóa`;
            }
        });
    }
});

// ─── TẢI DỮ LIỆU TỪ SERVER ─────────────────────────────────
async function loadSubjectsFromServer() {
    try {
        const response = await fetch("/api/center/subjects");
        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            showToast("Không thể tải dữ liệu", err.message || "Có lỗi xảy ra khi tải danh sách môn học.", "danger");
            return;
        }
        subjectsData = await response.json();
        currentPage = 1;
        renderSubjects();
    } catch (err) {
        console.error("Lỗi:", err);
        showToast("Lỗi kết nối", "Không thể tải danh sách môn học.", "danger");
    }
}

// ─── RENDER LƯỚI CARD ──────────────────────────────────────
function renderSubjects() {
    const gridContainer = document.getElementById("subjectsGridContainer");
    const countSpan = document.getElementById("subjectsCount");
    const pagination = document.getElementById("subjectsPagination");
    if (!gridContainer) return;

    const filtered = subjectsData.filter(s =>
        s.subjectCode.toLowerCase().includes(searchQuery) ||
        s.subjectName.toLowerCase().includes(searchQuery) ||
        (s.description && s.description.toLowerCase().includes(searchQuery))
    );

    if (countSpan) countSpan.textContent = filtered.length;

    if (filtered.length === 0) {
        gridContainer.innerHTML = `
            <div class="col-12 text-center py-5 text-muted">
                <i class="bi bi-book fs-1 d-block mb-3 text-secondary opacity-50"></i>
                <p class="mb-0 fw-semibold">${searchQuery !== "" ? "Không tìm thấy môn học nào khớp với từ khóa tìm kiếm." : "Trung tâm chưa có môn học nào. Hãy thêm môn học đầu tiên!"}</p>
            </div>`;
        if (pagination) pagination.innerHTML = "";
        return;
    }

    const startIndex = (currentPage - 1) * pageSize;
    const pageItems = filtered.slice(startIndex, startIndex + pageSize);

    // Mảng màu nền ngẫu nhiên cho các card
    const colorPalettes = [
        { bg: "rgba(219, 234, 254, 0.6)", accent: "#1d4ed8", icon: "bi-code-slash" },
        { bg: "rgba(220, 252, 231, 0.6)", accent: "#15803d", icon: "bi-translate" },
        { bg: "rgba(254, 249, 195, 0.6)", accent: "#a16207", icon: "bi-calculator" },
        { bg: "rgba(252, 231, 243, 0.6)", accent: "#9d174d", icon: "bi-palette" },
        { bg: "rgba(237, 233, 254, 0.6)", accent: "#6d28d9", icon: "bi-music-note-beamed" },
        { bg: "rgba(255, 237, 213, 0.6)", accent: "#c2410c", icon: "bi-globe" },
    ];

    gridContainer.innerHTML = pageItems.map((subject, idx) => {
        const absIdx = startIndex + idx;
        const palette = colorPalettes[absIdx % colorPalettes.length];
        const createdDate = new Date(subject.createdAt).toLocaleDateString("vi-VN");

        return `
        <div class="col-md-6 col-lg-4">
            <div class="subject-card shadow-sm h-100 p-3">
                <!-- Header card với màu nền -->
                <div class="rounded-3 p-3 mb-3 d-flex align-items-center gap-3" style="background-color: ${palette.bg};">
                    <div class="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0"
                         style="width: 44px; height: 44px; background-color: ${palette.accent}20;">
                        <i class="bi ${palette.icon} fs-5" style="color: ${palette.accent};"></i>
                    </div>
                    <div class="overflow-hidden">
                        <span class="badge subject-code-badge rounded-pill px-2 py-1 mb-1"
                              style="background-color: ${palette.accent}20; color: ${palette.accent};">
                            ${escapeHtml(subject.subjectCode)}
                        </span>
                        <h6 class="fw-bold mb-0 text-truncate" title="${escapeHtml(subject.subjectName)}">${escapeHtml(subject.subjectName)}</h6>
                    </div>
                </div>

                <!-- Mô tả -->
                <p class="text-muted small mb-3 px-1" style="min-height: 40px;">
                    ${subject.description ? escapeHtml(subject.description) : '<em class="text-secondary">Chưa có mô tả.</em>'}
                </p>

                <!-- Thống kê -->
                <div class="d-flex flex-wrap gap-3 px-1 mb-3">
                    <div class="stat-item">
                        <i class="bi bi-calendar-week" style="color: ${palette.accent};"></i>
                        <span><strong>${subject.numberOfSessions}</strong> buổi học</span>
                    </div>
                    <div class="stat-item">
                        <i class="bi bi-folder2-open" style="color: ${palette.accent};"></i>
                        <span><strong>${subject.materialCount}</strong> tài liệu</span>
                    </div>
                    <div class="stat-item">
                        <i class="bi bi-clock-history text-muted"></i>
                        <span>${createdDate}</span>
                    </div>
                </div>

                <!-- Nút hành động -->
                <div class="card-footer-buttons d-flex gap-2 justify-content-end">
                    <button class="btn btn-outline-warning btn-action-circle shadow-sm"
                            title="Chỉnh sửa môn học"
                            onclick="openEditModal(${subject.id})">
                        <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-action-circle shadow-sm"
                            title="Xóa môn học"
                            onclick="openDeleteModal(${subject.id}, '${escapeHtml(subject.subjectName)}')">
                        <i class="bi bi-trash-fill"></i>
                    </button>
                </div>
            </div>
        </div>`;
    }).join("");

    // Render phân trang
    renderPagination(filtered.length, pagination);
}

// ─── PHÂN TRANG ─────────────────────────────────────────────
function renderPagination(totalItems, container) {
    if (!container) return;
    const totalPages = Math.ceil(totalItems / pageSize);
    if (totalPages <= 1) { container.innerHTML = ""; return; }

    let html = `<nav><ul class="pagination pagination-sm mb-0">`;
    html += `<li class="page-item ${currentPage === 1 ? "disabled" : ""}">
        <a class="page-link rounded-start-pill" href="#" onclick="goToPage(${currentPage - 1}); return false;">&#8249;</a></li>`;
    for (let p = 1; p <= totalPages; p++) {
        html += `<li class="page-item ${p === currentPage ? "active" : ""}">
            <a class="page-link" href="#" onclick="goToPage(${p}); return false;">${p}</a></li>`;
    }
    html += `<li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
        <a class="page-link rounded-end-pill" href="#" onclick="goToPage(${currentPage + 1}); return false;">&#8250;</a></li>`;
    html += `</ul></nav>`;
    container.innerHTML = html;
}

function goToPage(page) {
    const totalPages = Math.ceil(subjectsData.filter(s =>
        s.subjectCode.toLowerCase().includes(searchQuery) ||
        s.subjectName.toLowerCase().includes(searchQuery)
    ).length / pageSize);
    if (page < 1 || page > totalPages) return;
    currentPage = page;
    renderSubjects();
    window.scrollTo({ top: 0, behavior: "smooth" });
}

// ─── MỞ MODAL SỬA ───────────────────────────────────────────
function openEditModal(subjectId) {
    const subject = subjectsData.find(s => s.id === subjectId);
    if (!subject) return;

    document.getElementById("editSubjectId").value = subject.id;
    document.getElementById("editSubjectCode").value = subject.subjectCode;
    document.getElementById("editSubjectName").value = subject.subjectName;
    document.getElementById("editNumberOfSessions").value = subject.numberOfSessions;
    document.getElementById("editDescription").value = subject.description || "";

    new bootstrap.Modal(document.getElementById("editSubjectModal")).show();
}

// ─── MỞ MODAL XÓA ───────────────────────────────────────────
function openDeleteModal(subjectId, subjectName) {
    document.getElementById("deleteSubjectId").value = subjectId;
    document.getElementById("deleteSubjectName").textContent = `${subjectName}`;
    new bootstrap.Modal(document.getElementById("deleteSubjectModal")).show();
}

// ─── HELPER: ESCAPE HTML ────────────────────────────────────
function escapeHtml(str) {
    if (!str) return "";
    return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
              .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}

// ─── HELPER: TOAST THÔNG BÁO ────────────────────────────────
function showToast(title, message, type = "success") {
    const existingContainer = document.getElementById("toast-container-subject");
    let container = existingContainer;
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container-subject";
        container.style.cssText = "position:fixed;top:20px;right:20px;z-index:9999;display:flex;flex-direction:column;gap:10px;";
        document.body.appendChild(container);
    }

    const icons = { success: "bi-check-circle-fill", danger: "bi-exclamation-circle-fill", warning: "bi-exclamation-triangle-fill", info: "bi-info-circle-fill" };
    const colors = { success: "#198754", danger: "#dc3545", warning: "#ffc107", info: "#0dcaf0" };

    const toastEl = document.createElement("div");
    toastEl.style.cssText = `min-width:300px;background:#fff;border-radius:12px;padding:14px 16px;box-shadow:0 4px 20px rgba(0,0,0,0.13);border-left:4px solid ${colors[type] || colors.success};animation:slideIn 0.3s ease;`;
    toastEl.innerHTML = `
        <div style="display:flex;align-items:flex-start;gap:10px;">
            <i class="bi ${icons[type] || icons.success}" style="color:${colors[type]};font-size:1.1rem;margin-top:2px;flex-shrink:0;"></i>
            <div>
                <div style="font-weight:700;color:#111;font-size:0.9rem;">${title}</div>
                <div style="color:#555;font-size:0.82rem;margin-top:2px;">${message}</div>
            </div>
        </div>`;

    const style = document.createElement("style");
    style.textContent = `@keyframes slideIn{from{opacity:0;transform:translateX(30px);}to{opacity:1;transform:translateX(0);}}`;
    if (!document.getElementById("toast-style-subject")) { style.id = "toast-style-subject"; document.head.appendChild(style); }

    container.appendChild(toastEl);
    setTimeout(() => {
        toastEl.style.animation = "slideIn 0.3s ease reverse";
        setTimeout(() => toastEl.remove(), 300);
    }, 4000);
}
