using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.Models
{
    public class VerifyTwoFactorTokenViewModel
    {
        public string? Email { get; set; }
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
        public string? Token { get; set; }
        [Required]
        [Display(Name = "Two Factor Code")]
        public string? TwoFactorCode { get; set; }
    }
}
