using ASP.Net_Core_MVC.DTOS.Product;
using ASP.Net_Core_MVC.Models;
using AutoMapper;

namespace ASP.Net_Core_MVC
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Define your mappings inside the constructor
            CreateMap<Product, GetProduct>();
            CreateMap<AddProduct, Product>();
            CreateMap<EditProduct, Product>();
        }
    }
}
