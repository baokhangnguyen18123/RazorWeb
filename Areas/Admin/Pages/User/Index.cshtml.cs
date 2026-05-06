using Bogus;
using CS58.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CS58.Areas.Admin.Pages.User;

[Authorize(Roles = "Admin")] 
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    public IndexModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [TempData]
    public string StatusMessage { get; set; }
    public class UserAndRoles : AppUser
    {
        public string RoleNames { get; set; }
    }
    public List<UserAndRoles> users { get; set; }

    public const int ITEMS_PER_PAGE = 15;

    [BindProperty(SupportsGet =true,Name ="p")]
    public int currentPage {get;set;}
    public int countPage {get;set;}

    public int totalUsers {get;set;}

    public async Task OnGetAsync() // Đổi tên thành OnGetAsync theo chuẩn đặt tên bất đồng bộ
    {
        var qr = _userManager.Users.OrderBy(u => u.UserName);
        totalUsers = await qr.CountAsync();
        countPage = (int)Math.Ceiling((double)totalUsers / ITEMS_PER_PAGE);

        if (countPage == 0) 
        {
            countPage = 1; 
        }

        if (currentPage < 1)
        {
            currentPage = 1;
        }
        if (currentPage > countPage)
        {
            currentPage = countPage;
        }

        var query = qr.Skip((currentPage - 1) * ITEMS_PER_PAGE)
                    .Take(ITEMS_PER_PAGE)
                    .Select(u => new UserAndRoles 
                    {
                        Id = u.Id,
                        UserName = u.UserName
                        // KHÔNG gọi _userManager ở đây
                    });
        users = await query.ToListAsync();

        foreach (var user in users)
        {
            // await ở đây chuẩn bất đồng bộ, không dùng .Result
            var roles = await _userManager.GetRolesAsync(user); 
            user.RoleNames = string.Join(", ", roles);
        }
    }
    public void OnPost() => RedirectToPage();
}