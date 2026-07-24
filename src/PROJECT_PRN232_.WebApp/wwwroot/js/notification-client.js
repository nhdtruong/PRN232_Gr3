// ============================================================
// EduBridge — SignalR Notification Client
// Features:
//   • Real-time badge counter via SignalR
//   • Toast popup on new notification
//   • Dropdown list with click-to-read detail modal
//   • Mark single / mark-all-read
// ============================================================

document.addEventListener('DOMContentLoaded', () => {
    // ── GUARD CLAUSE ─────────────────────────────────────────────────────────
    const globalMeta = document.getElementById('global-chat-metadata');
    if (!globalMeta) return;
    const role = globalMeta.getAttribute('data-current-role');
    if (role !== 'Parent' && role !== 'Teacher' && role !== 'Center') return;
    // ─────────────────────────────────────────────────────────────────────────

    // Inject the detail modal into DOM (once, on first load)
    if (role === 'Parent' || role === 'Teacher') {
        injectNotificationModal();
    }

    // 1. SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => console.log("Connected to EduBridge NotificationHub via SignalR."))
        .catch(err => console.error("Error connecting to SignalR Hub:", err.toString()));

    // 2. Hub event handlers
    connection.on("UpdateUnreadCount", (count) => {
        if (role === 'Parent' || role === 'Teacher') {
            updateBadgeCount(count);
        }
    });

    connection.on("ReceiveNotification", (notification, newUnreadCount) => {
        if (role === 'Parent' || role === 'Teacher') {
            updateBadgeCount(newUnreadCount);
        }
        
        // Show realtime toast
        showToast(notification.title, notification.message);

        // Dispatch custom event for sub-pages to react to notifications in real-time
        window.dispatchEvent(new CustomEvent('eb-notification-received', { detail: notification }));

        // Realtime update for Center's class transfer pending requests badge
        if (role === 'Center' && notification.title === "Yêu cầu đổi lớp mới") {
            const transferBadge = document.getElementById('transfer-pending-badge');
            if (transferBadge) {
                let currentCount = parseInt(transferBadge.innerText) || 0;
                currentCount++;
                transferBadge.innerText = currentCount;
                transferBadge.style.setProperty('display', 'inline-block', 'important');
            }
        }

        const dropdownMenu = document.getElementById('notification-list-container');
        if (dropdownMenu && dropdownMenu.classList.contains('show')) {
            loadNotifications();
        }
    });

    // 3. Open dropdown → load list
    const dropdownToggle = document.getElementById('notificationDropdown');
    if (dropdownToggle) {
        dropdownToggle.addEventListener('show.bs.dropdown', () => loadNotifications());
    }

    // 4. Mark-all-read button
    const markAllReadBtn = document.getElementById('mark-all-read-btn');
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation();
            try {
                const response = await fetch('/api/notifications/mark-all-read', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' }
                });
                if (response.ok) {
                    updateBadgeCount(0);
                    await loadNotifications();
                }
            } catch (err) {
                console.error("Error marking all as read:", err);
            }
        });
    }

    // 5. Event delegation: click on notification item → open detail modal
    //    Attached once to the persistent container element.
    const notifList = document.getElementById('notification-items-list');
    if (notifList) {
        notifList.addEventListener('click', (e) => {
            const item = e.target.closest('[data-notif-id]');
            if (item) openNotificationDetail(item);
        });
    }
});

// ── Badge update ──────────────────────────────────────────────────────────────
function updateBadgeCount(count) {
    const badge = document.getElementById('notification-badge');
    const markAllBtn = document.getElementById('mark-all-read-btn');

    if (badge) {
        badge.innerText = count;
        if (count === 0) {
            badge.style.setProperty('display', 'none', 'important');
        } else {
            badge.style.display = 'inline-block';
        }
    }
    if (markAllBtn) {
        markAllBtn.style.display = count === 0 ? 'none' : 'inline-block';
    }
}

