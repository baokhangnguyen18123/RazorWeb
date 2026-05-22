using CS58.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CS58.Areas.Admin.Pages.Role;


[Authorize(Roles = "Admin")] 
public class IndexModel : RolePageModel
{

    public IndexModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext) : base(roleManager, myBlogContext)
    {
    }
    public class RoleModel :IdentityRole
    {
        public string[] Claims { get; set; }
    }
    public List<RoleModel> roles { get; set; }
    
    public async Task OnGet()
    {
        var r = await _roleManager.Roles.ToListAsync();
        roles = new List<RoleModel>();
        foreach (var _r in r)
        {
            var claims = await _roleManager.GetClaimsAsync(_r);
            var claimStrings = claims.Select(c => $"{c.Type}:{c.Value}"); 
            var roleModel = new RoleModel()
            {
                Name = _r.Name,
                Id = _r.Id,
                Claims = claimStrings.ToArray()
            };
            roles.Add(roleModel);
        }
    }
    public void OnPost() => RedirectToPage();
}