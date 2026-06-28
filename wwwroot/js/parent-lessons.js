// Dữ liệu mock về con của phụ huynh
const childrenData = [
    { id: 101, name: "Nguyễn Văn A (Lớp C# - PRN232)" },
    { id: 102, name: "Nguyễn Thị B (Lớp Web - PRN211)" }
];

// Dữ liệu lịch học theo từng con
const lessonsByChild = {
    101: [
        { id: 1, title: "Buổi 1: Giới thiệu khóa học & Cài đặt môi trường", date: "2026-06-01T18:00:00", description: "Làm quen với cú pháp C#, cài đặt Visual Studio.", materials: [
            { title: "Slide bài giảng OOP C#", type: "Document", url: "https://docs.google.com" },
            { title: "Video Setup môi trường cài đặt", type: "Video", url: "https://youtube.com" }
        ]},
        { id: 2, title: "Buổi 2: Biến, Kiểu dữ liệu và Các cấu trúc điều khiển", date: "2026-06-04T18:00:00", description: "Học về kiểu dữ liệu số, câu lệnh if-else.", materials: [
            { title: "Bài tập về nhà buổi 2", type: "Homework", url: "https://docs.google.com" }
        ]}
    ],
    102: [
        { id: 10, title: "Buổi 1: Tổng quan về ASP.NET Core & Razor Pages", date: "2026-06-02T19:30:00", description: "Cấu trúc thư mục của dự án Web ASP.NET Core.", materials: [
            { title: "Slide bài giảng tổng quan", type: "Document", url: "https://docs.google.com" }
        ]}
    ]
};

document.addEventListener("DOMContentLoaded", () => {
    const select = document.getElementById("selectChild");
    if (!select) return;

    // Đổ danh sách con vào Dropdown
    select.innerHTML = '<option value="">-- Chọn con --</option>' + 
        childrenData.map(c => `<option value="${c.id}">${c.name}</option>`).join("");

    select.addEventListener("change", () => {
        const childId = select.value;
        if (!childId) {
            resetView();
            return;
        }
        renderLessons(childId);
    });
});

function resetView() {
    document.getElementById("lessonsList").innerHTML = `
        <div class="text-center py-5 text-muted" id="noLessonsMessage">
            <i class="bi bi-calendar2-x fs-1 d-block mb-2 text-secondary"></i>
            Vui lòng chọn con ở góc phải để xem lịch học.
        </div>`;
    document.getElementById("lessonDetailsArea").innerHTML = `
        <div class="text-center py-5 text-muted" id="noSelectLessonMessage">
            <i class="bi bi-info-circle fs-2 d-block mb-2 text-secondary"></i>
            Chọn một buổi học ở danh sách bên trái để xem tài liệu chi tiết.
        </div>`;
}

function renderLessons(childId) {
    const container = document.getElementById("lessonsList");
    const list = lessonsByChild[childId] || [];

    if (list.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5 text-muted">
                <i class="bi bi-calendar-x fs-2 d-block mb-2"></i>
                Không tìm thấy lịch học của con.
            </div>`;
        return;
    }

    container.innerHTML = list.map((lesson, idx) => {
        const date = new Date(lesson.date).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });

        return `
            <div class="list-group-item lesson-item p-4 border-bottom" onclick="selectLesson(${childId}, ${lesson.id}, this)">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <span class="badge bg-light text-primary border rounded-pill">Buổi ${idx + 1}</span>
                    <span class="text-muted small">${date}</span>
                </div>
                <h6 class="fw-bold text-dark mb-1">${lesson.title}</h6>
                <p class="text-muted small mb-0">${lesson.description}</p>
            </div>
        `;
    }).join("");
}

function selectLesson(childId, lessonId, element) {
    // Active class item
    document.querySelectorAll(".lesson-item").forEach(item => item.classList.remove("active"));
    element.classList.add("active");

    const lessons = lessonsByChild[childId] || [];
    const lesson = lessons.find(l => l.id === lessonId);
    const detailArea = document.getElementById("lessonDetailsArea");

    if (!lesson) return;

    if (!lesson.materials || lesson.materials.length === 0) {
        detailArea.innerHTML = `
            <h6 class="fw-bold text-dark mb-3">${lesson.title}</h6>
            <div class="alert alert-light text-center py-4 border rounded-3">
                <i class="bi bi-info-circle fs-3 text-secondary d-block mb-2"></i>
                Buổi học này chưa được tải lên học liệu nào.
            </div>`;
        return;
    }

    const materialsHtml = lesson.materials.map(m => {
        let typeIcon = "📄";
        let typeName = "Tài liệu";
        let badgeColor = "bg-primary";

        if (m.type === "Video") {
            typeIcon = "🎥";
            typeName = "Video bài giảng";
            badgeColor = "bg-danger";
        } else if (m.type === "Homework") {
            typeIcon = "📝";
            typeName = "Bài tập";
            badgeColor = "bg-warning text-dark";
        }

        return `
            <div class="card border rounded-3 mb-2 p-3">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <span class="badge ${badgeColor} rounded-pill px-2.5 py-0.5 small mb-1">${typeName}</span>
                        <div class="fw-bold text-dark small">${typeIcon} ${m.title}</div>
                    </div>
                    <a href="${m.url}" target="_blank" class="btn btn-outline-success btn-sm rounded-pill">
                        <i class="bi bi-download me-1"></i> Tải / Xem
                    </a>
                </div>
            </div>
        `;
    }).join("");

    detailArea.innerHTML = `
        <h6 class="fw-bold text-dark mb-3">${lesson.title}</h6>
        <p class="text-muted small mb-4">Danh sách tài liệu học ôn tập tại nhà:</p>
        ${materialsHtml}
    `;
}
