using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;

namespace TutorBuddy.Areas.Identity.Pages.Account.Manage
{
    public class ViewStudentBookingsModel : PageModel
    {
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly TutorBuddyDbContext _dbContext;
        public ViewStudentBookingsModel(
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            TutorBuddyDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [BindProperty]
        public List<ReadModel> Bookings { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public class ReadModel
        {
            public int bookingId { get; set; }
            public string ModuleName { get; set; }

            public string StudentName { get; set; }

            public string SessionDay { get; set; }

            public string sessionStartTime { get; set; }

            public string sessionEndTime { get; set; }

            public double bookingFee { get; set; }

            public bool isBooked { get; set; }
        }

        private async Task LoadAsync(Profile user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Student tutor = _dbContext.Students.Where(x => x.TutorID == user.TutorID).FirstOrDefault();

            List<TutorModule> tutorModules = _dbContext.TutorModules.Where(x => x.TutorID == tutor.TutorID).ToList();


            foreach (var item in tutorModules)
            {
                List<SingleBooking> bookings = _dbContext.Bookings.Where(x => x.TutorModuleID == item.TutorModuleID).ToList();
                if (bookings == null)
                {
                    continue;
                }
                foreach (var booking in bookings)
                {
                    Student student = _dbContext.Students.Where(x => x.StudNum == booking.StudNum).FirstOrDefault();

                    TimeSlot time = _dbContext.TimeSlots.Where(x => x.TimeSlotID== booking.TimeSlotId).FirstOrDefault();

                    Module module = _dbContext.Module.Where(x => x.ModuleCode == item.ModuleCode).FirstOrDefault();

                    string bookedTime = booking.SessionTime.Day.ToString() + "/" + booking.SessionTime.Month.ToString() + "/" + booking.SessionTime.Year.ToString();

                    ReadModel model = new ReadModel()
                    {
                        bookingId = booking.BookingID,

                        ModuleName = module.ModuleName,

                        StudentName = student.FirstName + " " + student.LastName,

                        SessionDay = bookedTime,

                        sessionStartTime = DateTime.Today.Add(time.SessionStartTime.TimeOfDay).ToString("hh':'mm tt"),

                        sessionEndTime = DateTime.Today.Add(time.SessionEndTime.TimeOfDay).ToString("hh':'mm tt"),

                       bookingFee = booking.BookingFee,

                       isBooked = booking.IsBooked
                    };
                    Bookings.Add(model);
                }
            } 
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            try
            {
                await LoadAsync(user);
            }
            catch  (Exception e)
            {
                return NotFound("Something went wrong, "+e.Message);
            }
           
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
        
    }
}
