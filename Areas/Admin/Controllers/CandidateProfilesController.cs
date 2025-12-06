using cv.deltareco.com.Data;
using cv.deltareco.com.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cv.deltareco.com.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CandidateProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CandidateProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== INDEX ===========================
        public async Task<IActionResult> Index()
        {
            var profiles = await _context.CandidateProfiles
                                         .Include(x => x.CandidateCV)
                                         .ToListAsync();

            return View(profiles);
        }

        // ===================== CREATE ==========================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CVs = _context.CandidateCVs.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(CandidateProfile model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CVs = _context.CandidateCVs.ToList();
                return View(model);
            }

            _context.CandidateProfiles.Add(model);
            _context.SaveChanges();

            TempData["success"] = "Candidate Profile added successfully!";
            return RedirectToAction("Index");
        }

        // ===================== EDIT ============================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var profile = _context.CandidateProfiles.FirstOrDefault(x => x.Id == id);
            if (profile == null) return NotFound();

            ViewBag.CVs = _context.CandidateCVs.ToList();
            return View(profile);
        }

        [HttpPost]
        public IActionResult Edit(CandidateProfile model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CVs = _context.CandidateCVs.ToList();
                return View(model);
            }

            _context.CandidateProfiles.Update(model);
            _context.SaveChanges();

            TempData["success"] = "Profile updated!";
            return RedirectToAction("Index");
        }

        // ===================== DETAILS =========================
        public IActionResult Details(int id)
        {
            var profile = _context.CandidateProfiles
                                   .Include(x => x.CandidateCV)
                                   .FirstOrDefault(x => x.Id == id);

            if (profile == null) return NotFound();

            return View(profile);
        }

        // ===================== DELETE ==========================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var profile = _context.CandidateProfiles
                                   .Include(x => x.CandidateCV)
                                   .FirstOrDefault(x => x.Id == id);

            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var profile = _context.CandidateProfiles.FirstOrDefault(x => x.Id == id);
            if (profile == null) return NotFound();

            _context.CandidateProfiles.Remove(profile);
            _context.SaveChanges();

            TempData["success"] = "Profile deleted!";
            return RedirectToAction("Index");
        }
    }
}
