using Microsoft.AspNetCore.Identity;

namespace ASP.Net_Core_MVC.Models
{
    public class ApplicationRole : IdentityRole
    {
        // Add custom properties here
        public string? Description { get; set; }

    }
}
