using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        public int? ResultId { get; set; }
        public Result? Result { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool Passed { get; set; }

        // relative path under wwwroot, e.g. "certificates/{file}.html"
        [StringLength(255)]
        public string? FilePath { get; set; }

        [StringLength(64)]
        public string VerificationCode { get; set; } = Guid.NewGuid().ToString("N");
    }
}