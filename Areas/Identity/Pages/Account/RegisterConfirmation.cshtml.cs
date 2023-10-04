using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TutorBuddy.Models;
using TutorBuddy.Services;
using Microsoft.Extensions.Configuration;

namespace TutorBuddy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<Profile> _userManager;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;

        public RegisterConfirmationModel(UserManager<Profile> userManager,IMailService mailService,IConfiguration configuration)
        {
            _userManager = userManager;
            _mailService = mailService;
            _configuration = configuration;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            returnUrl = $"{_configuration["AppUrl"]}/Identity/Account/ConfirmEmail?userId={userId}&code={code}";
            await _mailService.SendEmailAsync(Email, "Confirm Email Account", $"<h1> Hello!, Please click on the link to confirm your email <a href={returnUrl} style={"padding: 8px 10px"}>Verify Email</a>");

            /*DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                  "/Account/ConfirmEmail",
                  pageHandler: null,
                  values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                  protocol: Request.Scheme);
            }*/

            return Page();
        }
    }
}
