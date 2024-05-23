using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TaskR.Helper
{
    public class HexColorAttribute : ValidationAttribute
    {
        public HexColorAttribute()
        {
            const string defaultErrorMessage = "Invalid Value";
            ErrorMessage ??= defaultErrorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string)
            {
                var hex = (string)value;
                if (hex == "#000000")
                    return new ValidationResult("Black Tags are not allowed");
                var regex = Regex.Match(hex, @"^#(?:[0-9a-fA-F]{3}){1,2}$");
                if (regex.Success)
                    return ValidationResult.Success;
                else
                    return new ValidationResult("invalid");
            }
            return new ValidationResult("invalid");
        }
    }
}
