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
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Get the earliest CreatedOn / UploadedOn dates for defaults
            var firstProfileDate = await _context.CandidateProfiles.MinAsync(x => (DateTime?)x.CreatedOn) ?? DateTime.Today;
            var firstCVDate = await _context.CandidateCVs.MinAsync(x => (DateTime?)x.UploadedOn) ?? DateTime.Today;
            var firstActivityDate = await _context.ActivityLogs.MinAsync(x => (DateTime?)x.PerformedAt) ?? DateTime.Today;

            // Use the earliest date among all tables for "from" if no date selected
            var earliestDate = new[] { firstProfileDate, firstCVDate, firstActivityDate }.Min();

            // Set default dates if not provided
            var from = fromDate ?? earliestDate;
            var to = toDate?.AddDays(1).AddTicks(-1) ?? DateTime.Today.AddDays(1).AddTicks(-1);

            // Counters
            var totalUsers = await _context.Users.CountAsync();
            var totalCandidateCVCounts = await _context.CandidateCVs
                                            .Where(x => x.UploadedOn >= from && x.UploadedOn <= to)
                                            .CountAsync();
            var totalCandidateCounts = await _context.CandidateProfiles
                                            .Where(x => x.CreatedOn >= from && x.CreatedOn <= to)
                                            .CountAsync();
            var recentUploadedCVCounts = await _context.CandidateCVs
                                            .Where(x => x.UploadedOn >= from && x.UploadedOn <= to)
                                            .CountAsync();

            // Latest 10 Candidates
            var candidatesprofile = await _context.CandidateProfiles
                                        .Include(x => x.CandidateCV)
                                        .Where(x => x.CreatedOn >= from && x.CreatedOn <= to)
                                        .OrderByDescending(x => x.Id)
                                        .Take(10)
                                        .ToListAsync();

            // Latest 10 CVs
            var cvs = await _context.CandidateCVs
                            .Where(x => x.UploadedOn >= from && x.UploadedOn <= to)
                            .OrderByDescending(x => x.UploadedOn)
                            .Take(10)
                            .ToListAsync();

            // Latest 10 Activity Logs
            var activity = await _context.ActivityLogs
                            .Where(x => x.PerformedAt >= from && x.PerformedAt <= to)
                            .OrderByDescending(x => x.PerformedAt)
                            .Take(10)
                            .ToListAsync();

            // Month-wise counts (filtered)
            var profileCounts = Enumerable.Range(1, 12)
                                .Select(m => _context.CandidateProfiles
                                    .Count(x => x.CreatedOn.Month == m && x.CreatedOn >= from && x.CreatedOn <= to))
                                .ToList();

            var cvCounts = Enumerable.Range(1, 12)
                            .Select(m => _context.CandidateCVs
                                .Count(x => x.UploadedOn.Month == m && x.UploadedOn >= from && x.UploadedOn <= to))
                            .ToList();

            // Prepare ViewModel
            var vm = new HomeViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                TotalUsers = totalUsers,
                TotalCandidateCVCounts = totalCandidateCVCounts,
                TotalCandidateCounts = totalCandidateCounts,
                RecentUploadedCVCounts = recentUploadedCVCounts,
                Profiles = candidatesprofile,
                CandidateCVs = cvs,
                ActivityLogs = activity,
                ProfileCounts = profileCounts,
                CVCounts = cvCounts,
                FromDate = from,
                ToDate = to
            };

            return View(vm);
        }

    }

}
