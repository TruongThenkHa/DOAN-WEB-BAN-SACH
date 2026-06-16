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
        public IActionResult Index()
        {
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
        public IActionResult UserManagement()
        {
            return View();
        }

        // ==============================
        // KHO HÀNG
        // ==============================
        public IActionResult Inventory()
        {
            return View();
        }

        // ==============================
        // DANH SÁCH ĐƠN HÀNG
        // ==============================
        public async Task<IActionResult> OrderList(string? q)
        {
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

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CurrentQ = q;
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
    }

    return RedirectToAction("OrderList");
}
    }
}