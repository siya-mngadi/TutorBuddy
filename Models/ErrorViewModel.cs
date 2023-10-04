using System;

namespace TutorBuddy.Models
{
    public class ErrorViewModel
    {
        public string Message { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(Message);

    }
}
