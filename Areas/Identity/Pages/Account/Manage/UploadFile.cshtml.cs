using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;
using TutorBuddy.Models.Utilities;

namespace TutorBuddy.Areas.Identity.Pages.Account.Manage
{
    public class UploadFileModel : PageModel
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly TutorBuddyDbContext _dbContext;
        public UploadFileModel(
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            TutorBuddyDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _hostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public List<ReadModel> Files { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string TutorId { get; set; }

            [Required]
            [Display(Name = "File Name")]
            public string FileName { get; set; }
            [Display(Name = "File Description")]
            public string FileDescription { get; set; }

            public string FileUploaded { get; set; }

            [Display(Name = "File Upload")]
            public IFormFile DisplayFile { get; set; }

            public string FileType { get; set; }
        }

        public class ReadModel
        {
            public int FileID { get; set; }
            public string TutorId { get; set; }
            public string FileName { get; set; }

            public string FileDescription { get; set; }

            public string FileType { get; set; }

        }

        private void LoadAsync(Profile user)
        {

            Student student = _dbContext.Students.Where(x => x.StudNum == user.StudNum).FirstOrDefault();
            Input = new InputModel
            {
                TutorId = student.TutorID
            };

            List<TutorFile> files = _dbContext.Files.Where(x => x.TutorID == student.TutorID).ToList();

            foreach (var file in files)
            {
                ReadModel model = new ReadModel()
                {
                    FileID = file.FileID,
                    FileName = file.FileName,
                    FileDescription = file.FileDescription,
                    FileType = file.FileType,
                };
                Files.Add(model);

            }

               
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

               LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                 LoadAsync(user);
                return Page();
            }

            try
            {
                TutorFile file = new();

                if (ModelState.IsValid)
                {
                    file.FileType = getFileType(Input.DisplayFile);
                    file.TutorID = user.TutorID;
                    file.FileName = Input.FileName;
                    file.FileDescription = Input.FileDescription;
                    file.DisplayFile = Input.DisplayFile;
                    file.FileUploaded = UploadedFile(file);
                    _dbContext.Files.Add(file);
                    await _dbContext.SaveChangesAsync();
                }
                
            }
            catch
            {
                StatusMessage = "Your file upload failed, please try again. upload file with correct type";
                return RedirectToPage();
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your file has been uploaded";
            return RedirectToPage();
        }
        public string getFileType(IFormFile file)
        {
            string doc = file.FileName.ToLower().Trim();
            string[] types = Enum.GetNames(typeof(FileType));
            foreach (var item in types)
            {
                if (doc.Contains(item))
                {
                    return item;
                }
            }
            return null;
        }
        private string UploadedFile(TutorFile files)
        {
            string uniqueFileName = "";
            if (files.DisplayFile != null)
            {
                string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "TutorFiles");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + files.DisplayFile.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                using var fileStream = System.IO.File.Create(filePath);
                files.DisplayFile.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
    }
}
