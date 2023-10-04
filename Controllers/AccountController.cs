using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;
using TutorBuddy.Models.Helper;
using TutorBuddy.Models.InputModel;
using TutorBuddy.Models.NewFolder;

namespace TutorBuddy.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TutorBuddyDbContext Db;
        private readonly UserManager<Profile> userManager;
        private readonly Profile profile;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostEnvironment;     

        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public AccountController(TutorBuddyDbContext Db, UserManager<Profile> userManager,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostEnvironment)
        {
            this.Db = Db;
            this.userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _hostEnvironment = hostEnvironment;
            profile = JsonConvert.DeserializeObject<Profile>(_session.GetString("User"));
        }
        // GET: AccountController/Create
        [HttpGet]
        public ActionResult RegisterStudent()
        {
            //clean the database of null student numbers
            //cleanUpDatabase();
            if (profile == null)
            {
                var callUrl = Url.Page(
                         "/Account/Register",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);
                return RedirectToPage(callUrl);
            }
          
            StudentInputModel student = new();
            //Get name and surname
            string[] FirstLastName = profile.ProfileName.Split(' ');
            student.Student.FirstName = FirstLastName[0];
            student.Student.LastName = FirstLastName[1];

            //Pass Data for drop down options through temp data
            List<Module> modules = Db.Module.ToList();
            ViewData["Modules"] = modules;

            student.Student.HourlyFee = 0.00;
            student.Student.TypeOfTutor = false;         
            
            //pass new student to view
            return View(student);
        }        
           
        public ActionResult RegisterTutor()
        {
            if (profile == null)
            {
                var callUrl = Url.Page(
                         "/Account/Register",
                         pageHandler: null,
                         values: new { area = "Identity",returnUrl = "~/" },
                         protocol: Request.Scheme);
                return RedirectToPage(callUrl);
            }

            TutorInputModel Tutor = new();
            //Get name and surname
            string[] FirstLastName = profile.ProfileName.Split(' ');
            Tutor.Student.FirstName = FirstLastName[0];
            Tutor.Student.LastName = FirstLastName[1];
            //Add one dummy data for Timeslots
            Tutor.Student.TimeSlots = new List<TimeSlot>();

            Tutor.Student.TimeSlots.Add(new TimeSlot()
            {
                TutorID = Tutor.Student.TutorID,
                SessionDay = "",
                SessionStartTime = DateTime.Parse("00:00"),
                SessionEndTime = DateTime.Parse("00:00")
            });

            Tutor.Modules = Db.Module.ToList();
            //pass tutor to view

            return View(Tutor);
        }

        private void cleanUpDatabase()
        {
            List<Profile> profiles = Db.Users.ToList();

            foreach (Profile profile in profiles)
            {
                if (profile.StudNum == null)
                {
                    Db.Users.Remove(profile);
                }
            }
            Db.SaveChanges();
        }

        private string UploadedFile(Student student) {
            string uniqueFileName = "DefaultProfile.png";
            if (student.DisplayPhoto != null)
            {
                string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "IMGprofiles");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + student.DisplayPhoto.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                    student.DisplayPhoto.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        public void Notify(string message, string title = "Sweet Toast", NotificationType notification = NotificationType.success)
        {
            var msg = new
            {
                text = message,
                title = title,
                icon = notification.ToString(),
                type = notification.ToString(),
                provider = "sweetAlert"

            };

            TempData["Notify"] = JsonConvert.SerializeObject(msg);
        }
        
        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStudent(StudentInputModel Stud)
        {
            try
            {

                Stud.Student.TypeOfTutor = false;
                //update Student and add to table

                string uniqueFileName = UploadedFile(Stud.Student);
                Stud.Student.UserProfile = uniqueFileName;
                    profile.StudNum = Stud.Student.StudNum;
                    profile.Student = Stud.Student;
                //update user table
                //add student in the student table
                //Db.Students.Add(Stud.Student);
                Db.Add(Stud.Student);

                await Db.SaveChangesAsync();

                var result = await userManager.CreateAsync(profile, profile.Password);
                    
                if (!result.Succeeded)
                {               
                
                    throw new DbUpdateException();
                }
                
                Notify("User successful created", "Registration", NotificationType.success);
                //Authenticate user Account
                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    var callUrl = Url.Page(
                         "/Account/RegisterConfirmation",
                         pageHandler: null,
                         values: new { area = "Identity", email = profile.Email, returnUrl = "~/" },
                         protocol: Request.Scheme);
                    return Redirect(callUrl);
                }
                else
                {
                    var callUrl = Url.Page(
                        "/Account/Register",
                        pageHandler: null,
                        values: new { area = "Identity", returnUrl = "~/" },
                        protocol: Request.Scheme);
                    return RedirectToPage(callUrl);
                }
            }
            catch
            {
                cleanUpDatabase();
                Notify("User Not Created, Please Try again", "Registration", NotificationType.error);
                return View(Stud);
            }
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTutor(TutorInputModel Tutor)
        {
            if (Tutor.Student.ModuleList.Count== 0)
            {
                Notify("Please pick modules", "Registration", NotificationType.warning);

                Tutor.Modules = Db.Module.ToList();

                return View(Tutor);
            }

            Student exit = Db.Students.Where(x => x.StudNum == Tutor.Student.StudNum).FirstOrDefault();
            if (exit != null)
            {
                Notify("Student Number Already exist, Please input your student number", "Registration", NotificationType.warning);

                Tutor.Modules = Db.Module.ToList();

                return View(Tutor);
            }
            try
            {
                string uniqueFileName = UploadedFile(Tutor.Student);   
                Tutor.Student.UserProfile = uniqueFileName;
                //Assign TutorID
                Tutor.Student.TutorID = "T" + Tutor.Student.StudNum;
                    profile.TutorID = Tutor.Student.TutorID;
                    profile.StudNum = Tutor.Student.StudNum;
                //declare TutorModule list
                   Tutor.Student.TutorModules = new List<TutorModule>();
                    //add student's modules
                    foreach(string item in Tutor.Student.ModuleList)
                    {
                        TutorModule tutorModule = new();
                        tutorModule.TutorID = Tutor.Student.TutorID;
                        tutorModule.ModuleCode = item;
                        //Find Module in DB
                        Module tempModule = Db.Module.Where(x => x.ModuleCode.Equals(item)).FirstOrDefault<Module>();
                        tutorModule.Module = tempModule;

                    Tutor.Student.TutorModules.Add(tutorModule);
                    }
                    foreach (TimeSlot time in Tutor.Student.TimeSlots)
                    {
                        time.TutorID = Tutor.Student.TutorID;
                    }
                Db.Add(Tutor.Student);
                //save all changes in the database

                await Db.SaveChangesAsync();
                //Update Profile Table

                await userManager.CreateAsync(profile, profile.Password);
                //Authenticate user Account 

                Notify("User successful created", "Registration", NotificationType.success);

                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    var callUrl = Url.Page(
                         "/Account/RegisterConfirmation",
                         pageHandler: null,
                         values: new { area = "Identity", email = profile.Email, returnUrl = "~/" },
                         protocol: Request.Scheme);
                    return Redirect(callUrl);
                }
                else
                {
                    var callUrl = Url.Page(
                        "/Account/Register",
                        pageHandler: null,
                        values: new { area = "Identity" },
                        protocol: Request.Scheme);
                    return RedirectToPage(callUrl);
                }

            }
            catch
            {
                cleanUpDatabase();
                Tutor.Modules = Db.Module.ToList();
                Notify("Error occurred", "Registration", NotificationType.warning);
                return View(Tutor);
            }
        }
    }
}