using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;

namespace Book_Store.Pages.Admin.Authors
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Author> Authors { get; set; } = new List<Author>();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNum { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            var query = _context.Authors.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                var qNormalized = Q.Trim().ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(qNormalized));
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            if (PageNum < 1) PageNum = 1;
            if (PageNum > TotalPages && TotalPages > 0) PageNum = TotalPages;

            Authors = await query
                .OrderBy(a => a.Name)
                .Skip((PageNum - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
