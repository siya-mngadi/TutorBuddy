using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TutorBuddy.Models;

namespace TutorBuddy.Areas.Identity.Data
{
    public class TutorBuddyDbContext:IdentityDbContext<Profile>
    {
        public TutorBuddyDbContext(DbContextOptions<TutorBuddyDbContext> options)
           : base(options)
        {
  
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<TutorFile> Files { get; set; }
        public DbSet<FilePayment> FilePayments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SingleBooking> Bookings { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<TutorModule> TutorModules { get; set; }
        public DbSet<Student> Students { get; set; }
    }
}
