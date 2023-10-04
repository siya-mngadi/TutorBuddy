using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models.InputModel
{
    public class PayInputModel
    {
        [Required]
        [Display(Name ="Card Number")]
        [StringLength(16, MinimumLength = 16,ErrorMessage ="Card number must be 16 digits, please try again")]
        public string CardNumber { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
       
        [Display(Name = "Amount")]
        public double BookingFee { get; set; }
        [Required]
        [Range(1, 12,ErrorMessage ="Month are between 1 and 12, please input correct number")]
        [Display(Name = "Month")]
        public int Month { get; set; }
        [Required]
        [Range( 2021, 2030, ErrorMessage = "Please enter the correct Year")]
        [Display(Name = "Year")]
        public int Year { get; set; }
        [Required]
        [Range(100, 999, ErrorMessage = "CCV has 3 digits, please input correct CCV")]
        [Display(Name = "CCV")]
        public int CCV { get; set; }
        [Required]
        public int bookingId { get; set; }

    }
}
