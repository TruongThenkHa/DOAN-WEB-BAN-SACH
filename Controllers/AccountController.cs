using Microsoft.AspNetCore.Mvc;
using Book_Store.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Book_Store.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isExist = _context.Users.Any(u => u.Email == model.Email);

                if (isExist)
                {
                    ModelState.AddModelError("", "Email này đã được đăng ký!");
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    Username = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                TempData["ErrorMessage"] = "Đăng nhập thất bại!";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = "Đăng nhập thành công!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        */

        [HttpGet]
        public IActionResult Profile()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var userIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdVal, out var userId))
            {
                var user = _context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Book)
                    .FirstOrDefault(u => u.ID == userId);

                if (user != null)
                {
                    user.Orders = user.Orders.OrderByDescending(o => o.OrderDate).ToList();
                    return View(user);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var userIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdVal, out var userId))
            {
                var user = _context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Book)
                    .FirstOrDefault(u => u.ID == userId);

                if (user != null)
                {
                    // Loại bỏ validation cho các trường không chỉnh sửa trong form này
                    ModelState.Remove("Email");
                    ModelState.Remove("PasswordHash");
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                    ModelState.Remove("Username");

                    if (string.IsNullOrWhiteSpace(model.FullName))
                    {
                        ModelState.AddModelError("FullName", "Họ và tên không được để trống.");
                    }
                    if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Số điện thoại không được để trống.");
                    }
                    if (!model.DateOfBirth.HasValue)
                    {
                        ModelState.AddModelError("DateOfBirth", "Ngày sinh không được để trống.");
                    }

                    if (ModelState.IsValid)
                    {
                        user.FullName = model.FullName;
                        user.PhoneNumber = model.PhoneNumber;
                        user.DateOfBirth = model.DateOfBirth;
                        user.Address = model.Address;

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                        return RedirectToAction("Profile");
                    }

                    user.Orders = user.Orders.OrderByDescending(o => o.OrderDate).ToList();
                    return View(user);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }
    }
}