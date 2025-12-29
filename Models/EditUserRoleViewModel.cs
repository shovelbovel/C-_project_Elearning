using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Elearning.Models
{
    public class EditUserRoleViewModel
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int SelectedRoleId { get; set; }
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}