using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models
{
    public class Profile:IdentityUser
    {
        [Column(TypeName ="nvarchar(100)")]
        [DataType(DataType.Text)]
        public string ProfileName { get; set; }

        [DefaultValue(false)]
        [Required]
        [Display(Name = "Register as Tutor? ")]
        public bool IsTutor { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Column(TypeName = "nvarchar(9)")]
        [Display(Name = "Student Number")]
        public string StudNum { get; set; }

        [ForeignKey("StudNum")]
        public Student Student { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }
    }
}
