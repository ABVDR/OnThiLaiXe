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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OnThiLaiXe.Models;
using Hangfire;
using OnThiLaiXe.Services;

namespace OnThiLaiXe.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IGmailSender _gmailSender;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IBackgroundJobClient backgroundJobClient,
            IGmailSender gmailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _backgroundJobClient = backgroundJobClient;
            _gmailSender = gmailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Nếu đã từng đăng nhập bằng Google thì đăng nhập luôn
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            // Tự động tạo tài khoản nếu chưa có
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                ErrorMessage = "Email not received from external provider.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var user = new ApplicationUser { UserName = email, Email = email };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);

                // Tạo role nếu chưa có
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                // Gán role mặc định
                await _userManager.AddToRoleAsync(user, "Customer");

                // Gửi xác nhận email (tuỳ chọn)
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = encodedCode },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                var emailBody = $@"
            <!DOCTYPE html>
            <html lang='vi'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Chào mừng bạn!</title>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <table width='100%' cellspacing='0' cellpadding='0' 
                    style='max-width: 600px; margin: auto; background: white; padding: 20px; 
                    border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>

                    <!-- Header đẹp hơn -->
                    <tr>
                        <td align='center' style='background-color: #25b9fe; padding: 15px; border-radius: 8px 8px 0 0;'>
                            <h3 style='color: rgb(251, 249, 249); margin: 0px;'> Chào mừng bạn đến với <b>ProTest </b>! </h3>
                        </td>
                    </tr>

                    <!-- Logo -->
                    <tr>
                        <td align='center' style='padding-top: 20px;'>
                            <img src='https://media.loveitopcdn.com/3807/logo-hutech-2.png' alt='LOGO' 
                                style='max-width: 70px; margin-bottom: 20px;'>
                        </td>
                    </tr>

                    <!-- Nội dung chính -->
                    <tr>
                        <td>
                            <h2 style='text-align: center; color: #333;'>Xin chào!</h2>
                            <p style='color: #555; font-size: 16px; text-indent: 40px;'>
                                Chúng tôi rất vui khi bạn đăng nhập thành công qua Google! Cám ơn bạn đã lựa chọn sử dụng trang web của tôi giữa ngàn vạn trang web ngoài kia.
                                Tài khoản của bạn là chìa khóa mở ra một thế giới đầy tính năng thú vị và tiện ích. 
                                Hãy khám phá ngay và tận hưởng những trải nghiệm tốt nhất mà chúng tôi mang lại!
                            </p>
                        </td>
                    </tr>

                    <!-- Nút điều hướng -->
                    <tr>
                        <td align='center' style='padding: 20px 0;'>
                            <a href='https://localhost:7289/' 
                                style='background-color: #25b9fe; color: white; padding: 12px 24px; 
                                text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                                Khám phá ngay
                            </a>
                        </td>
                    </tr>
                    <tr>
                        <td style='text-align: center; '>
                            <p style='font-size: 14px; color: #444;'>
                                Nếu có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi. 
                                Đội ngũ hỗ trợ luôn sẵn sàng giúp bạn bất cứ lúc nào! 
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='text-align: center; font-size: 12px; color: #999; padding-top: 20px;'>
                            123 Xa Lộ Hà Nội, TP.HCM, Việt Nam<br>
                            +84 222 333 123 |  support@ProTest.com<br>
                            © 2025 Your Company. All Rights Reserved.
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

                // Gửi email chào mừng
                _backgroundJobClient.Enqueue(() => _gmailSender.SendEmailAsync(user.Email, "Chào mừng bạn!", emailBody));
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
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

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
