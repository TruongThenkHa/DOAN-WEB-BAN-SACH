using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Publishers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Publisher> Publishers { get; set; } = new List<Publisher>();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNum { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            var query = _context.Publishers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                var qNormalized = Q.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(qNormalized));
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            if (PageNum < 1) PageNum = 1;
            if (PageNum > TotalPages && TotalPages > 0) PageNum = TotalPages;

            Publishers = await query
                .OrderBy(p => p.Name)
                .Skip((PageNum - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
