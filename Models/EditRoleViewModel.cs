using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.Models
{
    public class EditRoleViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required(ErrorMessage = "Role Name is Required")]
        public string RoleName { get; set; }
    }
}
