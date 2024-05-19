using System.ComponentModel.DataAnnotations;

namespace TaskR.Helper
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            const string defaultErrorMessage = "Invalid Value";
            ErrorMessage ??= defaultErrorMessage;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
                //return new ValidationResult("Required");
            }
            if (!DateTime.TryParse(value.ToString(), out DateTime date))
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName));
            }
            if (date < DateTime.Now)
            {
                return new ValidationResult("Date must be in the future");
            }
            return ValidationResult.Success;
        }
    }
}
