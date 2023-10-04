using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TutorBuddy.CustomValidation;

namespace TutorBuddy.Models
{
    public class TimeSlot
    {
        [Key]
        public int TimeSlotID { get; set; }

        [Required]
        [Display(Name = "Session Day")]
        public string SessionDay { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        [TimeValidation("SessionEndTime")]
        public DateTime SessionStartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString = "{0:H:mm}")]
        public DateTime SessionEndTime { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }
    }
}
