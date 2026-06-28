// Dữ liệu mẫu (Mock data) cho tài liệu học tập
let materialsData = [
    { id: 1, title: "Slide bài giảng OOP & 4 tính chất cốt lõi", type: "Document", url: "https://docs.google.com/presentation/d/1", uploadedAt: "2026-06-11T19:30:00" },
    { id: 2, title: "Video giải thích tính Đa hình và Kế thừa", type: "Video", url: "https://www.youtube.com/watch?v=1", uploadedAt: "2026-06-12T08:15:00" },
    { id: 3, title: "Bài tập về nhà: Thiết kế Class quản lý thư viện", type: "Homework", url: "https://docs.google.com/document/d/1", uploadedAt: "2026-06-12T10:00:00" }
];

document.addEventListener("DOMContentLoaded", () => {
    renderMaterials();

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
        form.addEventListener("submit", (e) => {
            e.preventDefault();
            const titleInput = document.getElementById("materialTitle");
            const typeSelect = document.getElementById("materialType");
            const urlInput = document.getElementById("materialUrl");

            const newMaterial = {
                id: materialsData.length + 1,
                title: titleInput.value,
                type: typeSelect.value,
                url: urlInput.value,
                uploadedAt: new Date().toISOString()
            };

            materialsData.push(newMaterial);
            renderMaterials();
            form.reset();
            showToast("Thành công", "Đã tải lên tài liệu mới thành công!", "success");
        });
    }
});

// Render tài liệu
function renderMaterials(filterType = "all") {
    const container = document.getElementById("materialsContainer");
    const countSpan = document.getElementById("materialCount");
    if (!container) return;

    const filtered = filterType === "all" 
        ? materialsData 
        : materialsData.filter(m => m.type === filterType);

    countSpan.textContent = filtered.length;

    if (filtered.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center py-5 text-muted" id="noMaterialsMessage">
                <i class="bi bi-folder-x fs-1 d-block mb-2 text-secondary"></i>
                Không tìm thấy học liệu nào phù hợp với bộ lọc.
            </div>`;
        return;
    }

    container.innerHTML = filtered.map(m => {
        let icon = "📄";
        let badgeColor = "bg-secondary";
        
        if (m.type === "Video") {
            icon = "🎥";
            badgeColor = "bg-danger";
        } else if (m.type === "Homework") {
            icon = "📝";
            badgeColor = "bg-warning text-dark";
        } else if (m.type === "Document") {
            icon = "📄";
            badgeColor = "bg-primary";
        }

        const date = new Date(m.uploadedAt).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });

        return `
            <div class="col-md-6 material-item" data-type="${m.type}">
                <div class="card h-100 border rounded-4 shadow-sm material-card">
                    <div class="card-body p-3">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <span class="badge ${badgeColor} rounded-pill px-2.5 py-1 small">${m.type}</span>
                            <span class="text-muted small">${date}</span>
                        </div>
                        <h6 class="fw-bold text-dark mb-3 text-truncate-2" style="height: 38px;">${icon} ${m.title}</h6>
                        
                        <div class="d-flex gap-2">
                            <a href="${m.url}" target="_blank" class="btn btn-outline-primary btn-sm rounded-pill flex-grow-1">
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

// Xóa tài liệu
function deleteMaterial(id) {
    if (confirm("Bạn có chắc muốn xóa tài liệu học tập này không?")) {
        materialsData = materialsData.filter(m => m.id !== id);
        renderMaterials();
        showToast("Đã xóa", "Học liệu đã được loại bỏ.", "danger");
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
