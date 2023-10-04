using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;

namespace TutorBuddy.ViewComponents
{
    [ViewComponent]
    public class ReviewViewComponent:ViewComponent
    {
        private readonly TutorBuddyDbContext db;
        public ReviewViewComponent(TutorBuddyDbContext db)
        {
            this.db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync(string TutorId)
        {
            var reviews = await GetItemsAsync(TutorId);
            return View(reviews);
        }
        private Task<List<Review>> GetItemsAsync(string TutorId)
        {
            var reviews = db.Reviews.Where(x => x.TutorID.Equals(TutorId)).ToListAsync();
            foreach (Review review in reviews.Result)
            {
                review.Stud = db.Students.Where(x => x.StudNum.Equals(review.StudNum)).SingleOrDefault();
            }
            return reviews;
        }
    }
}
