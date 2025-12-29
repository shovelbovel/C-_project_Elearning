using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Elearning.Data;
using Elearning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Elearning.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public ProfileController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
        {
            var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out var id)) return Forbid();

            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        public async Task<IActionResult> Edit()
        {
            var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out var id)) return Forbid();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, string? newPassword)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = _hasher.HashPassword(user, newPassword);
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetThemePreference([FromBody] ThemePreferenceModel model)
        {
            var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out var id)) return Forbid();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (model.Theme != "light" && model.Theme != "dark" && model.Theme != "system")
            {
                return BadRequest();
            }

            user.ThemePreference = model.Theme;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }

    public class ThemePreferenceModel { public string Theme { get; set; } = "light"; }
}