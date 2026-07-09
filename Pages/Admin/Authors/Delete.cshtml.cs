using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Authors
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Author Author { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FirstOrDefaultAsync(m => m.AuthorID == id);

            if (author == null)
            {
                return NotFound();
            }
            else
            {
                Author = author;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Check database constraint: check if any books are associated with this author
            var hasLinks = await _context.BookAuthors.AnyAsync(ba => ba.AuthorID == id);
            if (hasLinks)
            {
                TempData["ErrorMessage"] = "Không thể xóa tác giả này vì đang có sách thuộc tác giả đó.";
                return RedirectToPage("./Index");
            }

            var author = await _context.Authors.FindAsync(id);

            if (author != null)
            {
                Author = author;
                _context.Authors.Remove(Author);
                await _context.SaveChangesAsync();

                // Reseed identity if empty
                if (!await _context.Authors.AnyAsync())
                {
                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.Authors', RESEED, 0);");
                }

                TempData["SuccessMessage"] = $"Đã xóa tác giả '{Author.Name}' thành công!";
            }

            return RedirectToPage("./Index");
        }
    }
}