// ── Load notification list into dropdown ──────────────────────────────────────
async function loadNotifications() {
    const listContainer = document.getElementById('notification-items-list');
    if (!listContainer) return;

    try {
        const response = await fetch('/api/notifications?limit=10');
        if (!response.ok) throw new Error('API error');

        const notifications = await response.json();

        if (notifications.length === 0) {
            listContainer.innerHTML = `
                <div class="p-3 text-center text-muted small" id="no-notifications-placeholder">
                    <i class="bi bi-info-circle fs-5 d-block mb-1 text-secondary"></i>
                    Không có thông báo nào.
                </div>`;
            return;
        }

        listContainer.innerHTML = notifications.map(n => {
            const date = new Date(n.createdAt);
            const timeString =
                date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' +
                date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });

            const unreadClass = n.isRead ? '' : 'notification-unread';

            // Solid indigo dot for unread notifications
            const unreadDot = n.isRead ? '' : 
                `<span class="eb-unread-dot" style="width: 8px; height: 8px; border-radius: 50%; background-color: #4F46E5; display: inline-block; flex-shrink: 0;" title="Chưa đọc"></span>`;

            // Escape data stored in data-* attributes
            const safeTitle   = escapeHtml(n.title);
            const safeMessage = escapeHtml(n.message); // keeps HTML structure for the modal

            return `
                <div class="list-group-item list-group-item-action notification-item ${unreadClass} p-3 border-bottom border-0"
                     style="cursor: pointer; transition: background 0.15s; border-left: 3px solid transparent;"
                     data-notif-id="${n.id}"
                     data-notif-title="${safeTitle}"
                     data-notif-message="${safeMessage}"
                     data-notif-time="${timeString}"
                     data-notif-read="${n.isRead}"
                     title="Nhấn để xem chi tiết">
                    <div class="d-flex w-100 justify-content-between align-items-center">
                        <div class="d-flex align-items-center gap-2 overflow-hidden" style="max-width: 72%;">
                            ${unreadDot}
                            <h6 class="mb-0 fw-bold small text-truncate" style="font-size: 0.85rem; color: #1e1b4b;">${safeTitle}</h6>
                        </div>
                        <small class="text-muted flex-shrink-0" style="font-size: 0.7rem;">${timeString}</small>
                    </div>
                </div>`;
        }).join('');

    } catch (err) {
        console.error("Error loading notifications:", err);
        const listContainer2 = document.getElementById('notification-items-list');
        if (listContainer2) {
            listContainer2.innerHTML = `
                <div class="p-3 text-center text-danger small">
                    <i class="bi bi-exclamation-triangle-fill fs-5 d-block mb-1 text-danger"></i>
                    Lỗi tải thông báo. Vui lòng thử lại.
                </div>`;
        }
    }
}

