// RollCall Client-side handler for EduBridge Center
// Chỉ xử lý Điểm danh (status + ghi chú)
// Điểm số & Nhận xét GV được quản lý riêng ở trang GradeSheet

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

        // Đóng gói payload: chỉ attendance status + note
        const payload = {
            rows: [],
            isAttendanceOnly: true
        };

        rows.forEach(row => {
            const studentId = parseInt(row.getAttribute('data-student-id'));
            const attendanceSelect = row.querySelector('.attendance-select');
            const attendanceNoteInput = row.querySelector('.attendance-note-input');

            payload.rows.push({
                studentId: studentId,
                status: mapStatusToEnum(attendanceSelect.value),
                note: attendanceNoteInput.value.trim() !== '' ? attendanceNoteInput.value.trim() : null,
                score: null,
                teacherComment: null
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
                showToastAlert(
                    'success',
                    `<i class="bi bi-check-circle-fill me-2"></i>
                    <strong>Lưu điểm danh thành công!</strong> Trạng thái điểm danh của <strong>${payload.rows.length} học sinh</strong> đã được cập nhật.`,
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

    // Ánh xạ trạng thái chuỗi sang Enum Number của C# Backend
    function mapStatusToEnum(statusStr) {
        switch (statusStr) {
            case "Present": return 0;
            case "Absent": return 1;
            case "Late": return 2;
            case "Excused": return 3;
            default: return 0;
        }
    }

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
            text.textContent = 'Đang lưu điểm danh...';
            spinner.style.display = 'inline-block';
        } else {
            btn.disabled = false;
            text.textContent = 'Lưu điểm danh';
            spinner.style.display = 'none';
        }
    }

    // Không cần removeValidationStyles nữa vì đã bỏ cột điểm số

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
