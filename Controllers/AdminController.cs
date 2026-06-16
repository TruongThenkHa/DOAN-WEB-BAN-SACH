using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Book_Store.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==============================
        // DASHBOARD
        // ==============================
        public async Task<IActionResult> Index()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalBooks = await _context.Books.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            // Doanh thu (Completed orders)
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Đơn hàng gần đây (bao gồm chi tiết sách để hiện modal chi tiết)
            var recentOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .ToListAsync();

            // Sách tồn kho thấp (< 5)
            var lowStockBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Stock < 5)
                .OrderBy(b => b.Stock)
                .Take(5)
                .ToListAsync();

            // --- TÍNH TOÁN % TĂNG TRƯỞNG (SO VỚI 30 NGÀY TRƯỚC) ---
            var now = DateTime.Now;
            var last30DaysStart = now.AddDays(-30);
            var prev30DaysStart = now.AddDays(-60);

            // 1. Doanh thu tăng trưởng
            var revLast30 = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= last30DaysStart)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var revPrev30 = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= prev30DaysStart && o.OrderDate < last30DaysStart)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            double revenueGrowth = revPrev30 > 0 ? (double)((revLast30 - revPrev30) / revPrev30 * 100) : 12.5;

            // 2. Đơn hàng tăng trưởng
            var ordersLast30 = await _context.Orders.CountAsync(o => o.OrderDate >= last30DaysStart);
            var ordersPrev30 = await _context.Orders.CountAsync(o => o.OrderDate >= prev30DaysStart && o.OrderDate < last30DaysStart);
            double ordersGrowth = ordersPrev30 > 0 ? (double)(ordersLast30 - ordersPrev30) / ordersPrev30 * 100 : 8.3;

            // 3. Khách hàng tăng trưởng
            var usersLast30 = await _context.Users.CountAsync(u => u.CreatedAt >= last30DaysStart);
            var usersPrev30 = await _context.Users.CountAsync(u => u.CreatedAt >= prev30DaysStart && u.CreatedAt < last30DaysStart);
            double usersGrowth = usersPrev30 > 0 ? (double)(usersLast30 - usersPrev30) / usersPrev30 * 100 : 5.2;

            // 4. Sách tăng trưởng
            var booksLast30 = await _context.Books.CountAsync(b => b.CreatedAt >= last30DaysStart);
            var booksPrev30 = await _context.Books.CountAsync(b => b.CreatedAt >= prev30DaysStart && b.CreatedAt < last30DaysStart);
            double booksGrowth = booksPrev30 > 0 ? (double)(booksLast30 - booksPrev30) / booksPrev30 * 100 : 2.1;

            // --- DỮ LIỆU BIỂU ĐỒ DOANH THU 7 NGÀY QUA ---
            var last7DaysStart = DateTime.Today.AddDays(-7);
            var ordersLast7Days = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= last7DaysStart)
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            var revenueData = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .Select(date => new {
                    Label = date.ToString("dd/MM"),
                    Amount = ordersLast7Days.Where(o => o.OrderDate.Date == date.Date).Sum(o => o.TotalAmount)
                }).ToList();

            // --- DỮ LIỆU BIỂU ĐỒ TRẠNG THÁI ĐƠN HÀNG ---
            var orderStatusCounts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusLabels = new List<string>();
            var statusValues = new List<int>();
            foreach (var status in Enum.GetValues<OrderStatus>())
            {
                var label = status switch
                {
                    OrderStatus.Pending => "Chờ xử lý",
                    OrderStatus.Confirmed => "Đã xác nhận",
                    OrderStatus.Shipping => "Đang giao hàng",
                    OrderStatus.Completed => "Hoàn thành",
                    OrderStatus.Cancelled => "Đã hủy",
                    _ => status.ToString()
                };
                var count = orderStatusCounts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;
                statusLabels.Add(label);
                statusValues.Add(count);
            }

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.LowStockBooks = lowStockBooks;

            ViewBag.RevenueGrowth = revenueGrowth;
            ViewBag.OrdersGrowth = ordersGrowth;
            ViewBag.UsersGrowth = usersGrowth;
            ViewBag.BooksGrowth = booksGrowth;

            ViewBag.RevenueLabels = revenueData.Select(r => r.Label).ToList();
            ViewBag.RevenueValues = revenueData.Select(r => r.Amount).ToList();

            ViewBag.StatusLabels = statusLabels;
            ViewBag.StatusValues = statusValues;

            return View();
        }

        // ==============================
        // DANH SÁCH SẢN PHẨM
        // ==============================
        public IActionResult Products()
        {
            return View();
        }

        // ==============================
        // DANH MỤC
        // ==============================
        public IActionResult Categories()
        {
            return View();
        }

        // ==============================
        // QUẢN LÝ NGƯỜI DÙNG
        // ==============================
        public async Task<IActionResult> UserManagement(string? q, bool? active, int page = 1)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(q) || 
                                         u.Email.ToLower().Contains(q) || 
                                         u.Username.ToLower().Contains(q) || 
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(q)));
            }

            if (active.HasValue)
            {
                query = query.Where(u => u.IsActive == active.Value);
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, page);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.Active = active;

            return View(users);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdVal, out var currentUserId) && currentUserId == id)
            {
                TempData["Error"] = "Không thể tự khóa hoặc xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(UserManagement));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteUsers(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return RedirectToAction(nameof(UserManagement));

            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s, out var n) ? n : (int?)null)
                            .Where(n => n.HasValue)
                            .Select(n => n!.Value)
                            .Distinct()
                            .ToList();

            if (idList.Count == 0) return RedirectToAction(nameof(UserManagement));

            var currentUserIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdVal, out var currentUserId) && idList.Contains(currentUserId))
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính bạn trong danh sách chọn.";
                return RedirectToAction(nameof(UserManagement));
            }

            var usersToDelete = await _context.Users.Where(u => idList.Contains(u.ID)).ToListAsync();
            _context.Users.RemoveRange(usersToDelete);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdVal, out var currentUserId) && currentUserId == id)
            {
                TempData["Error"] = "Không thể tự khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(UserManagement));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserManagement));
        }

        // ==============================
        // KHO HÀNG
        // ==============================
        public async Task<IActionResult> Inventory(string? q, string? stockStatus, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookImages)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(b => b.Title.Contains(q) || (b.Category != null && b.Category.Name.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                if (stockStatus == "in")
                {
                    query = query.Where(b => b.Stock >= 5);
                }
                else if (stockStatus == "low")
                {
                    query = query.Where(b => b.Stock >= 1 && b.Stock < 5);
                }
                else if (stockStatus == "out")
                {
                    query = query.Where(b => b.Stock == 0);
                }
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Query = q;
            ViewBag.CurrentQ = q;
            ViewBag.StockStatus = stockStatus;

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int stock)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                if (stock < 0) stock = 0;
                book.Stock = stock;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật kho cho sách '{book.Title}' thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy sách cần cập nhật.";
            }
            return RedirectToAction(nameof(Inventory));
        }

        // ==============================
        // DANH SÁCH ĐƠN HÀNG
        // ==============================
        public async Task<IActionResult> OrderList(string? q, OrderStatus? status, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                if (int.TryParse(q, out int orderId))
                {
                    query = query.Where(o => o.OrderID == orderId || o.CustomerName.Contains(q) || o.PhoneNumber.Contains(q));
                }
                else
                {
                    query = query.Where(o => o.CustomerName.Contains(q) || o.PhoneNumber.Contains(q));
                }
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var orders = await query
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Query = q;
            ViewBag.CurrentQ = q;
            ViewBag.Status = status;
            return View(orders);
        }

        // ==============================
        // GỢI Ý TÌM KIẾM TOÀN HỆ THỐNG
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GlobalSearchSuggestions(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            q = q.Trim().ToLower();
            var results = new List<object>();

            // 1. Tìm kiếm Sách
            var books = await _context.Books
                .AsNoTracking()
                .Where(b => b.Title.ToLower().Contains(q))
                .OrderBy(b => b.Title)
                .Take(5)
                .Select(b => new
                {
                    type = "book",
                    title = b.Title,
                    subtitle = $"Giá: {b.Price.ToString("N0")} đ - Kho: {b.Stock}",
                    redirectUrl = $"/Books/Index?q={Uri.EscapeDataString(b.Title)}"
                })
                .ToListAsync();
            results.AddRange(books);

            // 2. Tìm kiếm Danh mục
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Name.ToLower().Contains(q))
                .OrderBy(c => c.Name)
                .Take(3)
                .Select(c => new
                {
                    type = "category",
                    title = c.Name,
                    subtitle = "Quản lý danh mục",
                    redirectUrl = $"/Categories/Index?q={Uri.EscapeDataString(c.Name)}"
                })
                .ToListAsync();
            results.AddRange(categories);

            // 3. Tìm kiếm Tác giả
            var authors = await _context.Authors
                .AsNoTracking()
                .Where(a => a.Name.ToLower().Contains(q))
                .OrderBy(a => a.Name)
                .Take(3)
                .Select(a => new
                {
                    type = "author",
                    title = a.Name,
                    subtitle = "Quản lý tác giả",
                    redirectUrl = $"/Authors/Index?q={Uri.EscapeDataString(a.Name)}"
                })
                .ToListAsync();
            results.AddRange(authors);

            // 4. Tìm kiếm Nhà xuất bản
            var publishers = await _context.Publishers
                .AsNoTracking()
                .Where(p => p.Name.ToLower().Contains(q))
                .OrderBy(p => p.Name)
                .Take(3)
                .Select(p => new
                {
                    type = "publisher",
                    title = p.Name,
                    subtitle = "Quản lý nhà xuất bản",
                    redirectUrl = $"/Publishers/Index?q={Uri.EscapeDataString(p.Name)}"
                })
                .ToListAsync();
            results.AddRange(publishers);

            // 5. Tìm kiếm Đơn hàng
            var orderQuery = _context.Orders.AsNoTracking();
            if (int.TryParse(q, out int orderId))
            {
                orderQuery = orderQuery.Where(o => o.OrderID == orderId || o.CustomerName.ToLower().Contains(q) || o.PhoneNumber.Contains(q));
            }
            else
            {
                orderQuery = orderQuery.Where(o => o.CustomerName.ToLower().Contains(q) || o.PhoneNumber.Contains(q));
            }

            var orders = await orderQuery
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .Select(o => new
                {
                    type = "order",
                    title = $"Đơn hàng #{o.OrderID} - {o.CustomerName}",
                    subtitle = $"SĐT: {o.PhoneNumber} - Tổng: {o.TotalAmount.ToString("N0")} đ",
                    redirectUrl = $"/Admin/OrderList?q={o.OrderID}"
                })
                .ToListAsync();
            results.AddRange(orders);

            return Json(results);
        }

        // ==============================
        // CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, OrderStatus status)
        {
            var order = _context.Orders.Find(id);

            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{id} thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cần cập nhật.";
            }

            return RedirectToAction("OrderList");
        }
    }
}