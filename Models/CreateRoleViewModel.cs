using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.Models
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }

        public string? Description { get; set; }
    }
}
