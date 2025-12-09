using Aspose.Words; // For .doc files
using cv.deltareco.com.Data;
using cv.deltareco.com.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig; // For PDF

namespace cv.deltareco.com.Areas.Admin.Controllers

{
    [Area("Admin")]
    public class CVController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ActivityLogService _log;

        private readonly UserManager<ApplicationUser> _userManager;
        public CVController(ApplicationDbContext context, IWebHostEnvironment env, ActivityLogService log, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _log = log;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {

            var query = _context.CandidateCVs
                      .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.UploadedOn >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.UploadedOn <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }
            var cvs =await query.ToListAsync();
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            return View(cvs);
        }
        public IActionResult CandidateProfiles()
        {
            var cvs = _context.CandidateProfiles.AsNoTracking().ToList();
            return View(cvs);
        }
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> CVFiles)
        {
            var user = await _userManager.GetUserAsync(User);
            if (CVFiles == null || CVFiles.Count == 0)
            {
                TempData["error"] = "Please select at least one file.";
                return RedirectToAction("Index");
            }

            long maxSize = 5 * 1024 * 1024; // 5 MB

            foreach (var file in CVFiles)
            {
                if (file.Length > maxSize)
                {
                    TempData["error"] = $"File {file.FileName} exceeds the 5 MB limit.";
                    return RedirectToAction("Index");
                }
            }

            // SAVE FILES
            foreach (var file in CVFiles)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/CVs");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var cv = new CandidateCV
                {
                    FileName = file.FileName,
                    FilePath = "/CVs/" + file.FileName,
                    UploadedOn = DateTime.Now
                };

                _context.CandidateCVs.Add(cv);
            }
            //                      await _log.LogAsync("CV", "Create",
            //    user.Id.ToString(),
            //    null,
            //    user,
            //    User.Identity.Name
            //);
       
            await _context.SaveChangesAsync();
  
            TempData["success"] = "CV(s) uploaded successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cv = await _context.CandidateCVs.FindAsync(id);

            if (cv == null)
                return NotFound();

            return View(cv);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string FileName)
        {
            var user = await _userManager.GetUserAsync(User);

            var cv = await _context.CandidateCVs.FindAsync(id);

            if (cv == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(FileName))
            {
                TempData["error"] = "File name cannot be empty.";
                return RedirectToAction("Edit", new { id });
            }

            // Rename file in wwwroot
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/CVs");

            string oldPath = Path.Combine(folderPath, cv.FileName);
            string newPath = Path.Combine(folderPath, FileName);

            try
            {
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Move(oldPath, newPath);
                }
            }
            catch
            {
                TempData["error"] = "Error renaming file. Try another name.";
                return RedirectToAction("Edit", new { id });
            }

            // Update record
            cv.FileName = FileName;
            cv.FilePath = "/CVs/" + FileName;
            _context.CandidateCVs.Update(cv);

            TempData["success"] = "CV updated successfully!";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return RedirectToAction("Index");

            var cvFiles = _context.CandidateCVs.Where(x => ids.Contains(x.Id)).ToList();

            // delete physical files too
            foreach (var file in cvFiles)
            {
                if (System.IO.File.Exists(file.FilePath))
                    System.IO.File.Delete(file.FilePath);
            }

            _context.CandidateCVs.RemoveRange(cvFiles);
            _context.SaveChanges();

            TempData["Success"] = "Selected CVs deleted successfully!";
            return RedirectToAction("Index");
        }


        // GET: Show Delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cv = await _context.CandidateCVs.FindAsync(id);

            if (cv == null)
                return NotFound();

            return View(cv); // show confirmation view
        }

        // POST: Perform delete after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var cv = await _context.CandidateCVs.FindAsync(id);

            if (cv == null)
                return NotFound();

            // Delete file from server
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CVs", cv.FileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath); 
            await _log.LogDeleteAsync(cv);

            // Remove from DB
            _context.CandidateCVs.Remove(cv);
            await _context.SaveChangesAsync();

            TempData["success"] = "CV deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Extract(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Fetch CV Record
            var cv = await _context.CandidateCVs.FirstOrDefaultAsync(x => x.Id == id);
            if (cv == null)
            {
                TempData["error"] = "CV record not found.";
                return RedirectToAction("Index");
            }

            // File path
            string fullPath = Path.Combine(_env.WebRootPath, cv.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["error"] = "File not found on server.";
                return RedirectToAction("Index");
            }

            string extension = Path.GetExtension(fullPath).ToLower();
            string text = "";

            // Detect and extract
            switch (extension)
            {
                case ".pdf":
                    text = ExtractTextFromPdf(fullPath);
                    break;

                case ".docx":
                    text = ExtractTextFromDocx(fullPath);
                    break;

                case ".doc":
                    text = ExtractTextFromDoc(fullPath);
                    break;

                default:
                    TempData["error"] = "Unsupported file format!";
                    return RedirectToAction("Index");
            }

            // Extract data
            string name = ExtractName(text);
            string email = ExtractEmail(text);
            string mobile = ExtractMobile(text);
            string skills = ExtractSkills(text);
            string education = ExtractEducation(text);
            string experience = ExtractExperience(text);

            // Save Candidate Profile
            var profile = new CandidateProfile
            {
                CandidateName = name,
                Email = email,
                Mobile = mobile,
                Education = education,
                Experience = experience,
                Skills = skills,
                CandidateCVId = cv.Id   // IMPORTANT: Now FK will NOT fail
            };

