using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _myBlogContext;

    public IndexModel(AppDbContext context)
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