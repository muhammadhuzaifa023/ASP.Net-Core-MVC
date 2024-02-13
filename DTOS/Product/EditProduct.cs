using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.DTOS.Product
{
    public class EditProduct
    {
        [Key] // For making ID Autoincrement 
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string Category { get; set; }
        public string color { get; set; }
        [Required]
        [Display(Name = "Unit price")]
        public decimal UnitPrice { get; set; }
        [Required]
        [Display(Name = "Available quantity")]
        public int AvailableQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

