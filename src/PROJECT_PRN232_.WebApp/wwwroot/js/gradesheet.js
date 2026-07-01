// GradeSheet Client-side handler for EduBridge Center
// Chỉ xử lý Điểm số + Nhận xét GV (không động đến điểm danh)
// Gửi PUT lên /api/lessons/{lessonId}/rollcall với attendance=null

document.addEventListener('DOMContentLoaded', () => {
    const metadataElement = document.getElementById('rollcall-metadata');
    if (!metadataElement) return;

    const lessonId = parseInt(metadataElement.getAttribute('data-lesson-id'));
    if (!lessonId || lessonId <= 0) return;

    const btnSave = document.getElementById('btn-save-rollcall');
    const btnText = document.getElementById('btn-save-text');
    const btnSpinner = document.getElementById('btn-save-spinner');
    const alertArea = document.getElementById('save-alert-area');

    if (!btnSave) return;

    btnSave.addEventListener('click', async () => {
        const rows = document.querySelectorAll('#rollcall-tbody .rollcall-row');

        if (rows.length === 0) {
            showToastAlert('warning', '<i class="bi bi-exclamation-triangle-fill me-2"></i>Bảng danh sách trống.', alertArea);
            return;
        }

        // Validate điểm số
        let hasError = false;
        rows.forEach(row => {
            const scoreInput = row.querySelector('.score-input');
            const val = scoreInput.value.trim();
            if (val !== '') {
                const num = parseFloat(val);
                if (isNaN(num) || num < 0 || num > 10) {
                    scoreInput.classList.add('is-invalid');
                    hasError = true;
                } else {
                    scoreInput.classList.remove('is-invalid');
                    scoreInput.classList.add('is-valid');
                }
            } else {
                scoreInput.classList.remove('is-invalid', 'is-valid');
            }
        });

        if (hasError) {
            showToastAlert('danger', '<i class="bi bi-x-circle-fill me-2"></i><strong>Lỗi!</strong> Điểm số phải từ 0 đến 10.', alertArea);
            return;
        }

        // Đóng gói payload: chỉ score + teacherComment, giữ nguyên attendance hiện tại (dùng status=Present làm placeholder, server sẽ không ghi đè nếu đã có)
        const payload = {
            rows: [],
            isGradeOnly: true
        };

        rows.forEach(row => {
            const studentId = parseInt(row.getAttribute('data-student-id'));
            const scoreInput = row.querySelector('.score-input');
            const teacherCommentInput = row.querySelector('.teacher-comment-input');

            const scoreRaw = scoreInput.value.trim();
            const scoreValue = scoreRaw !== '' ? parseFloat(scoreRaw) : null;

            payload.rows.push({
                studentId: studentId,
                status: 0, // Present = 0 (không thay đổi điểm danh thực tế, chỉ cập nhật score + comment)
                note: null,
                score: scoreValue,
                teacherComment: teacherCommentInput.value.trim() !== '' ? teacherCommentInput.value.trim() : null
            });
        });

        setBtnLoading(true, btnSave, btnText, btnSpinner);

        try {
            const response = await fetch(`/api/lessons/${lessonId}/rollcall`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (response.ok || response.status === 204 || response.status === 200) {
                // Xóa trạng thái validation
                rows.forEach(row => {
                    row.querySelector('.score-input')?.classList.remove('is-valid', 'is-invalid');
                });

                showToastAlert(
                    'success',
                    `<i class="bi bi-check-circle-fill me-2"></i>
                    <strong>Lưu kết quả học tập thành công!</strong> Điểm số & nhận xét của <strong>${payload.rows.length} học sinh</strong> đã cập nhật.
                    <br><span class="small mt-1 d-block opacity-75">
                        <i class="bi bi-bell-fill me-1"></i>
                        Thông báo kết quả học tập đã được đẩy <strong>tức thì</strong> đến Phụ huynh qua SignalR Realtime.
                    </span>`,
                    alertArea
                );
                alertArea.scrollIntoView({ behavior: 'smooth', block: 'center' });
            } else {
                let errorMsg = `HTTP ${response.status}`;
                try {
                    const errData = await response.json();
                    errorMsg = errData.message || errData.title || errorMsg;
                } catch { /* ignore */ }

                showToastAlert(
                    'danger',
                    `<i class="bi bi-x-circle-fill me-2"></i>
                    <strong>Lưu thất bại!</strong> Server phản hồi: <em>${escapeHtml(errorMsg)}</em>`,
                    alertArea
                );
            }
        } catch (networkError) {
            showToastAlert(
                'danger',
                `<i class="bi bi-wifi-off me-2"></i><strong>Lỗi kết nối!</strong> Không thể kết nối đến máy chủ.`,
                alertArea
            );
            console.error('Network error during gradesheet save:', networkError);
        } finally {
            setBtnLoading(false, btnSave, btnText, btnSpinner);
        }
    });

    // ===== HELPER FUNCTIONS =====

    function showToastAlert(type, htmlContent, container) {
        if (!container) return;
        container.innerHTML = '';
        container.style.display = 'block';

        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible d-flex align-items-start gap-2 shadow-sm rounded-3 border-0 px-4 py-3" role="alert">
                <div class="flex-grow-1">${htmlContent}</div>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', alertHtml);

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

    function setBtnLoading(isLoading, btn, text, spinner) {
        if (isLoading) {
            btn.disabled = true;
            text.textContent = 'Đang lưu kết quả...';
            spinner.style.display = 'inline-block';
        } else {
            btn.disabled = false;
            text.textContent = 'Lưu kết quả học tập';
            spinner.style.display = 'none';
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
});
