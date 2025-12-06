using Microsoft.AspNetCore.Identity;

namespace cv.deltareco.com.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        // Add other custom fields if required
    }
}
