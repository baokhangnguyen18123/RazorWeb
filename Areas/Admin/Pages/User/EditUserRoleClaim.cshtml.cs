using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using App.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Admin.Pages.User
{
    public class EditUserRoleClaimModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public EditUserRoleClaimModel(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [TempData]
        public string StatusMessage { get; set; }

        public NotFoundObjectResult OnGet() => NotFound("Không được truy cập");
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
        public AppUser user { get; set; }
        public async Task<IActionResult> OnGetAddClaimAsync(string userId)
        {
            user =  await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{userId}'.");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAddClaimAsync(string userId)
        {
            user =  await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{userId}'.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var claims = _context.UserClaims.Where(c => c.UserId == userId).ToList();
            if (claims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Claim đã tồn tại.");
                return Page();
            }

            // Thêm claim mới vào cơ sở dữ liệu
            await _userManager.AddClaimAsync(user, new Claim(Input.ClaimType, Input.ClaimValue));
            StatusMessage = "Claim đã được thêm thành công.";
            return RedirectToPage("./AddRole", new { id = userId });
        }

        public IdentityUserClaim<string> UserClaim { get; set; }
        public async Task<IActionResult> OnGetEditClaimAsync(int? claimId)
        {
            if(claimId == null)
            {
                return NotFound("Không tìm thấy claim");
            }
            UserClaim = _context.UserClaims.Where(c => c.Id == claimId).FirstOrDefault();
            
            user =  await _userManager.FindByIdAsync(UserClaim.UserId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{UserClaim.UserId}'.");
            }
            Input = new InputModel()
            {
                ClaimType = UserClaim.ClaimType,
                ClaimValue = UserClaim.ClaimValue
            };
            return Page();
        }
        public async Task<IActionResult> OnPostEditClaimAsync(int? claimId)
        {
            if(claimId == null)
            {
                return NotFound("Không tìm thấy claim");
            }
            UserClaim = _context.UserClaims.Where(c => c.Id == claimId).FirstOrDefault();
            
            user =  await _userManager.FindByIdAsync(UserClaim.UserId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{UserClaim.UserId}'.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.UserClaims.Any(c => c.UserId == UserClaim.UserId 
                                        && c.ClaimType == Input.ClaimType 
                                        && c.ClaimValue == Input.ClaimValue 
                                        && c.Id != claimId))
            {
                ModelState.AddModelError(string.Empty, "Claim đã tồn tại.");
                return Page();
            }

            UserClaim.ClaimType = Input.ClaimType;
            UserClaim.ClaimValue = Input.ClaimValue;
            await _context.SaveChangesAsync();
            StatusMessage = "Claim đã được cập nhật thành công.";
            return RedirectToPage("./AddRole", new { id = UserClaim.UserId });
        }
        public async Task<IActionResult> OnPostDeleteClaimAsync(int? claimId)
        {
            if(claimId == null)
            {
                return NotFound("Không tìm thấy claim");
            }
            UserClaim = _context.UserClaims.Where(c => c.Id == claimId).FirstOrDefault();
            
            user =  await _userManager.FindByIdAsync(UserClaim.UserId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{UserClaim.UserId}'.");
            }
            
            await _userManager.RemoveClaimAsync(user, new Claim(UserClaim.ClaimType, UserClaim.ClaimValue));
            await _context.SaveChangesAsync();
            StatusMessage = "Claim đã được cập nhật thành công.";
            return RedirectToPage("./AddRole", new { id = UserClaim.UserId });
        }
    }
}
