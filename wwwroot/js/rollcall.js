// RollCall Client-side handler for EduBridge Center (Phase 4)
// Khi bấm nút "Lưu bảng điểm & Điểm danh", script này sẽ:
//   1. Thu thập dữ liệu từ tất cả các dòng trong bảng.
//   2. Đóng gói theo chuẩn DTO LessonRollCallBulkUpsertDto.
//   3. Gửi PUT lên /api/lessons/{lessonId}/rollcall.
//   4. Hiển thị Toast Bootstrap 5 thành công/thất bại.

document.addEventListener('DOMContentLoaded', () => {
    // 1. Lấy LessonId từ metadata DOM
    const metadataElement = document.getElementById('rollcall-metadata');
    if (!metadataElement) {
        console.error("Không tìm thấy rollcall-metadata element.");
        return;
    }
    const lessonId = parseInt(metadataElement.getAttribute('data-lesson-id'));
    if (!lessonId || lessonId <= 0) {
        console.error("LessonId không hợp lệ:", lessonId);
        return;
    }

    const btnSave = document.getElementById('btn-save-rollcall');
    const btnText = document.getElementById('btn-save-text');
    const btnSpinner = document.getElementById('btn-save-spinner');
    const alertArea = document.getElementById('save-alert-area');

    if (!btnSave) return;

    // 2. Lắng nghe sự kiện click nút Lưu
    btnSave.addEventListener('click', async () => {
        // Thu thập dữ liệu từ bảng
        const rows = document.querySelectorAll('#rollcall-tbody .rollcall-row');

        if (rows.length === 0) {
            showToastAlert('warning', '<i class="bi bi-exclamation-triangle-fill me-2"></i>Bảng danh sách trống. Không có dữ liệu để lưu.', alertArea);
            return;
        }

        // Kiểm tra hợp lệ điểm số trước khi gửi
        let hasValidationError = false;
        rows.forEach((row, index) => {
            const scoreInput = row.querySelector('.score-input');
            const scoreValue = scoreInput.value.trim();
            if (scoreValue !== '') {
                const scoreNum = parseFloat(scoreValue);
                if (isNaN(scoreNum) || scoreNum < 0 || scoreNum > 10) {
                    scoreInput.classList.add('is-invalid');
                    hasValidationError = true;
                } else {
                    scoreInput.classList.remove('is-invalid');
                    scoreInput.classList.add('is-valid');
                }
            } else {
                scoreInput.classList.remove('is-invalid', 'is-valid');
            }
        });

        if (hasValidationError) {
            showToastAlert('danger', '<i class="bi bi-x-circle-fill me-2"></i><strong>Lỗi dữ liệu!</strong> Điểm số phải nằm trong khoảng 0–10. Vui lòng kiểm tra lại các ô đánh dấu đỏ.', alertArea);
            return;
        }

        // Đóng gói payload theo cấu trúc LessonRollCallBulkUpsertDto
        const payload = {
            rows: []
        };

        rows.forEach(row => {
            const studentId = parseInt(row.getAttribute('data-student-id'));
            const attendanceSelect = row.querySelector('.attendance-select');
            const scoreInput = row.querySelector('.score-input');
            const teacherCommentInput = row.querySelector('.teacher-comment-input');
            const attendanceNoteInput = row.querySelector('.attendance-note-input');

            const scoreRaw = scoreInput.value.trim();
            const scoreValue = scoreRaw !== '' ? parseFloat(scoreRaw) : null;

            payload.rows.push({
                studentId: studentId,
                status: attendanceSelect.value,  // "Present" | "Absent" | "Late" | "Excused"
                note: attendanceNoteInput.value.trim() !== '' ? attendanceNoteInput.value.trim() : null,
                score: scoreValue,
                teacherComment: teacherCommentInput.value.trim() !== '' ? teacherCommentInput.value.trim() : null
            });
        });

        // Đặt trạng thái Loading cho nút
        setBtnLoading(true, btnSave, btnText, btnSpinner);

        // 3. Gửi request AJAX PUT tới API
        try {
            const response = await fetch(`/api/lessons/${lessonId}/rollcall`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (response.ok || response.status === 204 || response.status === 200) {
                // 4a. Thành công: Hiển thị Toast thành công + thông báo
                removeValidationStyles(rows);
                showToastAlert(
                    'success',
                    `<i class="bi bi-check-circle-fill me-2"></i>
                    <strong>Lưu thành công!</strong> Điểm danh & điểm số của <strong>${payload.rows.length} học sinh</strong> đã được cập nhật vào hệ thống.
                    <br><span class="small mt-1 d-block opacity-75">
                        <i class="bi bi-bell-fill me-1"></i>
                        Thông báo kết quả học tập đã được đẩy <strong>tức thì</strong> đến Phụ huynh qua SignalR Realtime.
                    </span>`,
                    alertArea
                );

                // Cuộn mịn lên đầu trang để thấy thông báo
                alertArea.scrollIntoView({ behavior: 'smooth', block: 'center' });

            } else {
                // 4b. Thất bại với lỗi từ server
                let errorMsg = `HTTP ${response.status}`;
                try {
                    const errData = await response.json();
                    errorMsg = errData.message || errData.title || errorMsg;
                } catch {
                    // Không parse được JSON, dùng status code
                }

                showToastAlert(
                    'danger',
                    `<i class="bi bi-x-circle-fill me-2"></i>
                    <strong>Lưu thất bại!</strong> Server phản hồi: <em>${escapeHtml(errorMsg)}</em>
                    <br><span class="small mt-1 d-block opacity-75">Vui lòng kiểm tra quyền truy cập hoặc dữ liệu đã nhập.</span>`,
                    alertArea
                );
            }

        } catch (networkError) {
            // 4c. Lỗi kết nối mạng
            showToastAlert(
                'danger',
                `<i class="bi bi-wifi-off me-2"></i>
                <strong>Lỗi kết nối!</strong> Không thể kết nối đến máy chủ.
                <br><span class="small mt-1 d-block opacity-75">Vui lòng kiểm tra kết nối mạng và thử lại.</span>`,
                alertArea
            );
            console.error("Network error during rollcall save:", networkError);
        } finally {
            // Khôi phục trạng thái nút dù thành công hay thất bại
            setBtnLoading(false, btnSave, btnText, btnSpinner);
        }
    });

    // ===== HELPER FUNCTIONS =====

    // Hiển thị thông báo Alert nội tuyến (không reload trang)
    function showToastAlert(type, htmlContent, container) {
        if (!container) return;

        // Xóa alert cũ nếu có
        container.innerHTML = '';
        container.style.display = 'block';

        const iconMap = {
            success: 'bg-success',
            danger: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info'
        };

        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible d-flex align-items-start gap-2 shadow-sm rounded-3 border-0 px-4 py-3 animate__animated animate__fadeInDown" role="alert">
                <div class="flex-grow-1">
                    ${htmlContent}
                </div>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', alertHtml);

        // Tự động ẩn alert thành công sau 8 giây
        if (type === 'success') {
            setTimeout(() => {
                const alertEl = container.querySelector('.alert');
                if (alertEl) {
                    alertEl.classList.add('fade');
                    setTimeout(() => {
                        container.innerHTML = '';
                        container.style.display = 'none';
                    }, 300);
                }
            }, 8000);
        }
    }

    // Bật/tắt trạng thái Loading của nút lưu
    function setBtnLoading(isLoading, btn, text, spinner) {
        if (isLoading) {
            btn.disabled = true;
            text.textContent = 'Đang lưu & đồng bộ...';
            spinner.style.display = 'inline-block';
        } else {
            btn.disabled = false;
            text.textContent = 'Lưu bảng điểm & Điểm danh';
            spinner.style.display = 'none';
        }
    }

    // Xóa toàn bộ class is-valid / is-invalid sau khi lưu thành công
    function removeValidationStyles(rows) {
        rows.forEach(row => {
            const scoreInput = row.querySelector('.score-input');
            if (scoreInput) {
                scoreInput.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    // Escape HTML để ngăn XSS khi hiển thị error message từ server
    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, "&amp;")
                  .replace(/</g, "&lt;")
                  .replace(/>/g, "&gt;")
                  .replace(/"/g, "&quot;")
                  .replace(/'/g, "&#039;");
    }
});
