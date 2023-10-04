using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models
{
    public class ParentModel
    {
        public Profile Profile { get; set; }
        public Student Student { get; set; }
        public Student Tutor { get; set; }
        public SingleBooking SingleBooking { get; set ;}
        public TutorModule TutorModule { get; set; }
        public List<TutorModule> ListModules { get; set; } = new List<TutorModule>();
        public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }

    
}
