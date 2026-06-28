// Real-time Chat Client for EduBridge System (Center & Parent) — Phase 6 with File Upload
document.addEventListener('DOMContentLoaded', () => {
    // 1. Parse global metadata từ DOM
    const globalMetadata = document.getElementById('global-chat-metadata');
    if (!globalMetadata) return;

    const currentUserId = parseInt(globalMetadata.getAttribute('data-current-user-id'));
    const currentRole = globalMetadata.getAttribute('data-current-role');
    if (isNaN(currentUserId) || !currentRole) return;

    // Phát hiện xem có đang ở trang Chat không (dựa vào sự tồn tại của #chat-page-metadata)
    const pageMetadata = document.getElementById('chat-page-metadata');
    const isChatPage = pageMetadata !== null;

    let activeChannelId = null;
    let otherUserName = '';
    let pendingFile = null; // File đang chờ gửi

    // 2. Khởi tạo kết nối SignalR đến ChatHub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/chat")
        .withAutomaticReconnect()
        .build();

    // 3. Khởi động kết nối
    connection.start()
        .then(() => {
            console.log("Connected to ChatHub via SignalR.");

            if (isChatPage) {
                if (currentRole === 'Parent') {
                    // Parent: join kênh chat cố định của mình
                    const parentChannelId = parseInt(pageMetadata.getAttribute('data-parent-channel-id'));
                    activeChannelId = parentChannelId;
                    otherUserName = pageMetadata.getAttribute('data-parent-center-name');

                    connection.invoke("JoinChannel", activeChannelId)
                        .then(() => {
                            console.log(`Joined channel #${activeChannelId}`);
                            loadMessages(activeChannelId);
                        })
                        .catch(err => console.error("Error joining channel:", err));
                } else if (currentRole === 'Center') {
                    // Center: tự động click contact nếu có target-parent-id
                    const targetParentId = pageMetadata.getAttribute('data-target-parent-id');
                    if (targetParentId) {
                        const contactItem = document.querySelector(`.contact-item[data-parent-id="${targetParentId}"]`);
                        if (contactItem) contactItem.click();
                    }
                }
            }
        })
        .catch(err => console.error("SignalR Connection Error:", err.toString()));

    // 4. Lắng nghe tin nhắn đến (cùng channel đang mở)
    connection.on("ReceiveMessage", (message) => {
        if (message.channelId === activeChannelId) {
            appendMessage(message);
            scrollToBottom();
        }
    });

    // 5. Lắng nghe thông báo chat toàn cục (khi đang ở trang khác hoặc channel khác)
    connection.on("ReceiveChatNotification", (message) => {
        console.log("New chat notification received:", message);

        // Nếu đang ở trang Chat của Center → highlight contact item trong sidebar
        if (isChatPage && currentRole === 'Center') {
            const contactItem = document.querySelector(`.contact-item[data-parent-id="${message.senderId}"]`);
            if (contactItem && activeChannelId !== message.channelId) {
                contactItem.classList.add('list-group-item-warning');
                let badge = contactItem.querySelector('.unread-chat-badge');
                if (!badge) {
                    const flexWrapper = contactItem.querySelector('.flex-grow-1');
                    badge = document.createElement('span');
                    badge.className = 'badge bg-danger rounded-pill float-end unread-chat-badge';
                    badge.innerText = 'New';
                    flexWrapper.appendChild(badge);
                }
            }
        }

        // Nếu đang ở trang khác hoặc channel khác → tăng badge navbar + hiện Toast
        if (!isChatPage || activeChannelId !== message.channelId) {
            const badgeId = currentRole === 'Center' ? 'center-chat-badge' : 'chat-badge';
            const badge = document.getElementById(badgeId);
            if (badge) {
                let count = parseInt(badge.innerText) || 0;
                badge.innerText = count + 1;
                badge.classList.remove('d-none');
            }

            const toastMsg = currentRole === 'Parent'
                ? "Bạn có tin nhắn mới từ Trung tâm!"
                : `Phụ huynh ${message.senderName} vừa gửi tin nhắn mới!`;
            showChatToast("Tin nhắn mới", toastMsg);
        }
    });

    // =====================================================================
    // 6. TÍNH NĂNG ĐÍNH KÈM FILE
    // =====================================================================
    const attachBtn    = document.getElementById('attach-file-btn');
    const fileInput    = document.getElementById('chat-file-input');
    const previewBar   = document.getElementById('file-preview-bar');
    const previewName  = document.getElementById('file-preview-name');
    const previewSize  = document.getElementById('file-preview-size');
    const cancelBtn    = document.getElementById('file-preview-cancel');

    // Mở hộp thoại chọn file khi bấm nút Kẹp giấy
    if (attachBtn && fileInput) {
        attachBtn.addEventListener('click', () => {
            fileInput.value = ''; // Reset để cho phép chọn lại file cũ
            fileInput.click();
        });
    }

    // Khi người dùng chọn file
    if (fileInput) {
        fileInput.addEventListener('change', () => {
            const file = fileInput.files[0];
            if (!file) return;

            const ext = file.name.split('.').pop().toLowerCase();

            // Client-side validation dung lượng
            const limitMap = {
                jpg: 5, jpeg: 5, png: 5, gif: 5, webp: 5, bmp: 5,   // Ảnh 5MB
                mp4: 20, mov: 20, avi: 20, mkv: 20, webm: 20,         // Video 20MB
                pdf: 10, doc: 10, docx: 10, xls: 10, xlsx: 10         // Tài liệu 10MB
            };

            const limitMb = limitMap[ext];
            if (!limitMb) {
                showFileSizeError(`Định dạng '.${ext}' không được hỗ trợ.`);
                fileInput.value = '';
                return;
            }

            const fileMb = file.size / (1024 * 1024);
            if (fileMb > limitMb) {
                showFileSizeError(`File vượt quá giới hạn ${limitMb}MB. Dung lượng thực tế: ${fileMb.toFixed(1)}MB.`);
                fileInput.value = '';
                return;
            }

            // File hợp lệ → lưu vào biến pending và hiển thị preview bar
            pendingFile = file;
            if (previewBar && previewName && previewSize) {
                previewName.textContent = file.name;
                previewSize.textContent = `${fileMb.toFixed(1)} MB`;
                previewBar.classList.remove('d-none');
            }
        });
    }

    // Hủy chọn file đã chọn
    if (cancelBtn) {
        cancelBtn.addEventListener('click', () => {
            pendingFile = null;
            if (fileInput) fileInput.value = '';
            if (previewBar) previewBar.classList.add('d-none');
        });
    }

    // =====================================================================
    // 7. CENTER — Sidebar Contact List & Channel Switching
    // =====================================================================
    if (currentRole === 'Center' && isChatPage) {
        const contactItems = document.querySelectorAll('.contact-item');
        contactItems.forEach(item => {
            item.addEventListener('click', async (e) => {
                e.preventDefault();

                contactItems.forEach(c => c.classList.remove('active', 'list-group-item-warning'));
                item.classList.add('active');

                const badge = item.querySelector('.unread-chat-badge');
                if (badge) badge.remove();

                const parentId = parseInt(item.getAttribute('data-parent-id'));
                otherUserName = item.getAttribute('data-parent-name');

                try {
                    const response = await fetch('?handler=GetOrCreateChannel', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ parentId: parentId })
                    });

                    if (response.ok) {
                        const channel = await response.json();
                        activeChannelId = channel.id;

                        if (connection.state === signalR.HubConnectionState.Connected) {
                            await connection.invoke("JoinChannel", activeChannelId);
                        }

                        document.getElementById('chat-placeholder').style.setProperty('display', 'none', 'important');
                        const activeWindow = document.getElementById('chat-active-window');
                        activeWindow.style.display = 'flex';

                        document.getElementById('active-chat-name').innerText = otherUserName;
                        document.getElementById('active-chat-avatar').innerText = otherUserName.substring(0, 1).toUpperCase();
                        document.getElementById('active-channel-info').innerText = `Kênh #${activeChannelId}`;

                        await loadMessages(activeChannelId);
                        updateNavbarChatCount();
                    }
                } catch (err) {
                    console.error("Error setting up chat channel:", err);
                }
            });
        });

        const searchInput = document.getElementById('search-contacts');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                const term = e.target.value.toLowerCase().trim();
                document.querySelectorAll('.contact-item').forEach(item => {
                    const name = item.getAttribute('data-parent-name').toLowerCase();
                    item.style.display = name.includes(term) ? 'block' : 'none';
                });
            });
        }
    }

    // =====================================================================
    // 8. GỬI TIN NHẮN (Văn bản hoặc File)
    // =====================================================================
    if (isChatPage) {
        const chatForm  = document.getElementById('chat-form');
        const chatInput = document.getElementById('chat-input');
        const sendBtn   = document.getElementById('chat-send-btn');

        if (chatForm && chatInput) {
            chatForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (!activeChannelId) return;

                // --- Gửi file nếu đang có pendingFile ---
                if (pendingFile) {
                    await sendFileMessage(pendingFile, chatInput, sendBtn);
                    return;
                }

                // --- Gửi tin nhắn văn bản thông thường ---
                const messageText = chatInput.value.trim();
                if (!messageText) return;

                try {
                    setBtnLoading(true, sendBtn);
                    await connection.invoke("SendMessage", activeChannelId, messageText, 0, null);
                    chatInput.value = '';
                    chatInput.focus();
                } catch (err) {
                    console.error("Error sending text message via SignalR:", err);
                } finally {
                    setBtnLoading(false, sendBtn);
                }
            });
        }
    }

    // Gửi file lên server rồi phát qua SignalR
    async function sendFileMessage(file, chatInput, sendBtn) {
        if (!activeChannelId) return;

        setBtnLoading(true, sendBtn, 'Đang tải lên...');

        // Hiển thị spinner loading trong khung chat
        showUploadingSpinner();

        try {
            const formData = new FormData();
            formData.append('file', file);

            const uploadResponse = await fetch('/api/chat/upload', {
                method: 'POST',
                body: formData
            });

            if (!uploadResponse.ok) {
                const errData = await uploadResponse.json().catch(() => ({}));
                throw new Error(errData.message || `Upload thất bại (HTTP ${uploadResponse.status})`);
            }

            const uploadResult = await uploadResponse.json();
            const { fileUrl, messageType, originalName } = uploadResult;

            // Phát qua SignalR với URL đã upload và loại file
            await connection.invoke("SendMessage", activeChannelId, originalName || file.name, messageType, fileUrl);

            // Xóa file đang chọn
            pendingFile = null;
            if (fileInput) fileInput.value = '';
            if (previewBar) previewBar.classList.add('d-none');
            if (chatInput) chatInput.value = '';
            chatInput?.focus();

        } catch (err) {
            console.error("Error uploading file:", err);
            showFileSizeError(`Tải file thất bại: ${err.message}`);
        } finally {
            removeUploadingSpinner();
            setBtnLoading(false, sendBtn);
        }
    }

    // =====================================================================
    // 9. HELPERS
    // =====================================================================

    // Fetch và hiển thị lịch sử tin nhắn trong channel
    async function loadMessages(channelId) {
        const container = document.getElementById('chat-messages-container');
        if (!container) return;

        container.innerHTML = `
            <div class="d-flex h-100 align-items-center justify-content-center">
                <div class="spinner-border text-primary" role="status"></div>
            </div>
        `;

        try {
            const response = await fetch(`/api/chat/channels/${channelId}/messages`);
            if (!response.ok) throw new Error("Could not load channel messages.");

            const messages = await response.json();
            container.innerHTML = '';

            if (messages.length === 0) {
                container.innerHTML = `
                    <div class="text-center text-muted py-5 small" id="no-messages-msg">
                        <i class="bi bi-info-circle me-1"></i> Chưa có tin nhắn nào. Hãy gửi tin nhắn chào hỏi nhé!
                    </div>
                `;
                return;
            }

            messages.forEach(msg => appendMessage(msg));
            scrollToBottom();

        } catch (err) {
            console.error("Error loading chat messages:", err);
            container.innerHTML = `
                <div class="alert alert-danger text-center p-3 m-3 small">
                    Không thể tải lịch sử trò chuyện.
                </div>
            `;
        }
    }

    // Render bong bóng tin nhắn theo MessageType
    // messageType: 0=Text, 1=Image, 2=Video, 3=Document
    function appendMessage(message) {
        const container = document.getElementById('chat-messages-container');
        if (!container) return;

        const noMsg = document.getElementById('no-messages-msg');
        if (noMsg) noMsg.remove();

        const isSent = message.senderId === currentUserId;
        const wrapperClass = isSent ? 'sent' : 'received';

        const date = new Date(message.sentAt);
        const timeString = date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' +
                           date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });

        // Xây nội dung bong bóng theo loại tin nhắn
        let contentHtml = '';
        const msgType = message.messageType ?? 0; // 0 nếu không có

        if (msgType === 1 && message.fileUrl) {
            // Image
            contentHtml = `
                <img src="${escapeHtml(message.fileUrl)}"
                     class="img-fluid rounded chat-media"
                     style="max-width: 250px; cursor: pointer;"
                     onclick="window.open('${escapeHtml(message.fileUrl)}', '_blank')"
                     title="Bấm để xem ảnh đầy đủ" />
            `;
        } else if (msgType === 2 && message.fileUrl) {
            // Video
            contentHtml = `
                <video src="${escapeHtml(message.fileUrl)}" controls
                       class="rounded chat-media"
                       style="max-width: 300px; display: block;">
                </video>
            `;
        } else if (msgType === 3 && message.fileUrl) {
            // Document
            const fileName = escapeHtml(message.messageContent || 'Tải tài liệu');
            contentHtml = `
                <div class="d-flex align-items-center gap-2 p-2 bg-white rounded border">
                    <i class="bi bi-file-earmark-text fs-4 text-danger flex-shrink-0"></i>
                    <div class="flex-grow-1 overflow-hidden">
                        <div class="small fw-semibold text-truncate">${fileName}</div>
                        <a href="${escapeHtml(message.fileUrl)}" target="_blank"
                           class="btn btn-sm btn-light border mt-1">
                            <i class="bi bi-file-earmark-arrow-down me-1"></i>Tải file tài liệu
                        </a>
                    </div>
                </div>
            `;
        } else {
            // Text (mặc định)
            contentHtml = `<span class="message-text">${escapeHtml(message.messageContent)}</span>`;
        }

        const html = `
            <div class="message-bubble-wrapper ${wrapperClass}">
                <div class="message-bubble ${msgType !== 0 ? 'p-2' : ''}">
                    ${contentHtml}
                    <span class="message-meta">${timeString}</span>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', html);
    }

    function scrollToBottom() {
        const container = document.getElementById('chat-messages-container');
        if (container) container.scrollTop = container.scrollHeight;
    }

    function setBtnLoading(isLoading, btn, loadingText = 'Đang gửi...') {
        if (!btn) return;
        if (isLoading) {
            btn.disabled = true;
            btn.dataset.originalHtml = btn.innerHTML;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1" role="status"></span>${loadingText}`;
        } else {
            btn.disabled = false;
            if (btn.dataset.originalHtml) {
                btn.innerHTML = btn.dataset.originalHtml;
            }
        }
    }

    // Hiển thị spinner "đang tải file lên" trong khung chat
    function showUploadingSpinner() {
        const container = document.getElementById('chat-messages-container');
        if (!container) return;
        const el = document.createElement('div');
        el.id = 'uploading-spinner';
        el.className = 'text-center py-2 text-muted small';
        el.innerHTML = `<div class="spinner-border spinner-border-sm text-primary me-1" role="status"></div> Đang tải file lên...`;
        container.appendChild(el);
        container.scrollTop = container.scrollHeight;
    }

    function removeUploadingSpinner() {
        const el = document.getElementById('uploading-spinner');
        if (el) el.remove();
    }

    function showFileSizeError(msg) {
        showChatToast('Lỗi File', msg);
    }

    // Cập nhật lại badge navbar dựa vào số channel chưa đọc
    async function updateNavbarChatCount() {
        try {
            const response = await fetch('/api/chat/channels');
            if (!response.ok) return;
            const channels = await response.json();
            let count = 0;
            channels.forEach(ch => {
                if (!ch.isLastMessageRead && ch.lastMessageSenderId !== currentUserId) count++;
            });
            const badgeId = currentRole === 'Center' ? 'center-chat-badge' : 'chat-badge';
            const badge = document.getElementById(badgeId);
            if (badge) {
                badge.innerText = count;
                count === 0 ? badge.classList.add('d-none') : badge.classList.remove('d-none');
            }
        } catch (err) {
            console.error("Error updating chat badge count:", err);
        }
    }

    // Toast popup Bootstrap 5 ở góc phải dưới
    function showChatToast(title, message) {
        let container = document.getElementById('eb-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'eb-toast-container';
            container.className = 'position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '1080';
            document.body.appendChild(container);
        }

        const toastId = 'toast-chat-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast hide shadow-lg bg-white" role="alert" aria-live="assertive" aria-atomic="true"
                 style="border-radius: 8px; border-left: 4px solid #198754; min-width: 300px;">
                <div class="toast-header bg-white text-dark border-0 py-2">
                    <i class="bi bi-chat-left-text-fill text-success me-2"></i>
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
        const toastElement = document.getElementById(toastId);
        if (toastElement) {
            const bsToast = new bootstrap.Toast(toastElement, { autohide: true, delay: 4000 });
            bsToast.show();
            toastElement.addEventListener('hidden.bs.toast', () => toastElement.remove());
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
