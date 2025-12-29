using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elearning.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Column("OrderIndex")]
        public int Order { get; set; }
    }
}