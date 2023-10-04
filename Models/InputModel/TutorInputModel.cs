using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models.InputModel
{
    public class TutorInputModel
    {
        public Student Student { get; set; } = new();

        public string[] Weekdays = Enum.GetNames(typeof(DayOfWeek));

        public List<Module> Modules = new();
    }
}
