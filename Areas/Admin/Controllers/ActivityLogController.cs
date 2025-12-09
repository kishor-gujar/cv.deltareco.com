using cv.deltareco.com.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cv.deltareco.com.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize]  // only login users allowed
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ActivityLog
        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(x => x.PerformedAt)
                .ToListAsync();

            return View(logs);
        }

        // GET: /ActivityLog/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var log = await _context.ActivityLogs.FindAsync(id);
            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}
