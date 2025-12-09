using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cv.deltareco.com.Areas.Admin.ViewModels
{

    public class EditUserViewModel
    {

        public string Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can contain only letters and spaces")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must be 10-15 digits")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{6,}$",
            ErrorMessage = "Password must be at least 6 characters, include letters, numbers & special character")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
