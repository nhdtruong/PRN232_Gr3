// Lấy LessonId từ metadata ẩn của trang HTML
const metadataEl = document.getElementById("material-metadata");
const lessonId = metadataEl ? parseInt(metadataEl.getAttribute("data-lesson-id")) : 0;

let materialsData = [];

document.addEventListener("DOMContentLoaded", () => {
    if (lessonId > 0) {
        loadMaterialsFromServer();
    }

    // Xử lý chuyển đổi qua lại giữa chọn File và dán Link URL
    const sourceFileRadio = document.getElementById("sourceFile");
    const sourceUrlRadio = document.getElementById("sourceUrl");
    const fileInputGroup = document.getElementById("fileInputGroup");
    const urlInputGroup = document.getElementById("urlInputGroup");

    if (sourceFileRadio && sourceUrlRadio && fileInputGroup && urlInputGroup) {
        sourceFileRadio.addEventListener("change", () => {
            if (sourceFileRadio.checked) {
                fileInputGroup.style.display = "block";
                urlInputGroup.style.display = "none";
                document.getElementById("materialFile").setAttribute("required", "required");
                document.getElementById("materialUrl").removeAttribute("required");
            }
        });

        sourceUrlRadio.addEventListener("change", () => {
            if (sourceUrlRadio.checked) {
                fileInputGroup.style.display = "none";
                urlInputGroup.style.display = "block";
                document.getElementById("materialUrl").setAttribute("required", "required");
                document.getElementById("materialFile").removeAttribute("required");
            }
        });

        // Thiết lập require ban đầu cho input file
        document.getElementById("materialFile").setAttribute("required", "required");
    }

    // Xử lý bộ lọc filter loại học liệu
    const filterBtns = document.querySelectorAll(".filter-btn");
    filterBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            filterBtns.forEach(b => b.classList.remove("active"));
            btn.classList.add("active");
            const filterType = btn.getAttribute("data-filter");
            renderMaterials(filterType);
        });
    });

    // Xử lý submit Form Upload tài liệu mới
    const form = document.getElementById("uploadMaterialForm");
    if (form) {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalBtnText = submitBtn.innerHTML;

            const titleInput = document.getElementById("materialTitle");
            const typeSelect = document.getElementById("materialType");
            const isUploadFile = document.getElementById("sourceFile").checked;

            let fileURL = "";

            // Hiển thị trạng thái loading khi upload
            submitBtn.disabled = true;
            submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span> Đang tải lên...`;

            try {
                if (isUploadFile) {
                    // PHƯƠNG ÁN 1: Tải file vật lý lên server
                    const fileInput = document.getElementById("materialFile");
                    if (fileInput.files.length === 0) {
                        showToast("Lỗi", "Vui lòng chọn 1 file để tải lên.", "danger");
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalBtnText;
                        return;
                    }

                    const formData = new FormData();
                    formData.append("file", fileInput.files[0]);

                    const uploadResponse = await fetch("/api/center/materials/upload-file", {
                        method: "POST",
                        body: formData
                    });

                    if (uploadResponse.ok) {
                        const uploadResult = await uploadResponse.json();
                        fileURL = uploadResult.url; // Đường dẫn tương đối từ server
                    } else {
                        const error = await uploadResponse.json();
                        showToast("Lỗi upload file", error.message || "Không thể lưu file lên server.", "danger");
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalBtnText;
                        return;
                    }
                } else {
                    // PHƯƠNG ÁN 2: Dán link URL ngoài
                    const urlInput = document.getElementById("materialUrl");
                    fileURL = urlInput.value;
                }

                // Tiến hành lưu thông tin học liệu vào database
                const payload = {
                    title: titleInput.value,
                    materialType: typeSelect.value,
                    fileURL: fileURL
                };

                const response = await fetch(`/api/center/lessons/${lessonId}/materials`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    showToast("Thành công", "Đã tải lên học liệu mới thành công!", "success");
                    form.reset();
                    
                    // Thiết lập lại trạng thái mặc định cho form
                    if (sourceFileRadio) sourceFileRadio.click();

                    await loadMaterialsFromServer();
                } else {
                    const error = await response.json();
                    showToast("Thất bại", error.message || "Không thể lưu học liệu.", "danger");
                }
            } catch (err) {
                console.error("Lỗi:", err);
                showToast("Lỗi hệ thống", "Không thể kết nối đến máy chủ.", "danger");
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalBtnText;
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
            
            const activeBtn = document.querySelector(".filter-btn.active");
            const filterType = activeBtn ? activeBtn.getAttribute("data-filter") : "all";
            renderMaterials(filterType);
        } else {
            showToast("Lỗi", "Không thể tải danh sách tài liệu từ máy chủ.", "danger");
        }
    } catch (err) {
        console.error("Lỗi:", err);
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

        // Hỗ trợ link tải xuống trực tiếp nếu file lưu trên server của mình
        const isInternalFile = m.fileURL.startsWith("/uploads/");
        const downloadAttr = isInternalFile ? `download="${m.title}"` : "";

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
                            <a href="${m.fileURL}" target="_blank" ${downloadAttr} class="btn btn-outline-primary btn-sm rounded-pill flex-grow-1">
                                <i class="bi bi-box-arrow-up-right me-1"></i> Xem / Tải về
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
            console.error("Lỗi:", err);
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
