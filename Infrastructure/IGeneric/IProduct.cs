using ASP.Net_Core_MVC.DTOS.Product;

namespace ASP.Net_Core_MVC.Infrastructure.IGeneric
{
    public interface IProduct
    {
        Task<List<GetProduct>> GetProductList();
        Task<GetProduct> GetProductById(int id);
        Task<GetProduct> AddProduct(AddProduct newProduct);
        Task<GetProduct> EditProduct(int productId, EditProduct updatedProduct);
        Task<List<GetProduct>> DeleteProduct(int productId);

    }
}
