using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;

namespace TutorBuddy.Areas.Identity.Pages.Account.Manage
{
    public class ViewBookingsModel : PageModel
    {

        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly TutorBuddyDbContext _dbContext;
        public ViewBookingsModel(
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

            public string TutorName { get; set; }

            public string SessionDay { get; set; }

            public string sessionStartTime { get; set; }

            public string sessionEndTime { get; set; }

            public double bookingFee { get; set; }

            public bool isBooked { get; set; }

            public bool isPaidTutor { get; set; }

            public bool bookingPaid { get; set; }
        }



        private async Task LoadAsync(Profile user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Student student = _dbContext.Students.Where(x => x.StudNum == user.StudNum).FirstOrDefault();

                List<SingleBooking> bookings = _dbContext.Bookings.Where(x => x.StudNum== student.StudNum).ToList();

            foreach (var booking in bookings)
            {
                TutorModule tutorModule =_dbContext.TutorModules.Where(x => x.TutorModuleID == booking.TutorModuleID).FirstOrDefault();

                Student Tutor = _dbContext.Students.Where(x => x.TutorID==tutorModule.TutorID).FirstOrDefault();

                TimeSlot time = _dbContext.TimeSlots.Where(x => x.TimeSlotID == booking.TimeSlotId).FirstOrDefault();

                Module module = _dbContext.Module.Where(x => x.ModuleCode == tutorModule.ModuleCode).FirstOrDefault();

                string bookedTime = booking.SessionTime.Day.ToString() + "/" + booking.SessionTime.Month.ToString() + "/" + booking.SessionTime.Year.ToString();

                ReadModel model = new ReadModel()
                {
                    bookingId = booking.BookingID,

                    ModuleName = module.ModuleName,

                    TutorName = Tutor.FirstName + " " + Tutor.LastName,

                    SessionDay = bookedTime,

                    sessionStartTime = DateTime.Today.Add(time.SessionStartTime.TimeOfDay).ToString("hh':'mm tt"),

                    sessionEndTime = DateTime.Today.Add(time.SessionEndTime.TimeOfDay).ToString("hh':'mm tt"),

                    bookingFee = booking.BookingFee,

                    isBooked = booking.IsBooked,

                    isPaidTutor = Tutor.TypeOfTutor,

                    bookingPaid = booking.IsPaid
                };
                Bookings.Add(model);
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
            catch (Exception e)
            {
                return NotFound("Something went wrong, " + e.Message);
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
