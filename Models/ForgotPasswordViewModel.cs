using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
