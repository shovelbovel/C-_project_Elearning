using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = null!;

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }
    }
}