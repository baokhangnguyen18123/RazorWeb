// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CS58.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CS58.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Địa chỉ email")]
            public string Email { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        // Post yêu cầu login bằng dịch vụ ngoài
        public async Task<IActionResult> OnPost(string provider, string returnUrl = null)
        {
            // Kiểm tra yêu cầu dịch vụ provider tồn tại
            var listprovider = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var provider_process = listprovider.Find((m) => m.Name == provider);
            if (provider_process == null)
            {
                return NotFound("Dịch vụ không chính xác: " + provider);
            }

            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Lỗi từ nhà cung cấp bên ngoài: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Lỗi khi tải thông tin đăng nhập bên ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Đăng nhập bằng thông tin LoginProvider, ProviderKey từ info cung cấp bởi dịch vụ ngoài
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Đã có Account, đã liên kết với tài khoản ngoài - nhưng không đăng nhập được (có thể do chưa kích hoạt email)
                var userExisted = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (userExisted != null)
                {
                    return RedirectToPage("./RegisterConfirmation", new { Email = userExisted.Email });
                }

                // Chưa có Account liên kết với tài khoản ngoài
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Lỗi khi tải thông tin đăng nhập bên ngoài trong quá trình xác nhận.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                string externalEmail = null;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    externalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                }

                var userWithExternalEmail = (externalEmail != null) ? await _userManager.FindByEmailAsync(externalEmail) : null;

                // 1. Xử lý khi có thông tin về email từ info, đồng thời có user với email đó trong DB
                if ((userWithExternalEmail != null) && (Input.Email == externalEmail))
                {
                    // Xác nhận email luôn nếu chưa xác nhận
                    if (!userWithExternalEmail.EmailConfirmed)
                    {
                        var codeactive = await _userManager.GenerateEmailConfirmationTokenAsync(userWithExternalEmail);
                        await _userManager.ConfirmEmailAsync(userWithExternalEmail, codeactive);
                    }

                    // Thực hiện liên kết info và user
                    var resultAdd = await _userManager.AddLoginAsync(userWithExternalEmail, info);
                    if (resultAdd.Succeeded)
                    {
                        await _signInManager.SignInAsync(userWithExternalEmail, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Không thể liên kết tài khoản.");
                        return Page();
                    }
                }

                // 2. Tài khoản chưa có, tạo tài khoản mới
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Liên kết tài khoản ngoài với tài khoản vừa tạo
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Đã tạo user mới từ thông tin {Name}.", info.LoginProvider);

                        // Email tạo tài khoản và email từ info giống nhau -> xác thực email luôn
                        if (Input.Email == externalEmail)
                        {
                            var codeactive = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            await _userManager.ConfirmEmailAsync(user, codeactive);
                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }

                        // Trường hợp Email nhập vào khác Email từ Google, yêu cầu gửi mail xác thực
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Xác nhận địa chỉ email",
                            $"Hãy xác nhận địa chỉ email bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>bấm vào đây</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}