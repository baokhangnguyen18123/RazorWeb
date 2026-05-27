using System.ComponentModel.DataAnnotations;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Pkcs;

namespace App.Areas.Admin.Pages.Role
{
    [Authorize(Policy = "AllowEditRole")] 
    public class EditModel : RolePageModel
    {
        public EditModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }
        public class InputModel
        {
            [Display(Name = "Tên vai trò")]
            [Required(ErrorMessage = "Vui lòng nhập {0}")]
            [StringLength(256, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
            public string Name { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }

        public List<IdentityRoleClaim<string>> Claims { get; set; }

        public IdentityRole Role { get; set; }
        public async Task<IActionResult> OnGet(string id)
        {
            if (id == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                Input = new InputModel()
                {
                    Name = role.Name
                };
                Claims = await _context.RoleClaims.Where(rc => rc.RoleId == role.Id).ToListAsync();
                this.Role = role;
                return Page();
            }
            return NotFound("Không tìm thấy vai trò.");
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
            Claims = await _context.RoleClaims.Where(rc => rc.RoleId == role.Id).ToListAsync();
            // 2. Gán tên mới từ form Input
            role.Name = Input.Name;

            // 3. Thực hiện cập nhật thay vì tạo mới
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Đã cập nhật vai trò '{Input.Name}' thành công.";
                return RedirectToPage("./Index");
            }
            else
            {
                // Hiển thị lỗi nếu cập nhật thất bại (ví dụ: trùng tên)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
