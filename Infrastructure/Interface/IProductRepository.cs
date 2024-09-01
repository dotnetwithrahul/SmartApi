using FirebaseApiMain.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
//using FirebaseApiMain.Models;

namespace FirebaseApiMain.Infrastructure.Interface
{
    public interface IProductRepository
    {
        /// <summary>
        /// Fetches all products along with their categories.
        /// </summary>
        /// <returns>A dictionary with categories and products data.</returns>
        Task<Dictionary<string, object>> GetAllProductsAsync();


        //Task<bool> AddCategoryAsync(Category category);
        //Task<bool> AddProductAsync(Product Pproductroduct);


        Task<IActionResult> ManageProductAsync(ProductRequest productRequest);
        Task<IActionResult> ManageCategoryAsync(CategoryRequest categoryRequest);
        Task<IActionResult> InitiatePaymentAsync(PaymentRequest paymentRequest);

        Task<IActionResult> ManageCustomerAsync(CustomerRequest customerRequest);
        Task<IActionResult> SendOtpAsync(OtpRequest otpRequest);


        /// <summary>
        /// Fetches a product by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>A Product entity if found; otherwise, null.</returns>
        //Task<Product> GetProductByIdAsync(int id);

        ///// <summary>
        ///// Adds a new product to the Firebase database.
        ///// </summary>
        ///// <param name="product">The product to add.</param>
        ///// <returns>The added Product entity with updated ID.</returns>
        //Task<Product> AddProductAsync(Product product);

        ///// <summary>
        ///// Updates an existing product in the Firebase database.
        ///// </summary>
        ///// <param name="product">The product with updated information.</param>
        ///// <returns>A boolean indicating whether the update was successful.</returns>
        //Task<bool> UpdateProductAsync(Product product);

        ///// <summary>
        ///// Deletes a product from the Firebase database by its unique ID.
        ///// </summary>
        ///// <param name="id">The ID of the product to delete.</param>
        ///// <returns>A boolean indicating whether the deletion was successful.</returns>
        //Task<bool> DeleteProductAsync(int id);
    }

}
