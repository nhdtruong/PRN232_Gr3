let childrenData = [];

document.addEventListener("DOMContentLoaded", async () => {
    // Parse query parameters
    const urlParams = new URLSearchParams(window.location.search);
    const paramChildId = urlParams.get('ChildId');
    const paramLessonId = urlParams.get('LessonId');

    await loadChildrenFromServer();

    const select = document.getElementById("selectChild");
    if (select) {
        select.addEventListener("change", () => {
            const childId = select.value;
            if (!childId) {
                resetView();
                return;
            }
            renderLessons(childId);
        });

        // Tự động chọn con và tải lịch học khi được điều hướng từ thông báo
        if (paramChildId) {
            select.value = paramChildId;
            await renderLessons(paramChildId, paramLessonId);
        }
    }
});

// Tải danh sách con của phụ huynh
async function loadChildrenFromServer() {
    const select = document.getElementById("selectChild");
    if (!select) return;

    try {
        // Gọi API thật để lấy danh sách con từ DB
        const response = await fetch("/api/parent/my-children"); 
        if (response.ok) {
            const data = await response.json();
            childrenData = data;
        } else {
            console.error("Lỗi khi tải danh sách con từ server");
        }
    } catch (err) {
        console.error("Lỗi kết nối API lấy danh sách con:", err);
    }

    select.innerHTML = '<option value="">-- Chọn con --</option>' + 
        childrenData.map(c => `<option value="${c.id}">${c.fullName || c.name}</option>`).join("");
}

function resetView() {
    const badge = document.getElementById("lessonBadgeCount");
    if (badge) badge.style.display = "none";

    document.getElementById("lessonsList").innerHTML = `
        <div class="text-center py-5 text-muted my-4" id="noLessonsMessage">
            <i class="bi bi-calendar2-x fs-1 d-block mb-3 text-secondary opacity-50"></i>
            <p class="mb-0 fw-semibold">Vui lòng chọn con ở góc phải để xem lịch học chi tiết.</p>
        </div>`;
    document.getElementById("lessonDetailsArea").innerHTML = `
        <div class="text-center py-5 text-muted my-5" id="noSelectLessonMessage">
            <i class="bi bi-info-circle fs-2 d-block mb-3 text-secondary opacity-50"></i>
            <p class="mb-0 fw-semibold">Chọn một buổi học ở danh sách bên trái để xem tài liệu chi tiết.</p>
        </div>`;
}

// Tải lịch học thật của con từ database
async function renderLessons(childId, targetLessonId = null) {
    const container = document.getElementById("lessonsList");
    const badge = document.getElementById("lessonBadgeCount");
    if (container) {
        container.innerHTML = `
            <div class="text-center py-5">
                <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span>
                Đang tải lịch học của con từ máy chủ...
            </div>`;
    }

    try {
        // Gọi API thật của Người 4: GET /api/parent/children/{studentId}/lessons
        const response = await fetch(`/api/parent/children/${childId}/lessons`);
        if (response.ok) {
            const list = await response.json();
            
            if (badge) {
                badge.textContent = `${list.length} buổi`;
                badge.style.display = "inline-block";
            }
            
            if (list.length === 0) {
                container.innerHTML = `
                    <div class="text-center py-5 text-muted my-4">
                        <i class="bi bi-calendar-x fs-2 d-block mb-2 text-secondary opacity-50"></i>
                        Học sinh chưa tham gia lớp học nào hoặc chưa được xếp lịch.
                    </div>`;
                return;
            }

            container.innerHTML = list.map((lesson, idx) => {
                const date = new Date(lesson.lessonDate).toLocaleDateString("vi-VN", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                    hour: "2-digit",
                    minute: "2-digit"
                });

                return `
                    <div class="lesson-card p-3 d-flex flex-column gap-2" onclick="selectLesson(${childId}, ${lesson.id}, this)">
                        <div class="d-flex justify-content-between align-items-center">
                            <span class="badge bg-primary bg-opacity-10 text-primary rounded-pill px-2.5 py-1 small fw-semibold" style="font-size: 0.72rem;">
                                ${lesson.className || 'Lớp học'}
                            </span>
                            <span class="text-muted small" style="font-size: 0.75rem;"><i class="bi bi-clock me-1"></i>${date}</span>
                        </div>
                        <h6 class="fw-bold text-dark mb-0 text-truncate" style="font-size: 0.92rem;">${lesson.title}</h6>
                        <p class="text-muted small mb-0 text-truncate-2" style="font-size: 0.8rem; line-height: 1.4;">${lesson.description || 'Không có mô tả nội dung.'}</p>
                    </div>
                `;
            }).join("");

            // Tự động click chọn buổi học và cuộn tới nếu có targetLessonId
            if (targetLessonId) {
                setTimeout(() => {
                    const card = [...container.querySelectorAll(".lesson-card")].find(c => 
                        c.getAttribute("onclick") && c.getAttribute("onclick").includes(`, ${targetLessonId},`)
                    );
                    if (card) {
                        card.click();
                        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                }, 100);
            }
        } else {
            container.innerHTML = `<div class="text-center py-5 text-danger">Không có quyền xem thông tin học sinh này.</div>`;
        }
    } catch (err) {
        console.error("Lỗi:", err);
        container.innerHTML = `<div class="text-center py-5 text-danger">Lỗi kết nối máy chủ.</div>`;
    }
}

