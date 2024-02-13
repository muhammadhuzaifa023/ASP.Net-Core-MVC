using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.Models
{
    public class Product
    {
        [Key] // For making ID Autoincrement 
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string Category { get; set; }
        public string color { get; set; }
        [Display(Name = "Unit price")]
        public decimal UnitPrice { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
