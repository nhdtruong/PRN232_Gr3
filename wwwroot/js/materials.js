// Lấy LessonId từ metadata ẩn của trang HTML
const metadataEl = document.getElementById("material-metadata");
const lessonId = metadataEl ? parseInt(metadataEl.getAttribute("data-lesson-id")) : 0;

let materialsData = [];

document.addEventListener("DOMContentLoaded", () => {
    if (lessonId > 0) {
        loadMaterialsFromServer();
    }

    // Xử lý bộ lọc filter
    const filterBtns = document.querySelectorAll(".filter-btn");
    filterBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            filterBtns.forEach(b => b.classList.remove("active"));
            btn.classList.add("active");
            const filterType = btn.getAttribute("data-filter");
            renderMaterials(filterType);
        });
    });

    // Xử lý upload tài liệu mới
    const form = document.getElementById("uploadMaterialForm");
    if (form) {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            const titleInput = document.getElementById("materialTitle");
            const typeSelect = document.getElementById("materialType");
            const urlInput = document.getElementById("materialUrl");

            const payload = {
                title: titleInput.value,
                materialType: typeSelect.value,
                fileURL: urlInput.value
            };

            try {
                const response = await fetch(`/api/center/lessons/${lessonId}/materials`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    showToast("Thành công", "Đã tải lên tài liệu mới thành công và lưu vào database!", "success");
                    form.reset();
                    await loadMaterialsFromServer(); // Reload lại danh sách thật
                } else {
                    const error = await response.json();
                    showToast("Thất bại", error.message || "Không thể upload tài liệu.", "danger");
                }
            } catch (err) {
                console.error("Lỗi upload:", err);
                showToast("Lỗi", "Không thể kết nối đến máy chủ.", "danger");
            }
        });
    }
});

// Tải danh sách học liệu từ database
async function loadMaterialsFromServer() {
    const container = document.getElementById("materialsContainer");
    if (container) {
        container.innerHTML = `
            <div class="col-12 text-center py-5">
                <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span>
                Đang tải danh sách học liệu từ máy chủ...
            </div>`;
    }

    try {
        const response = await fetch(`/api/center/lessons/${lessonId}/materials`);
        if (response.ok) {
            materialsData = await response.json();
            
            // Cập nhật lại bộ lọc đang active
            const activeBtn = document.querySelector(".filter-btn.active");
            const filterType = activeBtn ? activeBtn.getAttribute("data-filter") : "all";
            renderMaterials(filterType);
        } else {
            showToast("Lỗi", "Không thể tải danh sách tài liệu từ máy chủ.", "danger");
        }
    } catch (err) {
        console.error("Lỗi tải tài liệu:", err);
        showToast("Lỗi kết nối", "Không thể nạp học liệu.", "danger");
    }
}

// Render tài liệu
function renderMaterials(filterType = "all") {
    const container = document.getElementById("materialsContainer");
    const countSpan = document.getElementById("materialCount");
    if (!container) return;

    const filtered = filterType === "all" 
        ? materialsData 
        : materialsData.filter(m => m.materialType === filterType);

    if (countSpan) {
        countSpan.textContent = filtered.length;
    }

    if (filtered.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center py-5 text-muted" id="noMaterialsMessage">
                <i class="bi bi-folder-x fs-1 d-block mb-2 text-secondary"></i>
                Không có học liệu nào phù hợp với bộ lọc.
            </div>`;
        return;
    }

    container.innerHTML = filtered.map(m => {
        let icon = "📄";
        let badgeColor = "bg-secondary";
        
        if (m.materialType === "Video") {
            icon = "🎥";
            badgeColor = "bg-danger";
        } else if (m.materialType === "Homework") {
            icon = "📝";
            badgeColor = "bg-warning text-dark";
        } else if (m.materialType === "Document") {
            icon = "📄";
            badgeColor = "bg-primary";
        }

        const date = new Date(m.uploadedAt).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });

        return `
            <div class="col-md-6 material-item" data-type="${m.materialType}">
                <div class="card h-100 border rounded-4 shadow-sm material-card">
                    <div class="card-body p-3">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <span class="badge ${badgeColor} rounded-pill px-2.5 py-1 small">${m.materialType}</span>
                            <span class="text-muted small">${date}</span>
                        </div>
                        <h6 class="fw-bold text-dark mb-3 text-truncate-2" style="height: 38px;">${icon} ${m.title}</h6>
                        
                        <div class="d-flex gap-2">
                            <a href="${m.fileURL}" target="_blank" class="btn btn-outline-primary btn-sm rounded-pill flex-grow-1">
                                <i class="bi bi-box-arrow-up-right me-1"></i> Xem chi tiết
                            </a>
                            <button class="btn btn-outline-danger btn-sm rounded-circle" onclick="deleteMaterial(${m.id})" title="Xóa">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }).join("");
}

// Xóa tài liệu thật trong database
async function deleteMaterial(id) {
    if (confirm("Bạn có chắc muốn xóa tài liệu học tập này khỏi cơ sở dữ liệu không?")) {
        try {
            const response = await fetch(`/api/center/materials/${id}`, {
                method: "DELETE"
            });

            if (response.ok) {
                showToast("Thành công", "Học liệu đã được xóa hoàn toàn khỏi database.", "success");
                await loadMaterialsFromServer();
            } else {
                const error = await response.json();
                showToast("Thất bại", error.message || "Không thể xóa học liệu.", "danger");
            }
        } catch (err) {
            console.error("Lỗi khi xóa học liệu:", err);
            showToast("Lỗi", "Không thể kết nối máy chủ để xóa.", "danger");
        }
    }
}

// Toast helper
function showToast(title, content, type = "success") {
    let alertClass = "bg-success";
    if (type === "danger") alertClass = "bg-danger";
    
    const container = document.createElement("div");
    container.className = "position-fixed bottom-0 end-0 p-3";
    container.style.zIndex = "1100";
    container.innerHTML = `
        <div class="toast show text-white ${alertClass} border-0 rounded-3 shadow" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}:</strong> ${content}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    document.body.appendChild(container);
    setTimeout(() => container.remove(), 3000);
}
