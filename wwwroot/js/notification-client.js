// SignalR Realtime Client Configuration for EduBridge System
document.addEventListener('DOMContentLoaded', () => {
    // ── GUARD CLAUSE (Global Layout Safety) ──────────────────────────────────
    // File này được nhúng toàn cục trong _Layout.cshtml cho mọi user đã login.
    // Chỉ Parent mới có #notificationDropdown và NotificationHub subscription.
    // Center KHÔNG có các phần tử này → thoát sớm để tránh lỗi + tốn tài nguyên.
    const globalMeta = document.getElementById('global-chat-metadata');
    if (!globalMeta) return; // User chưa đăng nhập
    if (globalMeta.getAttribute('data-current-role') !== 'Parent') return; // Không phải Parent
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Establish connection to SignalR NotificationHub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // 2. Start the connection
    connection.start()
        .then(() => {
            console.log("Connected to EduBridge NotificationHub via SignalR.");
        })
        .catch(err => {
            console.error("Error connecting to SignalR Hub:", err.toString());
        });

    // 3. Register Hub Event Handlers
    
    // When the Hub sends the unread count on initial connection or update
    connection.on("UpdateUnreadCount", (count) => {
        updateBadgeCount(count);
    });

    // When a new notification is pushed in real time
    connection.on("ReceiveNotification", (notification, newUnreadCount) => {
        // Update badge count
        updateBadgeCount(newUnreadCount);

        // Display interactive Bootstrap 5 Toast pop-up on screen
        showToast(notification.title, notification.message);

        // If the dropdown is currently open, refresh the list in real-time
        const dropdownMenu = document.getElementById('notification-list-container');
        if (dropdownMenu && dropdownMenu.classList.contains('show')) {
            loadNotifications();
        }
    });

    // 4. Dropdown Events (Load notifications when clicked)
    const dropdownToggle = document.getElementById('notificationDropdown');
    if (dropdownToggle) {
        dropdownToggle.addEventListener('show.bs.dropdown', () => {
            loadNotifications();
        });
    }

    // 5. Action: Mark All as Read
    const markAllReadBtn = document.getElementById('mark-all-read-btn');
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation(); // Keep dropdown open

            try {
                const response = await fetch('/api/notifications/mark-all-read', {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    // Update count to 0 locally
                    updateBadgeCount(0);
                    // Refresh notifications list to reflect the updated status
                    await loadNotifications();
                } else {
                    console.error("Failed to mark all as read: server returned non-200 code");
                }
            } catch (err) {
                console.error("Error making mark-all-read request:", err);
            }
        });
    }
});

// Helper: Update navbar badge count and mark-all-read button visibility
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
        if (count === 0) {
            markAllBtn.style.setProperty('display', 'none', 'important');
        } else {
            markAllBtn.style.display = 'inline-block';
        }
    }
}

// Helper: Fetch and render latest notifications list inside dropdown
async function loadNotifications() {
    const listContainer = document.getElementById('notification-items-list');
    if (!listContainer) return;

    try {
        const response = await fetch('/api/notifications?limit=10');
        if (!response.ok) throw new Error('API server returned error status');

        const notifications = await response.json();

        if (notifications.length === 0) {
            listContainer.innerHTML = `
                <div class="p-3 text-center text-muted small" id="no-notifications-placeholder">
                    <i class="bi bi-info-circle fs-5 d-block mb-1 text-secondary"></i>
                    Không có thông báo nào.
                </div>
            `;
            return;
        }

        // Generate and insert HTML items
        listContainer.innerHTML = notifications.map(n => {
            const date = new Date(n.createdAt);
            const timeString = date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' +
                               date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
            
            const unreadClass = n.isRead ? '' : 'notification-unread';
            
            return `
                <div class="list-group-item list-group-item-action notification-item ${unreadClass} p-3 border-bottom border-0">
                    <div class="d-flex w-100 justify-content-between align-items-center mb-1">
                        <h6 class="mb-1 text-dark fw-bold small" style="font-size: 0.85rem;">${escapeHtml(n.title)}</h6>
                        <small class="text-muted" style="font-size: 0.7rem;">${timeString}</small>
                    </div>
                    <p class="mb-0 text-secondary" style="font-size: 0.8rem; line-height: 1.3;">${escapeHtml(n.message)}</p>
                </div>
            `;
        }).join('');

    } catch (err) {
        console.error("Error loading notifications in dropdown:", err);
        listContainer.innerHTML = `
            <div class="p-3 text-center text-danger small">
                <i class="bi bi-exclamation-triangle-fill fs-5 d-block mb-1 text-danger"></i>
                Lỗi tải thông báo. Vui lòng thử lại.
            </div>
        `;
    }
}

// Helper: Show animated dynamic Toast alert in bottom-right corner
function showToast(title, message) {
    // 1. Check or construct global toast container
    let container = document.getElementById('eb-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'eb-toast-container';
        container.className = 'position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    // 2. Build unique toast markup
    const toastId = 'toast-' + Date.now() + Math.floor(Math.random() * 1000);
    const toastHtml = `
        <div id="${toastId}" class="toast hide shadow-lg bg-white" role="alert" aria-live="assertive" aria-atomic="true" style="border-radius: 8px; border-left: 4px solid #0d6efd; min-width: 300px;">
            <div class="toast-header bg-white text-dark border-0 py-2">
                <i class="bi bi-bell-fill text-primary me-2"></i>
                <strong class="me-auto fw-bold" style="font-size: 0.85rem;">${escapeHtml(title)}</strong>
                <small class="text-muted" style="font-size: 0.7rem;">Vừa xong</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body border-top text-secondary py-2" style="font-size: 0.8rem;">
                ${escapeHtml(message)}
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', toastHtml);

    // 3. Trigger Bootstrap 5 Toast API
    const toastElement = document.getElementById(toastId);
    if (toastElement) {
        const bsToast = new bootstrap.Toast(toastElement, {
            autohide: true,
            delay: 4000
        });
        bsToast.show();

        // Remove element from DOM completely after it's hidden to keep DOM clean
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
}

// Helper: Secure HTML escape to prevent XSS
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, "&amp;")
              .replace(/</g, "&lt;")
              .replace(/>/g, "&gt;")
              .replace(/"/g, "&quot;")
              .replace(/'/g, "&#039;");
}
