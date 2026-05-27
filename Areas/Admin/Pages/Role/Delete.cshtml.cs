using System.ComponentModel.DataAnnotations;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Areas.Admin.Pages.Role
{
    [Authorize(Roles = "Admin")] 
    public class DeleteModel : RolePageModel
    {
        public DeleteModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }

        public IdentityRole Role { get; set; }
        public async Task<IActionResult> OnGet(string id)
        {
            if (id == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }
            this.Role = role;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Tìm Role hiện tại trong cơ sở dữ liệu
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }
            
            // 2. Thực hiện xóa
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Đã xóa vai trò '{role.Name}' thành công.";
                return RedirectToPage("./Index");
            }
            else
            {
                // Hiển thị lỗi nếu xóa thất bại 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
