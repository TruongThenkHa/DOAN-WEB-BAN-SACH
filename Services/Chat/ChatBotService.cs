using System.Text;
using System.Text.Json;
using Book_Store.Models;
using Microsoft.EntityFrameworkCore;

namespace Book_Store.Services.Chat
{
    public interface IChatBotService
    {
        Task<string> GetReplyAsync(string userMessage);
    }

    public class ChatBotService : IChatBotService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatBotService> _logger;

        // Ollama runs locally
        private const string OllamaUrl = "http://localhost:11434/api/generate";
        private const string ModelName = "qwen2.5:1.5b"; // change to 3b if you pulled that instead

        private const string SystemPrompt =
            "You are a helpful shopping assistant for \"Nhà Sách Duy An\", a Vietnamese online bookstore. "
            + "Answer briefly and in a friendly tone (2-4 sentences max). "
            + "Only answer questions related to the bookstore: books, categories, prices, orders, payment methods, account help. "
            + "If asked something unrelated (coding, math homework, general trivia), politely say you can only help with bookstore questions. "
            + "If book data is provided below, use ONLY that data to answer — do not invent book titles, prices, or stock numbers. "
            + "Reply in the same language the user used (Vietnamese or English).";

        public ChatBotService(
            ApplicationDbContext db,
            IHttpClientFactory httpClientFactory,
            ILogger<ChatBotService> logger
        )
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Bạn muốn hỏi gì về sách hoặc đơn hàng? 📚";

            var intent = DetectIntent(userMessage);
            string? contextData = null;

            switch (intent)
            {
                case ChatIntent.SearchByTitleOrAuthor:
                    contextData = await BuildBookSearchContextAsync(userMessage);
                    break;

                case ChatIntent.PriceQuery:
                    contextData = await BuildBookSearchContextAsync(userMessage);
                    break;

                case ChatIntent.CategoryBrowse:
                    contextData = await BuildCategoryContextAsync(userMessage);
                    break;

                case ChatIntent.PaymentInfo:
                    contextData =
                        "Payment methods available: Momo e-wallet, Cash on Delivery (COD).";
                    break;

                case ChatIntent.General:
                default:
                    contextData = null;
                    break;
            }

