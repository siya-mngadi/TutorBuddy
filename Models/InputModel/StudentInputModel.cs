using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models.InputModel
{
    public class StudentInputModel
    {
        public Student Student { get; set; } = new();

    }
}
