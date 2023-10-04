using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models
{
    public class Module
    {
        [Key]
        [Required]
        public string ModuleCode { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        [Display(Name = "Module Name")]
        public string ModuleName { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(225)")]
        [Display(Name = "Module Description")]
        public string ModuleDescription { get; set; }
    }
}
