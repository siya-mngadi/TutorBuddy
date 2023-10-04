using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        [Display(Name = "Review")]
        [StringLength(150,ErrorMessage ="You have Exceed your word limit")]
        public string ReviewDescription { get; set; }
        [Required]
        [Display(Name ="Rate")]
        [Range(1,5,ErrorMessage ="Must be between 1 and 5 Please")]
        public int Rating { get; set; } = 0; 
        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(9)")]
        public string StudNum { get; set; }
        [ForeignKey("StudNum")]
        public Student Stud { get; set; }
    }
}
