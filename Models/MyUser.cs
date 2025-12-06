using System.ComponentModel.DataAnnotations;

namespace cv.deltareco.com.Models
{
    public class MyUser
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }
        public string LastNaem { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }  // Hashed store recommended
    }
}
