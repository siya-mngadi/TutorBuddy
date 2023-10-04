using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;
using TutorBuddy.Models.Helper;
using TutorBuddy.Models.InputModel;
using TutorBuddy.Services;

namespace TutorBuddy.Controllers
{
    public class BookingController : Controller
    {
        private readonly TutorBuddyDbContext dbContext;
        private readonly UserManager<Profile> userManager;
        private readonly IMailService _mailService;
        public BookingController(TutorBuddyDbContext context,
            UserManager<Profile> userManager,
            IMailService mailService)
        {
            dbContext = context;
            this.userManager = userManager;
            _mailService = mailService;
        }
        // GET: BookingController
        public IActionResult Index()
        {
            List<Student> students = dbContext.Students.Where(x => x.TutorID != null).ToList();
            return View(students);
        }

        // GET: BookingController/Details/5
        [Authorize]
        [HttpGet]
        public IActionResult TutorView(string TutorId)
        {
            //Fetch our current tutor
            Student Tutor = dbContext.Students.Where(x => x.TutorID.Equals(TutorId)).FirstOrDefault();
            UpdateTutor(Tutor);
            Review review = new Review();
            review.TutorID = TutorId;
            ViewData["Review"] = review;
            return View(Tutor);
        }


        private void UpdateTutor(Student tutor)
        {
            // fetch Tutor Modules 
         
                List<TutorModule> modules = dbContext.TutorModules.Where(x => x.TutorID == tutor.TutorID).ToList();
                tutor.TutorModules = modules;
                //fecth Modules
                foreach (TutorModule module in tutor.TutorModules)
                {
                    module.Module = dbContext.Module.Where(x => x.ModuleCode == module.ModuleCode).SingleOrDefault();
                }
            //fetch Time Slots 
            tutor.TimeSlots = dbContext.TimeSlots.Where(x => x.TutorID == tutor.TutorID).ToList();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult SaveReview(string comment,string TutorId,int rating)
        {
            try
            {
                //get Student 
                Profile student = userManager.FindByNameAsync(User.Identity.Name).Result;

                Review review = new Review()
                {
                    TutorID = TutorId,
                    ReviewDescription = comment,
                    Rating = rating,
                    StudNum = student.StudNum
                };

                Student Tutor = dbContext.Students.Where(x => x.TutorID == TutorId).SingleOrDefault();

                updateTutorRating(Tutor,review);

                dbContext.Update(Tutor);

                dbContext.Add(review);

                dbContext.SaveChanges();               

                var call = Url.ActionLink("TutorView", "Booking", values: new { TutorId = TutorId });
                return Redirect(call);
            }catch (Exception e)
            {
                ErrorViewModel error = new ErrorViewModel()
                {
                    Message = e.Message
                };
                return View("Error",error);
            }
        }
        private void updateTutorRating(Student Tutor,Review review)
        {
            List<Review> reviews = new();

            reviews = dbContext.Reviews.Where(x => x.TutorID == Tutor.TutorID).ToList();

            reviews.Add(review);

            int sum = 0;
            foreach (var item in reviews)
            {
                sum += item.Rating;
            }

            
            if (reviews.Count >0)
            {
                sum = sum / reviews.Count;
            }
            Tutor.Rating = sum;

        }
        // GET: BookingController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookingController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookingController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetPartialData()
        {
            return PartialView("_ExamplePartial");
        }
        public IActionResult payFee(int Id)
        {
            try
            {
               
                SingleBooking booking = dbContext.Bookings.Where(x => x.BookingID == Id).FirstOrDefault();

                Student student = dbContext.Students.Where(x => x.StudNum == booking.StudNum).FirstOrDefault();


                PayInputModel model = new() { 
                     FirstName = student.FirstName,
                     LastName = student.LastName,
                     BookingFee = booking.BookingFee,
                     bookingId = Id
                };
               return View(model);
            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Booking Not Found" });
            }
        }

        [HttpPost]
        public IActionResult payFee(PayInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SingleBooking booking = dbContext.Bookings.Where(x => x.BookingID == model.bookingId).FirstOrDefault();
                    booking.IsPaid = true;
                    dbContext.Bookings.Update(booking);
                    dbContext.SaveChanges();

                    return Json(new { isValid = true });
                }
                catch 
                {
                    return View("PopupError", new ErrorViewModel() { Message = "Not Save, Something went wrong"});
                }


            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "payFee", model) });
        }

        [HttpGet]
        public IActionResult DeleteBooking(int Id)
        {
            try
            {
                SingleBooking slot = dbContext.Bookings.Find(Id);

                if (slot == null)
                {
                    throw new Exception(message: "Not Found, Error 404");
                }

                return View(slot);

            }
            catch
            {
                return View("PopupError", new ErrorViewModel() { Message = "Record not Found"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int Id, SingleBooking slot)
        {
            try
            {
                SingleBooking time = dbContext.Bookings.Find(Id);
                if (time != null)
                {
                    dbContext.Bookings.Remove(time);
                }
                else
                {
                    dbContext.Bookings.Remove(slot);
                }
                await dbContext.SaveChangesAsync();
                Profile student = new();

               
                if (time != null)
                {
                    student = dbContext.Users.Where(x => x.StudNum.Equals(time.StudNum)).FirstOrDefault();
                }
                else
                {
                    student = dbContext.Users.Where(x => x.StudNum.Equals(slot.StudNum)).FirstOrDefault();
                }

                await _mailService.SendEmailAsync(student.Email, "Booking", "Booking Rejected by the Tutor. Please try to book another tutor");

                return Json(new { isValid = true });

            }
            catch
            {
                return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "DeleteTime", slot) });
            }
        }

        public async Task<IActionResult> AcceptBooking(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);

                if (user == null)
                {
                    throw new KeyNotFoundException();
                }

                SingleBooking booking = dbContext.Bookings.Where(x => x.BookingID == Id).FirstOrDefault();

                booking.IsBooked = true;

                dbContext.Bookings.Update(booking);

                dbContext.SaveChanges();

               Profile student = dbContext.Users.Where(x=>x.StudNum.Equals(booking.StudNum)).FirstOrDefault();

             await  _mailService.SendEmailAsync(student.Email, "Booking", "Booking Accepted by the Tutor. Please be on time for your tutorial");
               
                string callUrl = "~/";
                if (user.IsTutor)
                {
                     callUrl = Url.Page(
                         "/Account/Manage/ViewStudentBookings",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);

                    return Redirect(callUrl);
                }
                else
                {
                    callUrl = Url.Page(
                          "/Account/Manage/ViewBookings",
                          pageHandler: null,
                          values: new { area = "Identity", returnUrl = "~/" },
                          protocol: Request.Scheme);

                    
                }
                return Redirect(callUrl);
            }
            catch (Exception e)
            {
                return View("Error", new ErrorViewModel() { Message = e.Message });
            }
        }
       /* public async Task<IActionResult> DeleteBooking(int Id)
        {
            try
            {
                SingleBooking booking = dbContext.Bookings.Where(x => x.BookingID == Id).FirstOrDefault();

                var user = await userManager.GetUserAsync(User);

                dbContext.Bookings.Remove(booking);
                dbContext.SaveChanges();

                if (user == null)
                {
                    throw new KeyNotFoundException();
                }

                if (user.IsTutor)
                {
                    var callUrl = Url.Page(
                         "/Account/Manage/ViewStudentBookings",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);

                    return Redirect(callUrl);
                }
                else
                {
                   var callUrl = Url.Page(
                         "/Account/Manage/ViewBookings",
                         pageHandler: null,
                         values: new { area = "Identity", returnUrl = "~/" },
                         protocol: Request.Scheme);

                    return Redirect(callUrl);
                }
                
               
            }
            catch (Exception e)
            {
                return View("Error",new ErrorViewModel() { Message=e.Message});
            }
        }*/

        
    }
}
