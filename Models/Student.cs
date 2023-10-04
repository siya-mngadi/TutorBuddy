using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Formats;
using TutorBuddy.CustomValidation;

namespace TutorBuddy.Models
{
    public class Student
    {
        [Key]
        [Required]
        [Column(TypeName = "nvarchar(9)")]
        [StringLength(9,MinimumLength = 9,ErrorMessage ="Please Enter your Student Number 0 to 9 characters")]
        [Display(Name ="Student Number")]
        public string StudNum { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        [StringLength(15, MinimumLength = 9)]
        public string TutorID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Year Of Study")]
        public string YearOfStudy { get; set; }

        public string UserProfile { get; set; }

        [Display(Name = "Profile Photo")]
        [NotMapped]
        public IFormFile DisplayPhoto { get; set; }

        [Display(Name ="Register as a Paid Tutor?")]
        public bool TypeOfTutor { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Hourly Fee")]
        public double HourlyFee { get; set; } = 0;

        [Range(0, 250)]
        [Display(Name = "File Fee")]
        public double FileFee { get; set; } = 0;

        [Range(0,5,ErrorMessage ="Rating must be between 1 and 5")]
        public int Rating { get; set; } = 0;
        
        public List<Review> Reviews { get; set; }
 
        public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

        public List<TutorModule> TutorModules { get; set; } = new List<TutorModule>();

        public List<SingleBooking> Bookings { get; set; }

        [NotMapped]
        public List<string> ModuleList { get; set; } = new ();
    }
}
