using System.ComponentModel.DataAnnotations;

namespace TaskR.Views.Auth
{
    public class RegisterVm
    {
        [Required(ErrorMessage = "Please enter a Name")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password \"{0}\" must have {2} characters", MinimumLength = 8)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please re-enter password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again!")]
        public string PasswordConfirm { get; set; }
    }
}


