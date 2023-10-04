using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TutorBuddy.Models
{
    public class SingleBooking
    {
        [Key]
        public int BookingID { get; set; }
        [Required]
        [Display(Name = "Booking Fee")]
        public double BookingFee { get; set; }
        [Required]
        public bool IsBooked { get; set; } = false;
        [Required]
        public bool IsPaid { get; set; } = false;

        [Required]
        public int TimeSlotId { get; set; }

        public DateTime SessionTime { get; set; }

        [ForeignKey("TimeSlotId")]
        public virtual TimeSlot TimeSlot { get; set; } = new TimeSlot();
        [Column(TypeName = "nvarchar(9)")]

        [Required]
        public string StudNum { get; set; }
        [ForeignKey("StudNum")]
        public virtual Student Stud { get; set; }

        public int TutorModuleID { get; set; }
        [ForeignKey("TutorModuleID")]
        public virtual  TutorModule TutorModule { get; set; }
    }
}
