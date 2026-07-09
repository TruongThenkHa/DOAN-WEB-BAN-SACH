using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Category.Slug))
            {
                Category.Slug = Category.Name.ToLower().Replace(" ", "-");
            }

            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm danh mục '{Category.Name}' thành công!";
            return RedirectToPage("./Index");
        }
    }
}