//            await _log.LogAsync("CV", "Extracted",
//user.Id.ToString(),
//null,
//user,
//User.Identity.Name
//);
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();

            TempData["success"] = "Candidate details extracted successfully!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> BulkExtract(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["error"] = "Please select at least one CV.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            foreach (var id in ids)
            {
                var cv = await _context.CandidateCVs.FirstOrDefaultAsync(x => x.Id == id);
                if (cv == null) continue;

                string fullPath = Path.Combine(_env.WebRootPath, cv.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(fullPath)) continue;

                string extension = Path.GetExtension(fullPath).ToLower();
                string text = "";

                switch (extension)
                {
                    case ".pdf":
                        text = ExtractTextFromPdf(fullPath);
                        break;

                    case ".docx":
                        text = ExtractTextFromDocx(fullPath);
                        break;

                    case ".doc":
                        text = ExtractTextFromDoc(fullPath);
                        break;
                }

                if (string.IsNullOrEmpty(text))
                    continue;

                // Extract details
                string name = ExtractName(text);
                string email = ExtractEmail(text);
                string mobile = ExtractMobile(text);
                string skills = ExtractSkills(text);
                string education = ExtractEducation(text);
                string experience = ExtractExperience(text);

                // Save into Profile table
                var profile = new CandidateProfile
                {
                    CandidateName = name,
                    Email = email,
                    Mobile = mobile,
                    Education = education,
                    Experience = experience,
                    Skills = skills,
                    CandidateCVId = cv.Id
                };

                _context.CandidateProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Bulk extraction completed successfully!";
            return RedirectToAction("Index");
        }

        private string ExtractTextFromDoc(string path)
        {
        var doc = new Aspose.Words.Document(path);
            return doc.GetText();
        }
        private string ExtractTextFromDocx(string path)
        {
            using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(path, false))
            {
                DocumentFormat.OpenXml.Wordprocessing.Body body =
                    wordDoc.MainDocumentPart.Document.Body;

                return body.InnerText;
            }
        }

        private string ExtractTextFromPdf(string path)
        {
            string text = "";

            using (var doc = UglyToad.PdfPig.PdfDocument.Open(path))
            {
                foreach (var page in doc.GetPages())
                {
                    text += page.Text + "\n";
                }
            }

            return text;
        }


        private string ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : "";
        }

        private string ExtractMobile(string text)
        {
            var match = Regex.Match(text, @"(\+91[-\s]?)?[0-9]{10}");
            return match.Success ? match.Value : "";
        }

        private string ExtractName(string text)
        {
            var match = Regex.Match(text, @"Name[:\- ]+([A-Za-z ]+)");
            if (match.Success) return match.Groups[1].Value.Trim();
            return "";
        }

        private string ExtractSkills(string text)
        {
            var match = Regex.Match(text, @"Skills[:\- ]+([\s\S]{1,200})", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private string ExtractEducation(string text)
        {
            var match = Regex.Match(text, @"Education[:\- ]+([\s\S]{1,300})", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private string ExtractExperience(string text)
        {
            var match = Regex.Match(text, @"Experience[:\- ]+([\s\S]{1,300})", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

    }
}