// ── Open notification detail modal + mark as read ─────────────────────────────
async function openNotificationDetail(item) {
    const id      = item.dataset.notifId;
    const title   = item.dataset.notifTitle;
    const message = item.dataset.notifMessage;
    const time    = item.dataset.notifTime;
    const isRead  = item.dataset.notifRead === 'true';

    // Populate modal
    const modalTitle   = document.getElementById('eb-notif-modal-title');
    const modalMessage = document.getElementById('eb-notif-modal-message');
    const modalTime    = document.getElementById('eb-notif-modal-time');
    if (modalTitle)   modalTitle.textContent   = title;
    if (modalTime)    modalTime.textContent    = time;

    // Helper: Parse old plain text notification message to premium HTML on-the-fly
    let formattedMessage = message;
    if (message && !message.includes('<div') && !message.includes('<p')) {
        // 1. Material pattern
        const matMatch = message.match(/^Lớp\s+(.+?),\s+buổi\s+"(.+?)":\s+Giáo\s+viên\s+vừa\s+đăng\s+tải\s+tài\s+liệu\s+"(.+?)"/i);
        if (matMatch) {
            formattedMessage = `
<div class='mb-2'><b>Lớp học:</b> ${escapeHtml(matMatch[1])}</div>
<div class='mb-2'><b>Buổi học:</b> ${escapeHtml(matMatch[2])}</div>
<div class='mb-2'><b>Thời gian lớp học:</b> <span class='badge bg-primary'>${time}</span></div>`;
        } else {
            // 2. Lesson pattern
            const lesMatch = message.match(/^Lớp\s+"(.+?)"\s+vừa\s+thêm\s+buổi\s+học\s+mới:\s+"(.+?)"\s+vào\s+lúc\s+(.+)/i);
            if (lesMatch) {
                formattedMessage = `
<div class='mb-2'><b>Lớp học:</b> ${escapeHtml(lesMatch[1])}</div>
<div class='mb-2'><b>Buổi học:</b> ${escapeHtml(lesMatch[2])}</div>
<div class='mb-2'><b>Thời gian lớp học:</b> <span class='badge bg-primary'>${escapeHtml(lesMatch[3])}</span></div>`;
            } else {
                // 3. Enrollment pattern
                const enrollMatch = message.match(/^Con\s+bạn\s+\((.+?)\)\s+đã\s+được\s+thêm\s+vào\s+lớp\s+"(.+?)"/i);
                if (enrollMatch) {
                    formattedMessage = `
<div class='mb-2'><b>Học sinh:</b> ${escapeHtml(enrollMatch[1])}</div>
<div class='mb-2'><b>Lớp học:</b> <span class='badge bg-indigo text-white' style='background-color: #4F46E5;'>${escapeHtml(enrollMatch[2])}</span></div>`;
                } else {
                    // 4. Score pattern
                    const scoreMatch = message.match(/^Lớp\s+(.+?),\s+buổi\s+"(.+?)":\s+(.+?)\s+được\s+chấm\s+điểm\s+(.+?)\/10/i);
                    if (scoreMatch) {
                        formattedMessage = `
<div class='mb-2'><b>Lớp học:</b> ${escapeHtml(scoreMatch[1])}</div>
<div class='mb-2'><b>Buổi học:</b> ${escapeHtml(scoreMatch[2])}</div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='mb-2'><b>Học sinh:</b> ${escapeHtml(scoreMatch[3])}</div>
<div class='mb-2'><b>Điểm số:</b> <strong class='text-primary' style='font-size: 1.05rem;'>${escapeHtml(scoreMatch[4])}</strong> / 10`;
                    }
                }
            }
        }
    }

    if (modalMessage) modalMessage.innerHTML = formattedMessage; // Render structured HTML safely

    // Show modal (Bootstrap 5)
    const modalEl = document.getElementById('eb-notification-modal');
    if (modalEl) {
        const bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
        bsModal.show();
    }

    // Mark as read (if not already)
    if (!isRead) {
        try {
            const res = await fetch(`/api/notifications/${id}/mark-read`, { method: 'PUT' });
            if (res.ok) {
                const data = await res.json();
                updateBadgeCount(data.unreadCount);
                // Remove visual "unread" indicator from the item
                item.classList.remove('notification-unread');
                item.dataset.notifRead = 'true';
                const dot = item.querySelector('.eb-unread-dot');
                if (dot) dot.remove();
            }
        } catch (err) {
            console.error("Error marking notification as read:", err);
        }
    }
}

