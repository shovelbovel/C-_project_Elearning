using Microsoft.AspNetCore.Mvc;
using Elearning.Data;
using Elearning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace Elearning.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CourseController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses.ToListAsync();
            return View(courses);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();
            return View(course);
        }

        [Authorize(Roles = "Admin,Formateur")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Create(Course model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "courses");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fs);
                }
                model.ImagePath = Path.Combine("images", "courses", fileName).Replace("\\", "/");
            }

            _db.Courses.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Edit(Course model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            var course = await _db.Courses.FindAsync(model.Id);
            if (course == null) return NotFound();

            course.Title = model.Title;
            course.Description = model.Description;
            course.Category = model.Category;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "courses");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fs);
                }
                course.ImagePath = Path.Combine("images", "courses", fileName).Replace("\\", "/");
            }

            _db.Courses.Update(course);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}