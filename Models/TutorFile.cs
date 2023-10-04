using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TutorBuddy.Models
{
    public class TutorFile
    {
        [Key]
        public int FileID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(25)")]
        [Display(Name ="File Type")]
        public string FileType { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        [Display(Name = "File Name")]
        public string FileName { get; set; }
        [Required]
        [Display(Name = "File Description")]
        public string FileDescription { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string TutorID { get; set; }

        public string FileUploaded { get; set; }

        [Display(Name = "File Upload")]
        [NotMapped]
        public IFormFile DisplayFile { get; set; }
    }
}
