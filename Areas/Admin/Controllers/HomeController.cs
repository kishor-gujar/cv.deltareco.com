using cv.deltareco.com.Data;
using cv.deltareco.com.Models;
using cv.deltareco.com.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cv.deltareco.com.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;

            var todaysCVCount = await _context.CandidateCVs
                .Where(x => x.UploadedOn >= today)
                .CountAsync();

            if (user == null)
                return RedirectToAction("Login", "Account");
            var candidatesprofile =await _context.CandidateProfiles
                            .Include(x => x.CandidateCV)
                            .OrderByDescending(x => x.Id)
                            .Take(10) // last 10 entries only
                            .ToListAsync();
           
            var cvs = await _context.CandidateCVs.OrderBy(x => x.UploadedOn).Take(10).ToListAsync();
            var vm = new HomeViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                // COUNTERS
                TotalUsers = await _context.Users.CountAsync(),
                TotalCandidateCVCounts = await _context.CandidateCVs.CountAsync(),
                TotalCandidateCounts = await _context.CandidateProfiles.CountAsync(),
                Profiles = candidatesprofile,
                CandidateCVs = cvs,
                RecentUploadedCVCounts =todaysCVCount,

            };
            // Count per month for Profiles
            vm.ProfileCounts = Enumerable.Range(1, 12)
                .Select(m => _context.CandidateProfiles.Count(x => x.CreatedOn.Month == m))
                .ToList();

            // Count per month for CVs
            vm.CVCounts = Enumerable.Range(1, 12)
                .Select(m => _context.CandidateCVs.Count(x => x.UploadedOn.Month == m))
                .ToList();

            return View(vm);
        }
    }

}
