using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TutorBuddy.CustomValidation
{
    public class TimeValidation : CompareAttribute
    {
        public TimeValidation(string OtherProperty):base(OtherProperty)
        {
          
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                string strValue = value.ToString();
                var startTime = DateTime.Parse(strValue);
                var property = validationContext.ObjectType.GetProperty(OtherProperty);
                if (property == null)
                    return new ValidationResult(string.Format("Property '{0}' is Null", OtherProperty));
                var objEndTime = property.GetValue(validationContext.ObjectInstance, null);
                string strEndTime = objEndTime == null ? "" : objEndTime.ToString();

                var EndTime = DateTime.Parse(strEndTime);
                if (startTime.TimeOfDay > EndTime.TimeOfDay)
                    return new ValidationResult("start time can not be greater than end time");
                    
                return ValidationResult.Success;
            }
            catch (Exception)
            {
                return new ValidationResult("Invalid date format");
            }
        }
    }
}
