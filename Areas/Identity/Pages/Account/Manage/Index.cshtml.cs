﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;
using TutorBuddy.Models.InputModel;
using TutorBuddy.Models.NewFolder;

namespace TutorBuddy.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly TutorBuddyDbContext _dbContext;
        public IndexModel(
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            TutorBuddyDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _hostEnvironment = webHostEnvironment;
        }

        public string Username { get; set; }
        [Display(Name ="First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name ="Year Of Study")]
            public string YearOfStudy { get; set; }

            public string UserProfile { get; set; }

            [Display(Name = "Profile Photo")]
            [NotMapped]
            public IFormFile DisplayPhoto { get; set; }
        }

        private async Task LoadAsync(Profile user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Student student = _dbContext.Students.Where(x => x.StudNum == user.StudNum).FirstOrDefault();
            FirstName = student.FirstName;

            LastName = student.LastName;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                YearOfStudy = student.YearOfStudy,
                UserProfile= student.UserProfile
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            
            try
            {
                Student student = _dbContext.Students.Where(x => x.StudNum == user.StudNum).FirstOrDefault();

                student.YearOfStudy = Input.YearOfStudy;

                if (ModelState.IsValid)
                {
                    if (Input.DisplayPhoto!=null)
                    {
                        //if (student.UserProfile != null)
                        //{
                        //    string filePath = Path.Combine(_hostEnvironment.WebRootPath, "IMGprofiles", student.UserProfile);
                        //    if (filePath!= "DefaultProfile.png")
                        //    {
                        //        System.IO.File.Delete(filePath);
                        //    }
                           
                        //}                      

                        student.DisplayPhoto = Input.DisplayPhoto;
                        student.UserProfile = UploadedFile(student);
                    }
                    _dbContext.Students.Update(student);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch
            {
                StatusMessage = "Your profile update failed, please try again";
                return RedirectToPage();
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
        private string UploadedFile(Student student)
        {
            string uniqueFileName = "DefaultProfile.png";
            if (student.DisplayPhoto != null)
            {
                string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "IMGprofiles");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + student.DisplayPhoto.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    student.DisplayPhoto.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