            var prompt = BuildPrompt(userMessage, contextData);
            return await CallOllamaAsync(prompt);
        }

        // Intent detection
        private enum ChatIntent
        {
            General,
            SearchByTitleOrAuthor,
            PriceQuery,
            CategoryBrowse,
            PaymentInfo,
        }

        private static ChatIntent DetectIntent(string message)
        {
            var m = message.ToLowerInvariant();

            string[] priceWords = { "giá", "price", "bao nhiêu tiền", "cost" };
            string[] paymentWords = { "thanh toán", "payment", "momo", "trả tiền", "ship" };
            string[] categoryWords = { "thể loại", "category", "danh mục", "loại sách" };
            string[] searchWords =
            {
                "tìm",
                "search",
                "sách",
                "book",
                "tác giả",
                "author",
                "có quyển",
                "có cuốn",
            };

            if (priceWords.Any(w => m.Contains(w)))
                return ChatIntent.PriceQuery;
            if (paymentWords.Any(w => m.Contains(w)))
                return ChatIntent.PaymentInfo;
            if (categoryWords.Any(w => m.Contains(w)))
                return ChatIntent.CategoryBrowse;
            if (searchWords.Any(w => m.Contains(w)))
                return ChatIntent.SearchByTitleOrAuthor;

            return ChatIntent.General;
        }

        // Db Context builder
        private static readonly string[] PopularityWords =
        {
            "bán chạy",
            "best seller",
            "bestseller",
            "phổ biến",
            "nổi bật",
            "hay nhất",
            "tốt nhất",
            "best",
            "popular",
            "đề xuất",
            "gợi ý",
            "recommend",
        };

        private async Task<string> BuildBookSearchContextAsync(string userMessage)
        {
            var lower = userMessage.ToLowerInvariant();
            var isPopularityQuery = PopularityWords.Any(w => lower.Contains(w));

            var keywords = isPopularityQuery ? new List<string>() : ExtractKeywords(userMessage);

            var baseQuery = _db
                .Books.AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.IsActive);

            List<Book> matched;

            if (keywords.Count == 0)
            {
                matched = await baseQuery.OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync();
            }
            else
            {
                var allActiveBooks = await baseQuery.ToListAsync();

                matched = allActiveBooks
                    .Where(b =>
                        keywords.Any(k => b.Title.Contains(k, StringComparison.OrdinalIgnoreCase))
                        || b.BookAuthors.Any(ba =>
                            keywords.Any(k =>
                                ba.Author.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                    )
                    .Take(5)
                    .ToList();

                // Nothing matched
                if (matched.Count == 0)
                {
                    matched = allActiveBooks.OrderByDescending(b => b.CreatedAt).Take(5).ToList();
                }
            }

            var results = matched
                .Select(b => new
                {
                    b.Title,
                    b.Price,
                    b.Stock,
                    Category = b.Category != null ? b.Category.Name : "N/A",
                    Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                })
                .ToList();

            if (results.Count == 0)
                return "No books currently exist in the store database.";

            var heading = isPopularityQuery
                ? "IMPORTANT: This store has no sales-tracking data. There is no real "
                    + "'best-seller' list. Below are simply the newest books added to stock. "
                    + "You MUST tell the user there is no best-seller ranking, then list these "
                    + "as 'newest arrivals' instead:\n"
                : "Matching books found in store database:\n";

            var sb = new StringBuilder(heading);
            foreach (var b in results)
            {
                var authors = b.Authors.Count > 0 ? string.Join(", ", b.Authors) : "Unknown";
                sb.Append(
                    $"- \"{b.Title}\" by {authors} | Category: {b.Category} | Price: {b.Price:N0} VND | Stock: {b.Stock}\n"
                );
            }
            return sb.ToString();
        }

        private async Task<string> BuildCategoryContextAsync(string userMessage)
        {
            var categories = await _db
                .Categories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

            return "Available categories in store: " + string.Join(", ", categories);
        }

        private static List<string> ExtractKeywords(string message)
        {
            string[] stopWords =
            {
                "tìm",
                "search",
                "sách",
                "book",
                "có",
                "cuốn",
                "quyển",
                "giá",
                "bao",
                "nhiêu",
                "tiền",
                "của",
                "là",
                "gì",
                "không",
                "the",
                "for",
                "a",
                "an",
                "do",
                "you",
                "have",
            };

            var cleaned = new string(
                message
                    .ToLowerInvariant()
                    .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    .ToArray()
            );

            return cleaned
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .Take(5)
                .ToList();
        }

        // PROMPT BUILDING
        private static string BuildPrompt(string userMessage, string? contextData)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SystemPrompt);

            if (!string.IsNullOrWhiteSpace(contextData))
            {
                sb.AppendLine();
                sb.AppendLine("--- Store data for this query ---");
                sb.AppendLine(contextData);
                sb.AppendLine("--- End of data ---");
            }

            sb.AppendLine();
            sb.AppendLine($"Customer: {userMessage}");
            sb.AppendLine("Assistant:");

            return sb.ToString();
        }

        // OLLAMA CALL
        private async Task<string> CallOllamaAsync(string prompt)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var body = new
                {
                    model = ModelName,
                    prompt = prompt,
                    stream = false,
                    options = new { temperature = 0.4 },
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await client.PostAsync(OllamaUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ollama returned status {Status}", response.StatusCode);
                    return "Xin lỗi, trợ lý AI hiện không khả dụng. Vui lòng thử lại sau.";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var reply = doc.RootElement.GetProperty("response").GetString();

                return string.IsNullOrWhiteSpace(reply)
                    ? "Xin lỗi, tôi chưa hiểu câu hỏi của bạn."
                    : reply.Trim();
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Ollama request timed out");
                return "Trợ lý AI phản hồi hơi chậm, vui lòng thử lại.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot reach Ollama — is it running?");
                return "Không thể kết nối tới trợ lý AI cục bộ. Vui lòng kiểm tra Ollama đã chạy chưa.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected chatbot error");
                return "Đã có lỗi xảy ra, vui lòng thử lại.";
            }
        }
    }
}
