using cv.deltareco.com.Areas.Admin.ViewModels;
using cv.deltareco.com.Models;
using cv.deltareco.com.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cv.deltareco.com.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]  // only login users allowed
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ActivityLogService _log;

      
        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ActivityLogService log)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _log = log;
        }


        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new RegisterViewModel());
        }

        // ============================================
        // POST: Create User
        // ============================================
        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["success"] = "Registration successful!";
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            await _log.LogCreateAsync(model);
            return View(model);
        }
        // ============================================
        // GET: Delete User
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // ============================================
        // POST: Delete User
        // ============================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            await _log.LogDeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(user);
        }
        // ============================================
        // GET: Edit User
        // ============================================
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // ============================================
        // POST: Edit User
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
          if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // Update basic info
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email; // username = email
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            await _log.LogUpdateAsync(user, model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // Update password if entered
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Index");
        }
    }
}
