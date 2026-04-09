using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CS58.Models;
using Microsoft.AspNetCore.Authorization;

namespace CS58.Pages_Blog
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CS58.Models.MyBlogContext _context;

        public IndexModel(CS58.Models.MyBlogContext context)
        {
            _context = context;
        }

        public IList<Article> Article { get;set; } = default!;

        public const int ITEMS_PER_PAGE = 15;

        [BindProperty(SupportsGet =true,Name ="p")]
        public int currentPage {get;set;}
        public int countPage {get;set;}


        // public async Task OnGetAsync(string SearchString)
        // {
        //     // Article = await _context.articles.ToListAsync();
        //     var qr = from a in _context.articles
        //             orderby a.Created descending
        //             select a;

        //     if (!string.IsNullOrEmpty(SearchString))
        //     {
        //         Article = qr.Where(a => a.Title.Contains(SearchString)).ToList();
        //     }
        //         Article = await qr.ToListAsync();
        // }
        public async Task OnGetAsync(string SearchString)
        {
            var qr = _context.articles.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                qr = qr.Where(a => a.Title.Contains(SearchString));
            }

            int totalArtical = await _context.articles.CountAsync();
            countPage = (int)Math.Ceiling((double)totalArtical/ITEMS_PER_PAGE);

            if (currentPage < 1)
            {
                currentPage=1;
            }
            if (currentPage > countPage)
            {
                currentPage=countPage;
            }

            Article = await qr
                .OrderByDescending(a => a.Created)
                .Skip((currentPage-1)*ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToListAsync();
        }
        
    }
}
