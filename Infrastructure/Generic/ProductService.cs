using ASP.Net_Core_MVC.Data;
using ASP.Net_Core_MVC.DTOS.Product;
using ASP.Net_Core_MVC.Infrastructure.IGeneric;
using ASP.Net_Core_MVC.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_MVC.Infrastructure.Generic
{
    public class ProductService : IProduct
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public ProductService(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<GetProduct>> GetProductList()
        {
            // Retrieve products from the database asynchronously
            var products = await _context.Products.ToListAsync();

            // Map the products to AddProduct DTOs
            var mappedProducts = _mapper.Map<List<GetProduct>>(products);

            return mappedProducts;
        }
        public async Task<GetProduct> GetProductById(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return null; // or throw an exception

            // Map the product to GetProduct DTO
            var productDto = _mapper.Map<GetProduct>(product);
            return productDto;
        }
        public async Task<GetProduct> AddProduct(AddProduct newProduct)
        {
            try
            {
                var product = _mapper.Map<Product>(newProduct);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Map the newly added product to the GetProduct DTO
                var addedProductDto = _mapper.Map<GetProduct>(product);
                return addedProductDto;
            }
            catch
            {
                // Handle exceptions appropriately
                return null; // or throw an exception
            }
        }
        public async Task<GetProduct> EditProduct(int productId, EditProduct updatedProduct)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(productId);
                if (existingProduct == null)
                    return null; // or throw an exception

                _mapper.Map(updatedProduct, existingProduct);
                await _context.SaveChangesAsync();

                // Map the updated product to the GetProduct DTO
                var updatedProductDto = _mapper.Map<GetProduct>(existingProduct);
                return updatedProductDto;
            }
            catch
            {
                // Handle exceptions appropriately
                return null; // or throw an exception
            }
        }
        public async Task<List<GetProduct>> DeleteProduct(int productId)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(productId);
                if (existingProduct == null)
                    return null; // or throw an exception

                _context.Products.Remove(existingProduct);
                await _context.SaveChangesAsync();

                // You may return a list of remaining products or null if you don't need it
                var remainingProducts = await GetProductList();
                return remainingProducts;
            }
            catch
            {
                // Handle exceptions appropriately
                return null; // or throw an exception
            }
        }

    }
}
