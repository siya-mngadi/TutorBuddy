using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;
using TutorBuddy.Models.Helper;
using TutorBuddy.Models.InputModel;
using TutorBuddy.Models.NewFolder;
using TutorBuddy.Services;

namespace TutorBuddy.Controllers
{
    //[AllowAnonymous]
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly TutorBuddyDbContext Db;
        private readonly UserManager<Profile> userManager;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(TutorBuddyDbContext Db, UserManager<Profile> userManager,
            IWebHostEnvironment hostEnvironment,
            IMailService mailService)
        {
            this.Db = Db;
            this.userManager = userManager;
            _hostEnvironment = hostEnvironment;
            _mailService = mailService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Booking(string studentId)
        {
            ParentModel parentModel = new();

            parentModel.Profile = userManager.FindByNameAsync(User.Identity.Name).Result;
            parentModel.Student = Db.Students.Where(x => x.StudNum.Equals(parentModel.Profile.StudNum)).SingleOrDefault();
            parentModel.Tutor = Db.Students.Where(x => x.StudNum.Equals(studentId)).SingleOrDefault();
            parentModel.TimeSlots = Db.TimeSlots.Where(x => x.TutorID == parentModel.Tutor.TutorID).ToList();
            // parentModel.TutorModule = Db.TutorModules.Where(x => x.TutorModuleID.Equals(parentModel.TutorModule.TutorModuleID)).SingleOrDefault();
            parentModel.SingleBooking = new SingleBooking() {
                StudNum = parentModel.Student.StudNum,
                Stud = parentModel.Student,
                BookingFee = parentModel.Tutor.HourlyFee,
                TutorModule = new TutorModule(),
                TutorModuleID = -1
            };


            //ViewData["MembershipID"] = items;
            parentModel.ListModules = GetModules(parentModel.Tutor);
            //ViewBag.ListDays = GetDays(parentModel.Tutor);

            return View(parentModel);
        }
        private DateTime getSessionDateTime(TimeSlot slot)
        {
            //get year 
            int year = DateTime.Today.Year;
            //get Month
            int month = DateTime.Today.Month;

            int day = DateTime.Today.Day;

            for (int y = month; y < 12; y++)
            {
                int daysinMonth = DateTime.DaysInMonth(year, y);
                for (int i = day; i <= daysinMonth; i++)
                {
                    DateTime date = new DateTime(year, y, i);
                    string dayOfWeek = date.ToString("dddd");

                    if (dayOfWeek.Equals(slot.SessionDay))
                    {
                        return date;
                    }

                }
                day = 1;
            }
            return DateTime.Today;
        }

        private List<TutorModule> GetModules(Student Tutor)
        {
            return Db.TutorModules.Where(x=>x.TutorID==Tutor.TutorID).ToList();
        }

        private IEnumerable<SelectListItem> GetDays(Student Tutor)
        {
            return Db.TimeSlots.Where(x => x.TutorID == Tutor.TutorID)
                          .Select(s => new SelectListItem
                          {
                              Value = s.TimeSlotID.ToString(),
                              Text = s.SessionDay
                          }).ToList();
        }
        public IActionResult StudentBooking()
        {

            return View();
        }
        public IActionResult ViewTutor()
        {
            return View();
        }

        public IActionResult ViewAllTutors()
        {
            dynamic tutors = new ExpandoObject();
            tutors.Students = Db.Students.Where(x =>x.TutorID!=null).ToList();
            UpdateAllTutors(tutors.Students);
            return View(tutors);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllTutors(string tutorSearch)
        {
            ViewData["getTutor"] = tutorSearch;
            //var tutorQuery = from x in Db.Students select x;
            var tutorQuery = await Db.Students.Where(x => x.FirstName.Contains(tutorSearch) || x.LastName.Contains(tutorSearch)
                || x.StudNum.Contains(tutorSearch) || x.YearOfStudy.Contains(tutorSearch)).ToListAsync();

            List<Student> tutors = new();
            if (!string.IsNullOrEmpty(tutorSearch))
            {

                if(tutorQuery.Count()==0 || tutorQuery == null)
                {
                    tutorQuery = Db.Students.ToList();

                    foreach (var item in tutorQuery)
                    {
                        Module module = Db.Module.Where(x => x.ModuleCode.Contains(tutorSearch) || x.ModuleName.Contains(tutorSearch)).FirstOrDefault();

                        List<TutorModule> modules = Db.TutorModules.ToList();
                        if (module != null   && modules!=null)
                        {
                            foreach (var current in modules)
                            {
                                if (current.ModuleCode.Equals(module.ModuleCode) && current.TutorID.Equals(item.TutorID))
                                {
                                    if (!tutors.Contains(item))
                                        tutors.Add(item);
                                }
                            }
                        }
                        else
                        {
                            ViewData["StatusMessage"] = "No Tutors Found, Try another search Tutor or Module";
                        }                         
                        
                    }
                }
                else
                {
                    tutors = tutorQuery;
                }
            }
            else
            {
                tutors = await Db.Students.ToListAsync();
            }    

            UpdateAllTutors(tutors);
            return View(tutors);
        }

        private void UpdateAllTutors(List<Student> tutors)
        {
            // fetch Tutor Modules 
            foreach(Student student in tutors)
            {
                List<TutorModule> modules = Db.TutorModules.Where(x => x.TutorID == student.TutorID).ToList();
                student.TutorModules = modules;
                //fecth Modules
                foreach (TutorModule module in student.TutorModules)
                {
                    module.Module = Db.Module.Where(x=>x.ModuleCode == module.ModuleCode).SingleOrDefault();
                }
            }
            
           
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<ActionResult> Booking(ParentModel parent) {
            try
            {
                parent.SingleBooking.TimeSlot = Db.TimeSlots.Where(x => x.TimeSlotID == parent.SingleBooking.TimeSlotId).SingleOrDefault();
                
                parent.SingleBooking.TutorModule = Db.TutorModules.Where(x => x.TutorModuleID == parent.SingleBooking.TutorModuleID).SingleOrDefault();
                
                parent.SingleBooking.Stud = Db.Students.Where(x => x.StudNum == parent.SingleBooking.StudNum).SingleOrDefault();
               
                parent.SingleBooking.TutorModule.Module = Db.Module.Where(x => x.ModuleCode == parent.SingleBooking.TutorModule.ModuleCode).SingleOrDefault();
                               
                parent.SingleBooking.SessionTime = getSessionDateTime(parent.SingleBooking.TimeSlot);

                Db.Bookings.Add(parent.SingleBooking);

               await Db.SaveChangesAsync();

                Notify("Booked successfully", "Booking");

                sendEmails(parent.Tutor);

                var callUrl = "";
                if (parent.Student.TutorID == null)
                {
                    callUrl = Url.Page(
                         "/Account/Manage/ViewBookings",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);
                }
                else
                {
                    callUrl = Url.Page(
                         "/Account/Manage/ViewStudentBookings",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);
                }

               
                return Redirect(callUrl);
            }
            catch (Exception e)
            {
                Notify("Booking Not successful", "Booking", NotificationType.error);

                ErrorViewModel error = new ErrorViewModel()
                {
                    Message = e.Message
                };
                return View("Error",error);
            }            
        }

        public async void sendEmails(Student tutor)
        {
            var user = await userManager.GetUserAsync(User);
            string Tutormessage = $"{user.ProfileName} booked you for a session, please check your Booking requests to confirm";
            Profile tutorProfile = Db.Users.Where(x => x.StudNum.Equals(tutor.StudNum)).FirstOrDefault();
           await _mailService.SendEmailAsync(tutorProfile.Email,"Booking",Tutormessage);

            string studentMessage = "Booking successful, please await for the Tutor to Confirm";
            await _mailService.SendEmailAsync(user.Email,"Booking",studentMessage);
        }
        
        public void Notify(string message,string title="Sweet Toast", NotificationType notification = NotificationType.success)
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

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { Message = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        [Authorize]
        public IActionResult payFile()
        {
            return View();

        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> payFile(PayInputModel pay)
        {
            try
            {
                //find the file in Question
                TutorFile file = Db.Files.Find(pay.bookingId);

                var user = await userManager.GetUserAsync(User);
                if(file == null)
                {
                    throw new ArgumentNullException();
                }

                FilePayment payment = new()
                {
                    TutorID =file.TutorID,
                    StudNum = user.StudNum,
                    FileID = file.FileID
                };
                Db.FilePayments.Add(payment);

               await Db.SaveChangesAsync();

                return DownloadFile(file.TutorID).Result;
            }
            catch
            {
                return View("PopupError",new ErrorViewModel() { Message="An Error Occurred, Please Try again"});
            }
            
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadFile(string Id)
        {
            try
            {
                List<TutorFile> files = new();

                files = await Db.Files.Where(x => x.TutorID.Equals(Id)).ToListAsync();

                return View(files);
            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Not Found" });
            }
        }

        public IActionResult DownloadFile()
        {
            //Fetch all files in the Folder (Directory).
            string[] filePaths = Directory.GetFiles(Path.Combine(this._hostEnvironment.WebRootPath, "TutorFiles/"));

            //Copy File names to Model collection.
            List<TutorFile> files = new();
            foreach (string filePath in filePaths)
            {
                files.Add(new TutorFile { FileName = Path.GetFileName(filePath) });
            }

            return View(files);
        }

        [HttpPost]
        [Authorize]
        public IActionResult DownloadingFile(int Id)
        {
            try
            {
                TutorFile file = Db.Files.Find(Id);
                if (file == null)
                {
                    throw new ArgumentException();
                }
                string path = Path.Combine(this._hostEnvironment.WebRootPath, "TutorFiles/") + file.FileUploaded;

                //Read the File data into Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                
                return new FileContentResult(bytes,GetContentType(file));

            }
            catch (Exception e)
            {
                return View("PopupError", new ErrorViewModel() { Message = "File Not Found "+e.Message });
            }
        }
        private string GetContentType(TutorFile file)
        {
            var types = GetMimeTypes();
            return types[file.FileType];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {"txt", "text/plain"},
                {"pdf", "application/pdf"},
                {"doc", "application/vnd.ms-word"},
                {"docx", "application/vnd.ms-word"},
                {"xls", "application/vnd.ms-excel"},
                {"xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {"png", "image/png"},
                {"jpg", "image/jpeg"},
                {"jpeg", "image/jpeg"},
                {"csv", "text/csv"}
            };
        }

        [HttpGet]
        public IActionResult DeleteFile(int Id)
        {
            try
            {
                TutorFile file = Db.Files.Find(Id);

                if (file == null)
                {
                    throw new Exception(message: "Not Found, Error 404");
                }

                return View(file);

            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Record not found, {404}"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int Id, TutorFile file)
        {
            try
            {
                TutorFile file1 = Db.Files.Find(Id);
                if (file1 != null)
                {
                    Db.Files.Remove(file1);
                }
                else
                {
                    Db.Files.Remove(file);
                }
                await Db.SaveChangesAsync();

                return Json(new { isValid = true });

            }
            catch
            {
                return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "DeleteFile", file) });
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult DeleteTime(int Id)
        {
            try
            {
                TimeSlot slot = Db.TimeSlots.Find(Id);

                if (slot == null)
                {
                    throw new Exception(message: "Not Found, Error 404");
                }

                return View(slot);

            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Not Found, Error 404" });
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteTime(int Id, TimeSlot slot)
        {
            try
            {
                TimeSlot time = Db.TimeSlots.Find(Id);
                if (time != null)
                {
                    Db.TimeSlots.Remove(time);
                }
                else
                {
                    Db.TimeSlots.Remove(slot);
                }
                await Db.SaveChangesAsync();

                return Json(new { isValid = true });

            }
            catch
            {
                return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "DeleteTime", slot) });
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult AddTimeSlot(string Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Timeslot not Found");
                }
                Student student = Db.Students.Where(x=>x.StudNum==Id).FirstOrDefault();

                SlotInputModel slot = new()
                {
                    TutorID = student.TutorID
                };
                return View(slot);
            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Record Not Found, {404}"});
            }
            
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTimeSlot(SlotInputModel slot)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    TimeSlot timeSlot = new TimeSlot()
                    {
                        SessionStartTime = slot.SessionStartTime,
                        SessionEndTime = slot.SessionEndTime,
                        SessionDay = slot.SessionDay,
                        TutorID =slot.TutorID
                    };

                    Db.TimeSlots.Add(timeSlot);
                    await Db.SaveChangesAsync();

                    return Json(new { isValid = true });
                }
                catch
                {
                    return View("PopupError", new ErrorViewModel() { Message = "Record was not saved, please try again"});
                }
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddTimeSlot", slot) });
        }

        public IActionResult PopupError()
        {

            return View();
        }

    }
}
