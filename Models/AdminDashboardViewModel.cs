using System.Collections.Generic;

namespace Elearning.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalResults { get; set; }
        public List<string> RecentUsers { get; set; } = new List<string>();
    }
}