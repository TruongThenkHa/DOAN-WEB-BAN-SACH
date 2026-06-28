(function () {
    "use strict";

    const STORAGE_KEY = "bookstore_chat_history";
    const MAX_HISTORY = 20; // keep last N messages in memory only (per tab session)

    let chatHistory = [];
    let isWaitingForReply = false;

    const suggestions = [
        "Sách bán chạy nhất?",
        "Phương thức thanh toán?",
        "Có sách kỹ năng sống không?",
        "Cách đặt hàng?"
    ];

    function init() {
        renderWidgetShell();
        bindEvents();
        renderWelcomeMessage();
    }

    function renderWidgetShell() {
        const root = document.createElement("div");
        root.id = "chatbot-root";
        root.innerHTML = `
            <button class="chatbot-toggle-btn" id="chatbotToggleBtn" aria-label="Mở trợ lý AI">
                <i class="fas fa-comment-dots"></i>
            </button>
            <div class="chatbot-window" id="chatbotWindow">
                <div class="chatbot-header">
                    <div class="chatbot-header-info">
                        <div class="chatbot-avatar"><i class="fas fa-robot"></i></div>
                        <div>
                            <p class="chatbot-header-title">Trợ lý sách AI</p>
                            <div class="chatbot-header-status"><span class="dot"></span> Đang hoạt động</div>
                        </div>
                    </div>
                    <button class="chatbot-close-btn" id="chatbotCloseBtn" aria-label="Đóng">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="chatbot-messages" id="chatbotMessages"></div>
                <div class="chatbot-suggestions" id="chatbotSuggestions"></div>
                <div class="chatbot-input-area">
                    <input type="text" class="chatbot-input" id="chatbotInput"
                           placeholder="Nhập câu hỏi..." autocomplete="off" maxlength="500" />
                    <button class="chatbot-send-btn" id="chatbotSendBtn" aria-label="Gửi">
                        <i class="fas fa-paper-plane"></i>
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(root);
        renderSuggestions();
    }

    function renderSuggestions() {
        const wrap = document.getElementById("chatbotSuggestions");
        wrap.innerHTML = suggestions
            .map(s => `<button class="chatbot-suggestion-chip" type="button">${escapeHtml(s)}</button>`)
            .join("");

        wrap.querySelectorAll(".chatbot-suggestion-chip").forEach(chip => {
            chip.addEventListener("click", () => {
                sendMessage(chip.textContent);
            });
        });
    }

    function renderWelcomeMessage() {
        appendMessage(
            "bot",
            "Xin chào! Mình là trợ lý AI của Nhà Sách Duy An Mình có thể giúp bạn tìm sách, kiểm tra giá, hoặc trả lời câu hỏi về thanh toán. Bạn cần giúp gì?"
        );
    }

    function bindEvents() {
        document.getElementById("chatbotToggleBtn").addEventListener("click", toggleWindow);
        document.getElementById("chatbotCloseBtn").addEventListener("click", toggleWindow);
        document.getElementById("chatbotSendBtn").addEventListener("click", handleSendClick);
        document.getElementById("chatbotInput").addEventListener("keydown", e => {
            if (e.key === "Enter" && !isWaitingForReply) {
                handleSendClick();
            }
        });
    }

    function toggleWindow() {
        const win = document.getElementById("chatbotWindow");
        win.classList.toggle("open");
        if (win.classList.contains("open")) {
            document.getElementById("chatbotInput").focus();
        }
    }

    function handleSendClick() {
        const input = document.getElementById("chatbotInput");
        const text = input.value.trim();
        if (!text || isWaitingForReply) return;
        input.value = "";
        sendMessage(text);
    }

    async function sendMessage(text) {
        if (!text || isWaitingForReply) return;

        appendMessage("user", text);
        showTypingIndicator();
        isWaitingForReply = true;
        setSendingState(true);

        try {
            const response = await fetch("/api/chat/send", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: text })
            });

            const data = await response.json();
            hideTypingIndicator();

            appendMessage("bot", data.reply || "Xin lỗi, mình chưa hiểu câu hỏi này.");
        } catch (err) {
            hideTypingIndicator();
            appendMessage("bot", "Không thể kết nối tới trợ lý AI. Vui lòng kiểm tra lại sau.");
            console.error("Chatbot error:", err);
        } finally {
            isWaitingForReply = false;
            setSendingState(false);
        }
    }

    function setSendingState(disabled) {
        document.getElementById("chatbotSendBtn").disabled = disabled;
    }

    function appendMessage(role, text) {
        const container = document.getElementById("chatbotMessages");
        const msg = document.createElement("div");
        msg.className = `chatbot-msg ${role}`;
        msg.textContent = text;
        container.appendChild(msg);
        container.scrollTop = container.scrollHeight;

        chatHistory.push({ role, text });
        if (chatHistory.length > MAX_HISTORY) {
            chatHistory.shift();
        }
    }

    function showTypingIndicator() {
        const container = document.getElementById("chatbotMessages");
        const typing = document.createElement("div");
        typing.className = "chatbot-msg typing";
        typing.id = "chatbotTypingIndicator";
        typing.innerHTML = `<div class="typing-dots"><span></span><span></span><span></span></div>`;
        container.appendChild(typing);
        container.scrollTop = container.scrollHeight;
    }

    function hideTypingIndicator() {
        const el = document.getElementById("chatbotTypingIndicator");
        if (el) el.remove();
    }

    function escapeHtml(str) {
        const div = document.createElement("div");
        div.textContent = str;
        return div.innerHTML;
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
