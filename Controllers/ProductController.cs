using ASP.Net_Core_MVC.DTOS.Product;
using ASP.Net_Core_MVC.Infrastructure.IGeneric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProduct _productService;

        // Inject IProduct service into the constructor
        public ProductController(IProduct productService)
        {
            _productService = productService;
        }
        // --------------------------------------------     Get Product  -------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            // Call GetProductList method from ProductService
            var productList = await _productService.GetProductList();

            // Pass the productList to the view
            return View(productList);


            //// Call GetProductList method from ProductService
            //var productList = await _productService.GetProductList();

            //// Ensure that productList is of type List<GetProduct>
            //var mappedProductList = productList.Select(p => _mapper.Map<GetProduct>(p)).ToList();

            //// Pass the mappedProductList to the view
            //return View(mappedProductList);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //-----------------------------------------------------     AddProduct  ------------------------------------------------------------// 
        // GET: Products/Create

        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProduct newProduct)
        {
            if (ModelState.IsValid)
            {
                var addedProductDto = await _productService.AddProduct(newProduct);
                if (addedProductDto != null)
                {
                    // Product added successfully, you can redirect or return some response
                    return RedirectToAction("GetProduct"); // Assuming there's an Index action in your controller
                }
                else
                {
                    // Handle failure to add product
                    ModelState.AddModelError(string.Empty, "Failed to add product.");
                }
            }

            // If ModelState is not valid or product addition failed, return the view with validation errors
            return View(newProduct);
        }
        //------------------------------------------------    Edit Product --------------------------------------------------------//
        // GET: Products/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            var editProductViewModel = new EditProduct
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Category = product.Category,
                color = product.color,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity
            };

            return View(editProductViewModel);
        }
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProduct updatedProduct)
        {
            if (id != updatedProduct.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updatedProductDto = await _productService.EditProduct(id, updatedProduct);
                if (updatedProductDto != null)
                {
                    // Product updated successfully, you can redirect or return some response
                    return RedirectToAction(nameof(GetProduct));
                }
                else
                {
                    // Handle failure to edit product
                    ModelState.AddModelError(string.Empty, "Failed to update product.");
                }
            }

            // If ModelState is not valid or product editing failed, return the view with validation errors
            return View(updatedProduct);
        }
        //--------------------------------------------------------------  Delete Product ---------------------------------------------------------//
        // GET: Products/DeleteProduct/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        // POST: Products/DeleteProduct/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Call the service method to delete the product
            var deletedProduct = await _productService.DeleteProduct(id);

            if (deletedProduct == null)
            {
                // If the product was not found or deletion failed, return a NotFound result
                return NotFound();
            }

            // Redirect to a suitable page after the deletion (e.g., product list page)
            return RedirectToAction(nameof(GetProduct));
        }


    }
}
