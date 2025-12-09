using cv.deltareco.com.Data;
using cv.deltareco.com.Models;
using cv.deltareco.com.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using UglyToad.PdfPig.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace cv.deltareco.com.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CandidateProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _log;

        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateProfilesController(ApplicationDbContext context, ActivityLogService log, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _log = log;
        }

        // ===================== INDEX ===========================
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
         
            var query = _context.CandidateProfiles
                      .Include(c => c.CandidateCV)
                      .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedOn >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedOn <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var profiles = await query.OrderByDescending(c => c.CreatedOn)
                                      .Include(x => x.CandidateCV)
                                      .ToListAsync();
            // Pass the filter dates back to ViewData for pre-filling inputs
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            return View(profiles);
        }

        // ===================== CREATE ==========================
        //[HttpGet]
        //public IActionResult Create()
        //{
        //    ViewBag.CVs = _context.CandidateCVs.ToList();
        //    return View();
        //}

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CandidateCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CandidateCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int cvId = 0;

            // --------------------------
            // 1️⃣ SAVE CV FIRST
            // --------------------------
            if (model.CVFile != null)
            {
                string folder = "uploads/cvs/";
                string fileName = Guid.NewGuid() + Path.GetExtension(model.CVFile.FileName);
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                string filePath = Path.Combine(fullPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CVFile.CopyToAsync(stream);
                }

                var uploadedCV = new CandidateCV
                {
                    FileName = model.CVFile.FileName,
                    FilePath = "/" + folder + fileName,
                    UploadedOn = DateTime.Now
                };

                _context.CandidateCVs.Add(uploadedCV);
                await _context.SaveChangesAsync();

                cvId = uploadedCV.Id; // GET SAVED CV ID
            }

            // --------------------------
            // 2️⃣ SAVE CandidateProfile
            // --------------------------
            var candidate = new CandidateProfile
            {
                CandidateName = model.CandidateName,
                Email = model.Email,
                Mobile = model.Mobile,
                Education = model.Education,
                Experience = model.Experience,
                Skills = model.Skills,
                CandidateCVId = cvId    // LINK CV ID HERE
            };

            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();
            await _log.LogCreateAsync(candidate);
        
            TempData["Success"] = "Candidate created successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult ExportCsv(DateTime? fromDate, DateTime? toDate)
        {
            var data = _context.CandidateProfiles
                .Include(x => x.CandidateCV)
                .AsQueryable();

            if (fromDate.HasValue)
                data = data.Where(x => x.CreatedOn.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                data = data.Where(x => x.CreatedOn.Date <= toDate.Value.Date);

            var list = data.ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Name,Email,Mobile,Education,Experience,Skills,CVFile");

            foreach (var c in list)
            {
                csv.AppendLine(
                    $"{c.CandidateName}," +
                    $"{c.Email}," +
                    $"{c.Mobile}," +
                    $"{c.Education}," +
                    $"{c.Experience}," +
                    $"{c.Skills}," +
                    $"{(c.CandidateCV != null ? c.CandidateCV.FileName : "No File")}"
                );
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", "Candidates.csv");
        }
        public IActionResult ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var data = _context.CandidateProfiles
                .Include(x => x.CandidateCV)
                .AsQueryable();

            if (fromDate.HasValue)
                data = data.Where(x => x.CreatedOn.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                data = data.Where(x => x.CreatedOn.Date <= toDate.Value.Date);

            var list = data.ToList();

            using (var workbook = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(workbook, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();

                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart
                                 .GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Candidates"
                    };
                    sheets.Append(sheet);

                    // Header Row
                    Row header = new Row();
                    header.Append(
                        CreateCell("Name"),
                        CreateCell("Email"),
                        CreateCell("Mobile"),
                        CreateCell("Education"),
                        CreateCell("Experience"),
                        CreateCell("Skills"),
                        CreateCell("CV File")
                    );
                    sheetData.Append(header);

                    // Data rows
                    foreach (var c in list)
                    {
                        Row row = new Row();
                        row.Append(
                            CreateCell(c.CandidateName),
                            CreateCell(c.Email),
                            CreateCell(c.Mobile),
                            CreateCell(c.Education),
                            CreateCell(c.Experience),
                            CreateCell(c.Skills),
                            CreateCell(c.CandidateCV != null ? c.CandidateCV.FileName : "No File")
                        );
                        sheetData.Append(row);
                    }

                    workbookPart.Workbook.Save();
                }

                workbook.Position = 0;
                return File(workbook.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Candidates.xlsx");
            }
        }

        private Cell CreateCell(string text)
        {
            return new Cell()
            {
                DataType = CellValues.String,
                CellValue = new CellValue(text ?? "")
            };
        }
        [HttpPost]
        public IActionResult BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return RedirectToAction("Index");

            var profiles = _context.CandidateProfiles.Where(x => ids.Contains(x.Id)).ToList();

            _context.CandidateProfiles.RemoveRange(profiles);

            _context.SaveChanges();

            TempData["Success"] = "Selected candidates deleted successfully!";
            return RedirectToAction("Index");
        }

        // ===================== EDIT ============================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var profile = _context.CandidateProfiles.Include(x=>x.CandidateCV).FirstOrDefault(x => x.Id == id);
            if (profile == null) return NotFound();

            ViewBag.CVs = _context.CandidateCVs.ToList();
            return View(profile);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, CandidateProfile model, IFormFile NewCV)
        {
            var user = await _userManager.GetUserAsync(User);
            var oldData = await _context.CandidateProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            var candidate = await _context.CandidateProfiles
                .Include(x => x.CandidateCV)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (candidate == null)
                return NotFound();

            // Update profile fields
            candidate.CandidateName = model.CandidateName;
            candidate.Email = model.Email;
            candidate.Mobile = model.Mobile;
            candidate.Education = model.Education;
            candidate.Experience = model.Experience;
            candidate.Skills = model.Skills;

            // ====== NEW CV UPLOAD (PDF / DOC / DOCX Allowed) ======
            if (NewCV != null && NewCV.Length > 0)
            {
                // Validate extension
                var ext = Path.GetExtension(NewCV.FileName).ToLower();
                var allowed = new[] { ".pdf", ".doc", ".docx" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("NewCV", "Only PDF, DOC, DOCX formats allowed.");
                    return View(model);
                }

                // Folder
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/CVs");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Unique file name
                string uniqueName = Guid.NewGuid().ToString() + ext;
                string filePath = Path.Combine(folder, uniqueName);

                // Save new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await NewCV.CopyToAsync(stream);
                }

                // Delete old file if exists
                if (candidate.CandidateCV != null && !string.IsNullOrEmpty(candidate.CandidateCV.FilePath))
                {
                    string oldPath = Path.Combine("wwwroot", candidate.CandidateCV.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Create or update record
                if (candidate.CandidateCV == null)
                    candidate.CandidateCV = new CandidateCV();

                candidate.CandidateCV.FileName = NewCV.FileName;
                candidate.CandidateCV.FilePath = "/CVs/" + uniqueName;
                candidate.CandidateCV.UploadedOn = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            await _log.LogUpdateAsync(oldData, model);
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
        public  async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var profile = _context.CandidateProfiles.FirstOrDefault(x => x.Id == id);
            if (profile == null) return NotFound();

            _context.CandidateProfiles.Remove(profile);
            _context.SaveChanges();
            await _log.LogDeleteAsync(profile);

            TempData["success"] = "Profile deleted!";
            return RedirectToAction("Index");
        }
    }
}
