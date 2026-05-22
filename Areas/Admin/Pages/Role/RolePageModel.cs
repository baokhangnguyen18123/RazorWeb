
using CS58.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CS58.Areas.Admin.Pages.Role;

[Authorize]
public class RolePageModel : PageModel
{
    protected readonly RoleManager<IdentityRole> _roleManager;
    protected readonly MyBlogContext _context;
    [TempData]
    public string StatusMessage { get; set; }
    public RolePageModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext)
    {
        _roleManager = roleManager;
        _context = myBlogContext;
    }
}