// ── Inject premium detail modal into DOM (called once) ───────────────────────
function injectNotificationModal() {
    if (document.getElementById('eb-notification-modal')) return; // Already exists

    // Inject custom CSS to make published lesson reports beautiful and 2-column layout on wide screens
    const styleHtml = `
    <style>
        @media (min-width: 576px) {
            .published-lesson-report {
                display: grid !important;
                grid-template-columns: 1fr 1fr !important;
                gap: 16px !important;
                background: transparent !important;
            }
            .published-lesson-report > div:first-child {
                grid-column: span 2 !important;
                margin-bottom: 0 !important;
            }
            .published-lesson-report > div[style*="margin-bottom"] {
                margin-bottom: 0 !important;
                display: flex;
                flex-direction: column;
            }
            .published-lesson-report > div[style*="margin-bottom"] > div:last-child,
            .published-lesson-report > div[style*="margin-bottom"] > ul {
                flex-grow: 1;
            }
            .published-lesson-report > .text-center {
                grid-column: span 2 !important;
                margin-top: 8px !important;
            }
        }
        /* Style improvement for report boxes */
        .published-lesson-report div[style*="border: 1px solid"] {
            border: 1px solid rgba(79, 70, 229, 0.1) !important;
            background: #f8fafc !important;
            border-radius: 12px !important;
            padding: 14px !important;
            transition: all 0.2s ease;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02);
        }
        .published-lesson-report div[style*="border: 1px solid"]:hover {
            border-color: rgba(79, 70, 229, 0.2) !important;
            box-shadow: 0 4px 12px rgba(79, 70, 229, 0.05);
        }
        .published-lesson-report ul {
            background: #f8fafc !important;
            border: 1px solid rgba(79, 70, 229, 0.1) !important;
            border-radius: 12px !important;
            padding: 14px 14px 14px 30px !important;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02);
        }
        .published-lesson-report hr {
            display: none !important; /* Hide ugly raw HR lines */
        }
    </style>`;
    document.head.insertAdjacentHTML('beforeend', styleHtml);

    const modalHtml = `
    <div id="eb-notification-modal" class="modal fade" tabindex="-1"
         aria-labelledby="eb-notif-modal-title" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg" style="max-width: 760px; width: 95%;">
            <div class="modal-content" style="border-radius: 20px; border: none; overflow: hidden; box-shadow: 0 24px 64px rgba(79,70,229,0.22);">

                <!-- Header gradient -->
                <div class="modal-header" style="background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); border: none; padding: 1.25rem 1.6rem;">
                    <div class="d-flex align-items-center gap-3 flex-grow-1 overflow-hidden me-2">
                        <div style="min-width:40px; height:40px; border-radius:50%; background:rgba(255,255,255,0.2); display:flex; align-items:center; justify-content:center; box-shadow: inset 0 2px 4px rgba(255,255,255,0.1);">
                            <i class="bi bi-bell-fill text-white" style="font-size:1.05rem;"></i>
                        </div>
                        <div>
                            <h6 class="modal-title text-white fw-bold mb-0"
                                id="eb-notif-modal-title"
                                style="font-size:0.95rem; line-height:1.4; white-space:normal;"></h6>
                        </div>
                    </div>
                    <button type="button" class="btn-close btn-close-white flex-shrink-0"
                            data-bs-dismiss="modal" aria-label="Đóng"></button>
                </div>

                <!-- Body -->
                <div class="modal-body px-4 py-4" style="background-color: #fafafa;">
                    <div id="eb-notif-modal-message"
                       class="mb-3"
                       style="font-size:0.9rem; line-height:1.65; color:#374151;"></div>
                    <div class="d-flex align-items-center gap-1 border-top pt-3"
                         style="font-size:0.75rem; color:#9CA3AF; border-color: rgba(0,0,0,0.05) !important;">
                        <i class="bi bi-clock me-1"></i>
                        <span id="eb-notif-modal-time"></span>
                    </div>
                </div>

                <!-- Footer -->
                <div class="modal-footer" style="border-top:1px solid rgba(0,0,0,0.05); padding:0.8rem 1.6rem; background-color: #ffffff;">
                    <button type="button"
                            class="btn btn-sm px-4 fw-semibold text-white shadow-sm"
                            data-bs-dismiss="modal"
                            style="background:linear-gradient(135deg,#4F46E5,#7C3AED); border:none; border-radius:10px; font-size:0.85rem; padding: 8px 24px; transition: all 0.2s ease;">
                        Đóng
                    </button>
                </div>

            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
}

// ── Toast popup ───────────────────────────────────────────────────────────────
function showToast(title, message) {
    let container = document.getElementById('eb-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'eb-toast-container';
        container.className = 'position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    const toastId = 'toast-' + Date.now() + Math.floor(Math.random() * 1000);
    // Sleek single-row notification banner showing only the title
    const toastHtml = `
        <div id="${toastId}" class="toast hide shadow-lg bg-white" role="alert" aria-live="assertive" aria-atomic="true"
             style="border-radius:12px; border-left:4px solid #4F46E5; min-width:320px;">
            <div class="toast-header bg-white text-dark border-0 py-3 px-3 d-flex align-items-center justify-content-between">
                <div class="d-flex align-items-center gap-2 overflow-hidden me-2">
                    <i class="bi bi-bell-fill" style="color:#4F46E5; font-size:1rem; min-width:18px;"></i>
                    <strong class="fw-bold" style="font-size:0.83rem; color:#1e1b4b; line-height:1.35; white-space:normal;">${escapeHtml(title)}</strong>
                </div>
                <div class="d-flex align-items-center gap-2 flex-shrink-0">
                    <small class="text-muted" style="font-size:0.68rem; white-space:nowrap;">Vừa xong</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close" style="font-size:0.75rem;"></button>
                </div>
            </div>
        </div>`;

    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastEl = document.getElementById(toastId);
    if (toastEl) {
        const bsToast = new bootstrap.Toast(toastEl, { autohide: true, delay: 5000 });
        bsToast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    }
}

// ── XSS prevention ────────────────────────────────────────────────────────────
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, "&amp;")
              .replace(/</g, "&lt;")
              .replace(/>/g, "&gt;")
              .replace(/"/g, "&quot;")
              .replace(/'/g, "&#039;");
}

