using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Publishers
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Publisher Publisher { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check duplicate name
            var exists = await _context.Publishers.AnyAsync(p => p.Name == Publisher.Name);
            if (exists)
            {
                ModelState.AddModelError("Publisher.Name", "Nhà xuất bản này đã tồn tại trong hệ thống.");
                return Page();
            }

            _context.Publishers.Add(Publisher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm nhà xuất bản '{Publisher.Name}' thành công!";
            return RedirectToPage("./Index");
        }
    }
}
