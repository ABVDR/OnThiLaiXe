// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OnThiLaiXe.Models;
using OnThiLaiXe.Services;

namespace OnThiLaiXe.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IGmailSender _gmailSender;
        private readonly IBackgroundJobClient _backgroundJobClient;


        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IBackgroundJobClient backgroundJobClient,
            IGmailSender gmailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            }
            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                try
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created successfully.");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            null,
                            new { area = "Identity", userId, code, returnUrl },
                            Request.Scheme);

                        var emailBody = $@"
                <!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Chào mừng bạn, {user.FullName}!</title>
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
                <h2 style='text-align: center; color: #333;'>Xin chào {user.FullName}!</h2>
                <p style='color: #555; font-size: 16px; text-indent: 40px;'>
                    {user.FullName}, chúng tôi rất vui khi bạn đăng nhập thành công! Cám ơn bạn đã lựa chọn sử dụng trang web của tôi giữa ngàn vạn trang web ngoài kia.
                    Tài khoản của bạn là chìa khóa mở ra một thế giới đầy tính năng thú vị và tiện ích. 
                    Hãy khám phá ngay và tận hưởng những trải nghiệm tốt nhất mà chúng tôi mang lại!
                </p>
            </td>
        </tr>

        <!-- Lời nhắn tạo sự kết nối -->
        

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
                    Nếu có bất kỳ câu hỏi nào, {user.FullName} đừng ngần ngại liên hệ với chúng tôi. 
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


                    // Gửi email sau 7 ngay 
                    var scheduledEmailBody = $@" <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Ưu đãi VIP - Trải nghiệm ngay!</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <table width='100%' cellspacing='0' cellpadding='0' 
        style='max-width: 600px; margin: auto; background: white; padding: 20px; 
        border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>

    <tr>
        <td align='center' style='background-color: #8a2be2; padding: 15px; border-radius: 8px 8px 0 0;'>
            <h3 style='color: #fff; margin: 0;'>Bạn đã sẵn sàng khám phá toàn bộ tính năng?</h3>
        </td>
    </tr>
   
    <!-- Logo -->
    <tr>
        <td align='center' style='padding-top: 20px;'>
            <img src='https://media.loveitopcdn.com/3807/logo-hutech-2.png' alt='LOGO' 
                style='max-width: 80px; margin-bottom: 15px;'>
        </td>
    </tr>
    
    <tr>
        <td>
            <h2 style='text-align: center; color: #333; margin-bottom: 10px;'>Xin chào {user.FullName}!</h2>
            <p style='color: #555; font-size: 16px; text-align: justify; line-height: 1.6;'>
                Chúng tôi hy vọng {user.FullName} đã có khoảng thời gian thú vị khi khám phá dịch vụ của <strong>ProTest</strong>. Nhưng đó mới chỉ là khởi đầu! Hãy nâng cấp lên <strong>Plus</strong> để mở khóa toàn bộ tính năng cao cấp và tận hưởng trải nghiệm trọn vẹn hơn.        
            </p>
        </td>
    </tr>

    <!-- Danh sách lợi ích -->
    <tr>
        <td align='center'>
            <ul style='list-style-type: none; padding: 0; margin: 20px 0; text-align: left; display: inline-block; font-size: 16px; line-height: 1.6;'>
                <li>✔️ Truy cập không giới hạn vào tất cả các tính năng cao cấp</li>
                <li>✔️ Hỗ trợ ưu tiên 24/7 từ đội ngũ chuyên gia</li>
                <li>✔️ Tùy chỉnh giao diện và trải nghiệm theo sở thích</li>
                <li>✔️ Nhận cập nhật sớm nhất về các tính năng mới</li>
                <li>✔️ Ưu đãi đặc biệt dành riêng cho thành viên VIP</li>
            </ul>
        </td>
    </tr>

    <!-- Nút CTA -->
    <tr>
        <td align='center' style='padding: 20px 0;'>
            <a href='https://localhost:7289/' 
                style='background-color: #8a2be2; color: white; padding: 12px 24px; 
                text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>
                 Khám phá ngay
            </a>
        </td>
    </tr>
    
    <tr>
        <td style='text-align: center;'>
            <p style='font-size: 14px; color: #444;'>
                Nếu có bất kỳ câu hỏi nào, {{user.FullName}} đừng ngần ngại liên hệ với chúng tôi. 
                Đội ngũ hỗ trợ luôn sẵn sàng giúp bạn bất cứ lúc nào! 
            </p>
        </td>
    </tr>
    
    <!-- Footer -->
    <tr>
        <td style='text-align: center; font-size: 12px; color: #999; padding-top: 20px; border-top: 1px solid #ddd;'>
            123 Xa Lộ Hà Nội, TP.HCM, Việt Nam<br>
            +84 222 333 123 | support@ProTest.com<br>
            © 2025 Your Company. All Rights Reserved.
        </td>
    </tr>
</table>
</body>
</html>
";

                        try
                        {
                            await _emailSender.SendEmailAsync(Input.Email, "Xác nhận email", emailBody);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Gửi email xác nhận thất bại: {ex.Message}");
                        }

                        try
                        {
                            string role = string.IsNullOrEmpty(Input.Role) ? SD.Role_Customer : Input.Role;
                            await _userManager.AddToRoleAsync(user, role);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Gán role thất bại: {ex.Message}");
                        }

                        try
                        {
                            _backgroundJobClient.Enqueue(() => _gmailSender.SendEmailAsync(user.Email, "Chào mừng bạn!", emailBody));
                            _backgroundJobClient.Schedule(() => _gmailSender.SendEmailAsync(user.Email, "Chào mừng bạn!", emailBody), TimeSpan.FromSeconds(60));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Gửi email nền thất bại: {ex.Message}");
                        }

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi khi tạo tài khoản: {ex.Message}");
                }
            }

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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
