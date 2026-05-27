using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Areas.Admin.Pages.Role;
[Authorize(Roles = "Admin")] 
public class AddRoleClaimModel : RolePageModel
{
    public AddRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
    {
        
    }
    public class InputModel
    {
        [Display(Name = "Tên Claim")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [StringLength(256, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
        public string ClaimType { get; set; }
        [Display(Name = "Giá trị Claim")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [StringLength(256, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
        public string ClaimValue { get; set; }
    }
    [BindProperty]
    public InputModel Input { get; set; }

    public IdentityRole role { get; set; }
    public async Task<IActionResult> OnGet(string roleId)
    {
        role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return NotFound($"Không tìm thấy vai trò với ID '{roleId}'.");
        }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string roleId)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }
        role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return NotFound($"Không tìm thấy vai trò với ID '{roleId}'.");
        }
        if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue))
        {
            ModelState.AddModelError(string.Empty, "Claim đã tồn tại.");
            return Page();
        }
        var newClaim =new Claim(Input.ClaimType, Input.ClaimValue);
        var result = await _roleManager.AddClaimAsync(role, newClaim);

        if(!result.Succeeded)
        {
            result.Errors.ToList().ForEach(error =>
            {
                ModelState.AddModelError(string.Empty, error.Description);
            });
        }

        StatusMessage = $"Đã thêm claim '{Input.ClaimType}: {Input.ClaimValue}' vào vai trò {role.Name} thành công.";
        return RedirectToPage("./Edit", new { id = role.Id });
    }
}
