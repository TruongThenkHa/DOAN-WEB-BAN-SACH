using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Publishers
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Publisher Publisher { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers.FirstOrDefaultAsync(m => m.PublisherID == id);

            if (publisher == null)
            {
                return NotFound();
            }
            else
            {
                Publisher = publisher;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Check database constraint: check if any books are associated with this publisher
            var hasBooks = await _context.Books.AnyAsync(b => b.PublisherID == id);
            if (hasBooks)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà xuất bản này vì đang có sách thuộc nhà xuất bản đó.";
                return RedirectToPage("./Index");
            }

            var publisher = await _context.Publishers.FindAsync(id);

            if (publisher != null)
            {
                Publisher = publisher;
                _context.Publishers.Remove(Publisher);
                await _context.SaveChangesAsync();

                // Reseed identity if empty
                if (!await _context.Publishers.AnyAsync())
                {
                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.Publishers', RESEED, 0);");
                }

                TempData["SuccessMessage"] = $"Đã xóa nhà xuất bản '{Publisher.Name}' thành công!";
            }

            return RedirectToPage("./Index");
        }
    }
}
