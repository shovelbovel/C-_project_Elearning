using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        // Minimum percentage required to pass (e.g., 70)
        public int PassPercentage { get; set; } = 70;
    }
}