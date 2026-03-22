using System.ComponentModel.DataAnnotations;

namespace KT9_Admin_Panel_MVC.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<RoleCheckbox> Roles { get; set; } = new List<RoleCheckbox>();
    }

    public class RoleCheckbox
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}