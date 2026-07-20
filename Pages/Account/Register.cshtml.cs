using Book_Store.Models;
using Book_Store.ViewModel.users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Book_Store.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(ApplicationDbContext db, ILogger<RegisterModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        // GET /Account/Register
        public IActionResult OnGet()
        {
            // Redirect already-logged-in users away from register page
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Index");

            return Page();
        }

        // POST /Account/Register
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (_db.Users.Any(u => u.Email == Input.Email.Trim()))
            {
                ModelState.AddModelError("Input.Email", "Email này đã được sử dụng.");
                return Page();
            }

            var user = new User
            {
                Email = Input.Email.Trim(),
                Username = Input.Email.Trim(),
                FullName = Input.FullName?.Trim() ?? string.Empty,
                PhoneNumber = Input.PhoneNumber?.Trim(),
                DateOfBirth = Input.DateOfBirth,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", user.Email);

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToPage("/Account/Login");
        }
    }
}
