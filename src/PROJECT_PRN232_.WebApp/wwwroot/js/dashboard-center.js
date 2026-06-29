document.addEventListener("DOMContentLoaded", () => {
    const metadataEl = document.getElementById("classes-metadata");
    if (!metadataEl) return;

    // Phân tích danh sách lớp học từ JSON
    let classes = [];
    try {
        classes = JSON.parse(metadataEl.getAttribute("data-classes")) || [];
    } catch (e) {
        console.error("Lỗi parse JSON lớp học:", e);
    }

    let currentPage = 1;
    const pageSize = 3; // Hiển thị tối đa 3 lớp học trên 1 trang cho gọn gàng

    renderClassesPage();

    function renderClassesPage() {
        const container = document.getElementById("classesGridContainer");
        const pagination = document.getElementById("classesPagination");
        if (!container) return;

        if (classes.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="card border-0 shadow-sm rounded-4 bg-white py-5">
                        <div class="card-body text-center text-muted">
                            <i class="bi bi-building-add fs-1 d-block mb-3 text-secondary"></i>
                            <h5>Chưa có lớp học nào thuộc quyền quản lý của bạn</h5>
                            <p class="small text-muted mb-0">Vui lòng liên hệ bộ phận hỗ trợ hoặc tạo lớp học mới để bắt đầu giảng dạy.</p>
                        </div>
                    </div>
                </div>`;
            if (pagination) pagination.innerHTML = "";
            return;
        }

        // Cắt mảng lấy lớp học thuộc trang hiện tại
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const pageItems = classes.slice(startIndex, endIndex);

        // Render Cards
        container.innerHTML = pageItems.map(c => `
            <div class="col-lg-4 col-md-6 col-sm-12">
                <div class="card h-100 shadow-sm class-card-dashboard">
                    <div class="card-body p-4 d-flex flex-column">
                        <div class="d-flex align-items-center gap-2 mb-3">
                            <span class="badge bg-primary rounded-pill px-3 py-1.5 fw-bold" style="font-size: 0.72rem;">Class</span>
                            <span class="text-muted small">ID: #${c.Id}</span>
                        </div>
                        <h4 class="fw-bold text-dark mb-3 text-truncate">${c.ClassName}</h4>
                        <p class="text-muted small flex-grow-1 mb-4">
                            Trạng thái: <span class="badge rounded-pill px-2.5 py-1 fw-bold" style="${c.Status === 'Active' ? 'background-color: rgba(16, 185, 129, 0.15); color: #047857;' : 'background-color: rgba(239, 68, 68, 0.15); color: #b91c1c;'}">${c.Status === 'Active' ? 'Đang hoạt động' : 'Đã đóng'}</span>
                        </p>
                        
                        <div class="d-flex flex-column gap-2 mt-auto">
                            <a href="/Center/Lessons/Lesson/${c.Id}" class="btn btn-success btn-sm rounded-pill w-100 py-2.5 fw-semibold shadow-sm">
                                <i class="bi bi-calendar-event me-1"></i> Quản lý Buổi học & Học liệu
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `).join("");

        // Render thanh phân trang
        renderPagination(classes.length);
    }

    function renderPagination(totalItems) {
        const pagination = document.getElementById("classesPagination");
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

        // Nút trang số
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
                    renderClassesPage();
                }
            });
        });
    }
});
