using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Admin.Pages.Role;
[Authorize(Roles = "Admin")] 
public class EditRoleClaimModel : RolePageModel
{
    public EditRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
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

    IdentityRoleClaim<string> claim { get; set; }

    public async Task<IActionResult> OnGet(int? claimId)
    {
        if (claimId == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        claim = await _context.RoleClaims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        role = await _roleManager.FindByIdAsync(claim.RoleId);
        if (role == null)
        {
            return NotFound($"Không tìm thấy vai trò với ID '{claim.RoleId}'.");
        }
        Input = new InputModel
        {
            ClaimType = claim.ClaimType,
            ClaimValue = claim.ClaimValue
        };
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int? claimId)
    {
        if (claimId == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        claim = await _context.RoleClaims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        role = await _roleManager.FindByIdAsync(claim.RoleId);
        if (!ModelState.IsValid)
        {
            return Page();
        }
        if (role == null)
        {
            return NotFound($"Không tìm thấy vai trò'.");
        }
        if (await _context.RoleClaims.AnyAsync(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.RoleId == role.Id))
        {
            ModelState.AddModelError(string.Empty, "Claim đã tồn tại.");
            return Page();
        }
        claim.ClaimType = Input.ClaimType;
        claim.ClaimValue = Input.ClaimValue;

        await _context.SaveChangesAsync();

        StatusMessage = $"Đã cập nhật claim '{Input.ClaimType}: {Input.ClaimValue}' cho vai trò {role.Name} thành công.";
        return RedirectToPage("./Edit", new { id = role.Id });
    }
    public async Task<IActionResult> OnPostDeleteAsync(int? claimId)
    {
        if (claimId == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        claim = await _context.RoleClaims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null)
        {
            return NotFound($"Không tìm thấy claim với ID '{claimId}'.");
        }
        role = await _roleManager.FindByIdAsync(claim.RoleId);
        if (role == null)
        {
            return NotFound($"Không tìm thấy vai trò'.");
        }


        await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));

        StatusMessage = $"Đã xóa claim '{claim.ClaimType}: {claim.ClaimValue}' khỏi vai trò {role.Name} thành công.";
        return RedirectToPage("./Edit", new { id = role.Id });
    }

}
