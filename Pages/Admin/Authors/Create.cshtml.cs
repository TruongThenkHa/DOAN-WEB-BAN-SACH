using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Authors
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Author Author { get; set; } = new();

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
            var exists = await _context.Authors.AnyAsync(a => a.Name == Author.Name);
            if (exists)
            {
                ModelState.AddModelError("Author.Name", "Tác giả này đã tồn tại trong hệ thống.");
                return Page();
            }

            _context.Authors.Add(Author);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm tác giả '{Author.Name}' thành công!";
            return RedirectToPage("./Index");
        }
    }
}
