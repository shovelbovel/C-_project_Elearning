using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? Category { get; set; }

        // Relative path or filename for the course image stored under wwwroot/images/courses
        [StringLength(255)]
        public string? ImagePath { get; set; }

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}