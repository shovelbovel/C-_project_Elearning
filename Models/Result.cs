using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Result
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public int Score { get; set; }
        public int MaxScore { get; set; }

        public bool Passed { get; set; }
        public DateTime TakenAt { get; set; } = DateTime.UtcNow;
    }
}