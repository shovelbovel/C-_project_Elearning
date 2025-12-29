using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = null!;

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();

        // Order of the question within the quiz
        public int Order { get; set; } = 0;
    }
}