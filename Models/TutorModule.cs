using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models
{
    public class TutorModule
    {
        [Key]
        [Required]
        public int TutorModuleID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string TutorID{get;set;}
        
        [Required]
        public string ModuleCode { get; set; }
        [ForeignKey("ModuleCode")]
        public virtual Module Module { get; set; }
    }
}
