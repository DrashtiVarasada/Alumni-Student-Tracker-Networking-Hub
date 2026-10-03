using AlumniHub.Data;
using AlumniHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlumniHub.Controllers
{
    [Authorize]
    public class AlumniProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumniProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View your profile
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.AlumniProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Create");

            return View(profile);
        }

        // Show create profile form
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.AlumniProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existing != null)
                return RedirectToAction("Index");

            return View();
        }

        // Save new profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlumniProfile profile, IFormFile? profileImage)
        {
            var userId = _userManager.GetUserId(User);
            profile.UserId = userId!;

            if (profileImage != null && profileImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                profile.ProfileImage = fileName;
            }

            _context.AlumniProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Show edit profile form
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.AlumniProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Create");

            return View(profile);
        }

        // Save edited profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AlumniProfile profile, IFormFile? profileImage)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.AlumniProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existing == null)
                return NotFound();

            existing.FullName = profile.FullName;
            existing.Department = profile.Department;
            existing.GraduationYear = profile.GraduationYear;
            existing.CurrentCompany = profile.CurrentCompany;
            existing.Designation = profile.Designation;
            existing.City = profile.City;
            existing.Bio = profile.Bio;

            if (profileImage != null && profileImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                existing.ProfileImage = fileName;
            }

            _context.AlumniProfiles.Update(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Delete profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.AlumniProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
            {
                _context.AlumniProfiles.Remove(profile);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}