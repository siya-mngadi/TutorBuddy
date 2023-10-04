using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TutorBuddy.CustomValidation;

namespace TutorBuddy.Models.InputModel
{
    public class SlotInputModel
    {
        [Required]
        [Display(Name = "Session Day")]
        public string SessionDay { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        [TimeValidation("SessionEndTime")]
        public DateTime SessionStartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        public DateTime SessionEndTime { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }

        public string[] Weekdays = Enum.GetNames(typeof(DayOfWeek));
    }
}
