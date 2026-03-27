using CS58.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CS58.Pages;

public class IndexModel : PageModel
{
    private readonly MyBlogContext _myBlogContext;

    public IndexModel(MyBlogContext context)
    {
        _myBlogContext = context;
    }

    public List<Article> Articles { get; set; }

    public void OnGet()
    {
        var posts = (
            from a in _myBlogContext.articles 
            orderby a.Created descending
            select a
        ).ToList();
        ViewData["posts"]=posts;
    }
}