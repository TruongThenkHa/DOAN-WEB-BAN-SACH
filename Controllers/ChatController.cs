using Book_Store.Services.Chat;
using Microsoft.AspNetCore.Mvc;

namespace Book_Store.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatBotService chatBotService, ILogger<ChatController> logger)
        {
            _chatBotService = chatBotService;
            _logger = logger;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return Ok(new { reply = "Vui lòng nhập câu hỏi." });

            // Basic length guard — prevents abuse / huge prompts
            var message = request.Message.Length > 500 ? request.Message[..500] : request.Message;

            try
            {
                var reply = await _chatBotService.GetReplyAsync(message);
                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat request failed for message: {Message}", message);
                return Ok(
                    new
                    {
                        reply = "Xin lỗi, đã có lỗi xảy ra khi xử lý câu hỏi của bạn. Vui lòng thử lại.",
                    }
                );
            }
        }
    }
}
