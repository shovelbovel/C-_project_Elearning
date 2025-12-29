using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Elearning.Data;
using Microsoft.EntityFrameworkCore;
using Elearning.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Elearning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public AdminController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalCourses = await _db.Courses.CountAsync(),
                TotalQuizzes = await _db.Quizzes.CountAsync(),
                TotalResults = await _db.Results.CountAsync(),
                RecentUsers = await _db.Users.OrderByDescending(u => u.Id).Take(5).Select(u => u.FullName).ToListAsync()
            };

            return View(vm);
        }

        // Simple user list management
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Courses()
        {
            var courses = await _db.Courses.ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> EditUserRole(int id)
        {
            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var roles = await _db.Roles.ToListAsync();
            var vm = new EditUserRoleViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                SelectedRoleId = user.RoleId,
                Roles = roles.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(EditUserRoleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            var newRole = await _db.Roles.FindAsync(model.SelectedRoleId);
            if (newRole == null)
            {
                ModelState.AddModelError(string.Empty, "Selected role does not exist.");
                // reload roles for the view
                var roles = await _db.Roles.ToListAsync();
                model.Roles = roles.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList();
                return View(model);
            }

            // Prevent removing the last admin
            var currentRole = await _db.Roles.FindAsync(user.RoleId);
            if (currentRole != null && currentRole.Name == "Admin" && newRole.Name != "Admin")
            {
                var adminCount = await _db.Users.CountAsync(u => u.RoleId == currentRole.Id);
                if (adminCount <= 1)
                {
                    ModelState.AddModelError(string.Empty, "Cannot remove the last Admin user. Assign another Admin first.");
                    var roles = await _db.Roles.ToListAsync();
                    model.Roles = roles.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList();
                    return View(model);
                }
            }

            user.RoleId = model.SelectedRoleId;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        // Reset user password (admin)
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            var vm = new ResetPasswordViewModel { UserId = user.Id, Email = user.Email, FullName = user.FullName };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction("Users");
        }

        // Formateur dashboard (accessible to Formateur and Admin)
        [Authorize(Roles = "Formateur,Admin")]
        public async Task<IActionResult> FormateurDashboard()
        {
            var courses = await _db.Courses.ToListAsync();
            return View(courses);
        }

        // Student dashboard
        [Authorize(Roles = "Etudiant,Admin,Formateur")]
        public async Task<IActionResult> StudentDashboard()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            var results = await _db.Results.Where(r => r.UserId == userId).ToListAsync();
            return View(results);
        }
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}