function selectLesson(childId, lessonId, element) {
    // Active class item
    document.querySelectorAll(".lesson-card").forEach(item => item.classList.remove("active"));
    element.classList.add("active");

    const detailArea = document.getElementById("lessonDetailsArea");
    if (!detailArea) return;

    detailArea.innerHTML = `
        <div class="text-center py-4">
            <span class="spinner-border spinner-border-sm text-success me-2" role="status"></span>
            Đang tải tài liệu học tập ôn tập...
        </div>`;

    fetch(`/api/center/lessons/${lessonId}/materials`)
        .then(response => {
            if (response.ok) return response.json();
            throw new Error("Lỗi tải");
        })
        .then(materials => {
            if (materials.length === 0) {
                detailArea.innerHTML = `
                    <div class="alert alert-light text-center py-4 border rounded-3">
                        <i class="bi bi-info-circle fs-3 text-secondary d-block mb-2"></i>
                        Giáo viên chưa đăng tải học liệu/bài tập cho buổi học này.
                    </div>`;
                return;
            }

            const materialsHtml = materials.map(m => {
                let typeIcon = "📄";
                let typeName = "Tài liệu";
                let badgeColor = "bg-primary";
                let embedHtml = ""; // HTML nhúng video (nếu có)

                if (m.materialType === "Video") {
                    typeIcon = "🎥";
                    typeName = "Video bài giảng";
                    badgeColor = "bg-danger";

                    // Trích xuất ID Youtube để tạo link nhúng embed
                    const youtubeId = getYoutubeId(m.fileURL);
                    if (youtubeId) {
                        embedHtml = `
                            <div class="ratio ratio-16x9 mb-3 rounded-3 overflow-hidden shadow-sm">
                                <iframe src="https://www.youtube.com/embed/${youtubeId}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
                            </div>
                        `;
                    }
                } else if (m.materialType === "Homework") {
                    typeIcon = "📝";
                    typeName = "Bài tập";
                    badgeColor = "bg-warning text-dark";
                }

                // Cấu hình tải về trực tiếp đối với file hệ thống
                const isInternalFile = m.fileURL.startsWith("/uploads/");
                const downloadAttr = isInternalFile ? `download="${m.title}"` : "";

                return `
                    <div class="card border rounded-4 mb-3 p-3 shadow-sm">
                        ${embedHtml}
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <span class="badge ${badgeColor} rounded-pill px-2.5 py-0.5 small mb-1">${typeName}</span>
                                <div class="fw-bold text-dark small">${typeIcon} ${m.title}</div>
                            </div>
                            <a href="${m.fileURL}" target="_blank" ${downloadAttr} class="btn btn-outline-success btn-sm rounded-pill">
                                <i class="bi bi-box-arrow-up-right me-1"></i> Xem / Tải về
                            </a>
                        </div>
                    </div>
                `;
            }).join("");

            detailArea.innerHTML = `
                <p class="text-muted small mb-3">Tài liệu ôn tập chính thức:</p>
                ${materialsHtml}
            `;
        })
        .catch(err => {
            console.error("Lỗi:", err);
            detailArea.innerHTML = `<div class="alert alert-danger">Không thể tải tài liệu. Vui lòng thử lại.</div>`;
        });
}

// Hàm trích xuất ID của video Youtube từ các định dạng URL khác nhau
function getYoutubeId(url) {
    if (!url) return null;
    const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
    const match = url.match(regExp);
    return (match && match[2].length === 11) ? match[2] : null;
}
