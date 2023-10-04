using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TutorBuddy.Models
{
    public class FilePayment
    {
        [Key]
        public int PaymentID { get; set; }
        [Required]
        public int FileID { get; set; }
        [ForeignKey("FileID")]
        public virtual TutorFile File { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(9)")]
        public string StudNum{get;set;}
        [ForeignKey("StudNum")]
        public virtual Student Stud { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }
    }
}
