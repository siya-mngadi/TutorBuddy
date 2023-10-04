using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;

namespace TutorBuddy.Services.Reminder
{
    public class Reminder : BackgroundService
    {
        private readonly ILogger<Reminder> logger;

        private readonly TutorBuddyDbContext context;

        private readonly IMailService mailService;

        ConcurrentDictionary<string, List<string>> profiles = new();

        public Reminder(ILogger<Reminder> logger, IServiceProvider serviceProvider, IMailService mailService)
        {
            this.logger = logger;

            this.context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<TutorBuddyDbContext>();

            this.mailService = mailService;
        }        

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SendingReminders(stoppingToken);
        }

        private bool GetListToRemind()
        {
            try
            {

                List<SingleBooking> bookings = context.Bookings.ToList();


                if (bookings == null)
                {
                    return false;
                }

                foreach (var item in bookings)
                {
                    int day = item.SessionTime.Day;
                    int month = item.SessionTime.Month;
                    int year = item.SessionTime.Year;
                    TimeSlot timeSlot = context.TimeSlots.Find(item.TimeSlotId);

                    Profile student = context.Users.Where(x => x.StudNum == item.StudNum).FirstOrDefault();

                    Profile Tutor = context.Users.Where(x => x.TutorID == timeSlot.TutorID).FirstOrDefault();

                    int hour = Math.Abs(DateTime.Now.Hour - timeSlot.SessionStartTime.Hour);

                    bool isBeforeFiveMinutes = month == DateTime.Today.Month &&
                       year == DateTime.Today.Year &&
                       day == DateTime.Today.Day &&
                       hour == 2;

                    if (Tutor == null || student == null)
                        continue;

                    if (isBeforeFiveMinutes)
                    {
                        if (profiles.ContainsKey(Tutor.Email))
                        {
                            profiles[Tutor.Email].Add(student.Email);
                        }
                        else
                        {
                            profiles.TryAdd(Tutor.Email, new List<string>());
                            profiles[Tutor.Email].Add(student.Email);
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }


        }

        public async Task SendingReminders(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool isValid = GetListToRemind();
                if (isValid)
                {
                   ICollection<string> keys = profiles.Keys;

                    logger.LogInformation("Attempting to send emails");

                    foreach (var item in keys)
                    {
                        List<string> students = profiles[item];
                        if (students.Count > 0)
                        {
                          await sendTutorReminder(item);
                        }
                        await sendStudentReminder(students);
                    }
                }
                await Task.Delay(3600000, cancellationToken);
            }
        }

        private async Task sendTutorReminder(string email)
        {
            string subject = "Reminder";

            string Message = "Tutor\n you have students that you need to tutor in the next coming hours";

            await mailService.SendEmailAsync(email, subject, Message);
        }

        private async Task sendStudentReminder(List<string> students)
        {
            foreach (var item in students)
            {
                string subject = "Reminder";

                string Message = "You have a Tutorial in the next Few hours, Don't forget";


                await mailService.SendEmailAsync(item, subject, Message);
            }
        }

    }
}
