using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.CustomValidation
{
    public class EmailValidation:ValidationAttribute
    {
        private readonly string hostName = "mandela.ac.za";
        public EmailValidation(string errorMessage):base(errorMessage)
        {

        }
        public override bool IsValid(object value)
        {
            string host =(string) value;
            string[] email = host.Split('@');

            if (email.Contains(hostName)) 
                return true;

            return false;
        }
    }
}
