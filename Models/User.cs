using System.ComponentModel.DataAnnotations;

namespace Elearning.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public ICollection<Result> Results { get; set; } = new List<Result>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        // Persisted user preferences (simple for now)
        [StringLength(20)]
        public string ThemePreference { get; set; } = "light"; // values: light, dark, system
    }
}