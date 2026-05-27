// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Admin.Pages.User
{
    [Authorize(Roles = "Admin")] 
    public class AddRoleModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AddRoleModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext)
        
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = myBlogContext;
        }
        [TempData]
        public string StatusMessage { get; set; }
        public AppUser user { get; set; }

        [BindProperty]
        [Display(Name = "Vai trò")]
        public string[] RoleNames { get; set; }
        public SelectList allRoles { get; set; }

        public List<IdentityRoleClaim<string>> ClaimsInRole { get; set; }
        public List<IdentityUserClaim<string>> ClaimsOfUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy thành viên");
            }
            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy thành viên có ID: '{id}'.");
            }
            RoleNames = (await _userManager.GetRolesAsync(user)).ToArray<string>();
            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            allRoles = new SelectList(roleNames);
            await GetClaims(id);
            return Page();
        }
        async Task GetClaims(string id)
        {
            var listRoles = from r in _context.Roles
                            join ur in _context.UserRoles on r.Id equals ur.RoleId
                            where ur.UserId == id
                            select r;

            var _claimsInRole = from c in _context.RoleClaims
                                join r in listRoles on c.RoleId equals r.Id
                                select c;
            ClaimsInRole = await _claimsInRole.ToListAsync();

            ClaimsOfUser = await (from c in _context.UserClaims
                                where c.UserId == id
                                select c).ToListAsync();


        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy thành viên");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy thành viên có ID: '{id}'.");
            }
            //RoleNames
            await GetClaims(id);
            var OldRoles = (await _userManager.GetRolesAsync(user)).ToArray();
            var deleteRoles = OldRoles.Where(r => !RoleNames.Contains(r));
            var addRoles = RoleNames.Where(r => !OldRoles.Contains(r));

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            allRoles = new SelectList(roleNames);
            var resultDelete = await _userManager.RemoveFromRolesAsync(user, deleteRoles);
            if(!resultDelete.Succeeded)
            {
                resultDelete.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }

            var resultAdd = await _userManager.AddToRolesAsync(user, addRoles);
            if(!resultAdd.Succeeded)
            {
                resultAdd.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }


            StatusMessage = "Vai trò của bạn đã được cập nhật.";

            return RedirectToPage("./Index");
        }
    }
}
