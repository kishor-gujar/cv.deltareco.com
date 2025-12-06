using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using cv.deltareco.com.Models;
using Microsoft.AspNetCore.Authorization;

namespace cv.deltareco.com.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]  // only login users allowed
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

    }
}
