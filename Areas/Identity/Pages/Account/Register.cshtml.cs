using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TutorBuddy.CustomValidation;
using TutorBuddy.Models;

namespace TutorBuddy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Profile> _signInManager;
        private readonly UserManager<Profile> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name ="Register As Tutor")]
            public bool IsTutor { get; set; }

            public string TutorID { get; set; }

            [Required]
            [EmailAddress]
            //[EmailValidation("Please enter your school email!")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl= Url.Page(
                         "/Account/Register",
                         pageHandler: null,
                         values: new { area = "Identity"},
                         protocol: Request.Scheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                string Profilename = Input.FirstName.Trim() + " " + Input.LastName.Trim();

                var user = new Profile
                {
                    ProfileName = Profilename,
                    UserName = Input.Email,
                    Email = Input.Email,
                    IsTutor = Input.IsTutor,
                    Password = Input.Password
                };               

                /*var result = await _userManager.CreateAsync(user, Input.Password); */

                if (true)
                {

                    /* _logger.LogInformation("User created a new account with password.");

                     var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                     code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                     var callbackUrl = Url.Page(
                         "/Account/ConfirmEmail",
                         pageHandler: null,
                         values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                         protocol: Request.Scheme);

                     await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                         $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                     if (_userManager.Options.SignIn.RequireConfirmedAccount)
                     {
                         return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                     }
                     else
                     {
                         await _signInManager.SignInAsync(user, isPersistent: false);
                         return LocalRedirect(returnUrl);
                     }*/
                    _logger.LogInformation("User created a new account with password.");
                    HttpContext.Session.SetString("User",JsonConvert.SerializeObject(user));
                   
                    if (Input.IsTutor)
                    {
                        return RedirectToAction("RegisterTutor", "Account");
                    }
                    else
                    {
                        return RedirectToAction("RegisterStudent", "Account");
                    }
                    
                    
                }
               
                /*foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }*/
            }

            // If we got this far, something failed, redisplay form
            
            return Page();
        }
    }
}
