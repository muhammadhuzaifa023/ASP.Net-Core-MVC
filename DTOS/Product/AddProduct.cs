using System.ComponentModel.DataAnnotations;

namespace ASP.Net_Core_MVC.DTOS.Product
{
    public class AddProduct
    {
        //public int ProductId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
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
