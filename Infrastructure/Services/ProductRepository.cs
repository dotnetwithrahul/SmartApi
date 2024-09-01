using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Firebase;
using FirebaseApiMain.Infrastructure.Interface;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using FirebaseApiMain.Dtos;

namespace FirebaseApiMain.Infrastructure.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly HttpClient _client;



        public ProductRepository(HttpClient client)
        {
            _client = client;
        }



        public async Task<Dictionary<string, object>> GetAllProductsAsync()
        {
            var result = new Dictionary<string, object>();

            // Fetch categories
            var categoriesUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories.json";
            var categoriesResponse = await _client.GetAsync(categoriesUrl);

            if (categoriesResponse.IsSuccessStatusCode)
            {
                var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<Dictionary<string, Category>>(categoriesContent);
                result["categories"] = categories ?? new Dictionary<string, Category>();
            }
            else
            {
                result["categories"] = new Dictionary<string, Category>();
            }

            // Fetch products
            var productsUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products.json";
            var productsResponse = await _client.GetAsync(productsUrl);

            if (productsResponse.IsSuccessStatusCode)
            {
                var productsContent = await productsResponse.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<Dictionary<string, Product>>(productsContent);
                result["products"] = products ?? new Dictionary<string, Product>();
            }
            else
            {
                result["products"] = new Dictionary<string, Product>();
            }

            return result;
        }



        public async Task<bool> AddCategoryAsync(Category category)
        {
            var result = false;
            try
            {
                // Generate a new UUID for the category ID with a custom prefix
                var categoryId = "cat_" + Guid.NewGuid().ToString();

                // Create a new category object with the generated ID
                var categoryWithId = new
                {
                    id = categoryId,
                    name = category.name,
                    image_url = category.image_url
                };

                // Prepare the request URL, using the custom ID as part of the URL path
                var categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{categoryId}.json";

                // Serialize the category object to JSON
                var categoryJson = JsonSerializer.Serialize(categoryWithId);

                // Create the HTTP request content
                var content = new StringContent(categoryJson, Encoding.UTF8, "application/json");

                // Send a PUT request to Firebase with the custom ID in the URL
                var response = await _client.PutAsync(categoryUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred while adding the category: {ex.Message}");
            }

            return result;
        }











        public async Task<IActionResult> ManageProductAsync(ProductRequest productRequest)
        {
            try
            {
                string productUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (productRequest.Flag.ToLower())
                {
                    case "create":
                        string newProductId = "prod_" + Guid.NewGuid().ToString();
                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{newProductId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            productRequest.name,
                            productRequest.weight,
                            productRequest.no_of_bags,
                            productRequest.no_of__quintals,
                            productRequest.Amount,
                            productRequest.amc,
                            productRequest.image_url,
                            productRequest.categoryId
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PutAsync(productUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Product created successfully.", ProductId = newProductId });
                        }
                        break;

                    case "view_by_id":
                        if (string.IsNullOrEmpty(productRequest.ProductId))
                            return new BadRequestObjectResult("Product ID must be provided for view_by_id operation.");

                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";
                        response = await _client.GetAsync(productUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var productData = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(productData) || productData == "{}" || productData == "null")
                            {
                                return new NotFoundObjectResult("Product not found.");
                            }
                            var product = JsonSerializer.Deserialize<ProductRequest>(productData);
                            return new OkObjectResult(product);
                        }
                        break;

                    case "view_all":
                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products.json";
                        response = await _client.GetAsync(productUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allProductsData = await response.Content.ReadAsStringAsync();
                            var allProducts = JsonSerializer.Deserialize<Dictionary<string, ProductRequest>>(allProductsData);

                            
                            return new OkObjectResult(allProducts);
                        }
                        break;

                    case "update":
                        if (string.IsNullOrEmpty(productRequest.ProductId))
                            return new BadRequestObjectResult("Product ID must be provided for update operation.");

                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            productRequest.name,
                            productRequest.weight,
                            productRequest.no_of_bags,
                            productRequest.no_of__quintals,
                            productRequest.Amount,
                            productRequest.amc,
                            productRequest.image_url,
                            productRequest.categoryId
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PatchAsync(productUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Product updated successfully." });
                        }
                        break;

                    case "delete":
                        if (string.IsNullOrEmpty(productRequest.ProductId))
                            return new BadRequestObjectResult("Product ID must be provided for delete operation.");

                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";
                        response = await _client.DeleteAsync(productUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Product deleted successfully." });
                        }
                        break;

                    default:
                        return new BadRequestObjectResult("Invalid flag. Valid flags are 'create', 'view_all', 'view_by_id', 'update', and 'delete'.");
                }

                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }





        public async Task<IActionResult> InitiatePaymentAsync(PaymentRequest paymentRequest)
        {
            try
            {
                // Your Paytm credentials
                string paytmMerchantId = "YOUR_MERCHANT_ID";
                string paytmMerchantKey = "YOUR_MERCHANT_KEY";
                string paytmWebsite = "YOUR_WEBSITE";
                string paytmChannelId = "YOUR_CHANNEL_ID";
                string paytmIndustryType = "YOUR_INDUSTRY_TYPE";

                // Fetch product details
                var productRequest = new ProductRequest { Flag = "view_all" };
                var productDetailsResult = await ManageProductAsync(productRequest);

                // Check if the result is OK
                if (productDetailsResult is OkObjectResult okResult)
                {
                    var productDetails = okResult.Value as Dictionary<string, ProductRequest>;

                    if (productDetails == null)
                    {
                        return new BadRequestObjectResult("Unable to fetch product details.");
                    }

                    decimal totalAmount = 0;

                    foreach (var productEntry in paymentRequest.Products)
                    {
                        string productId = productEntry.Key;
                        int quantity = productEntry.Value;

                        if (productDetails.TryGetValue(productId, out var product))
                        {
                            // Use null-coalescing operator to handle possible null value
                            decimal productAmount = product.Amount ?? 0;
                            totalAmount += productAmount * quantity;
                        }
                        else
                        {
                            return new BadRequestObjectResult($"Product with ID {productId} not found.");
                        }
                    }

                    // Generate checksum and create Paytm payment URL
                    string checksum = GeneratePaytmChecksum(totalAmount, paytmMerchantKey);
                    string paytmUrl = $"https://securegw.paytm.in/order/process?CHECKSUMHASH={checksum}{totalAmount}";

                    // Return the Paytm payment URL to redirect the user
                    return new OkObjectResult(new { PaymentUrl = paytmUrl });
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }



        private string GeneratePaytmChecksum(decimal amount, string merchantKey)
        {
            // Implement checksum generation logic here
            // You may use a library or your own implementation
            return "checksum_generated_here";
        }


        public async Task<IActionResult> ManageCategoryAsync(CategoryRequest categoryRequest)
        {
            try
            {
                string categoryUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (categoryRequest.Flag.ToLower())
                {
                    case "create":
                        string newCategoryId = "cat_" + Guid.NewGuid().ToString();
                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{newCategoryId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            name = categoryRequest.name,
                            image_url = categoryRequest.image_url
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PutAsync(categoryUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Category created successfully.", CategoryId = newCategoryId });
                        }
                        break;

                    case "view_by_id":
                        if (string.IsNullOrEmpty(categoryRequest.CategoryId))
                            return new BadRequestObjectResult("Category ID must be provided for view_by_id operation.");

                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{categoryRequest.CategoryId}.json";
                        response = await _client.GetAsync(categoryUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var categoryData = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(categoryData) || categoryData == "{}" || categoryData == "null")
                            {
                                return new NotFoundObjectResult("Category not found.");
                            }
                            var category = JsonSerializer.Deserialize<CategoryRequest>(categoryData);
                            return new OkObjectResult(category);
                        }
                        break;

                    case "view_all":
                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories.json";
                        response = await _client.GetAsync(categoryUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCategoriesData = await response.Content.ReadAsStringAsync();
                            var allCategories = JsonSerializer.Deserialize<Dictionary<string, CategoryRequest>>(allCategoriesData);

                            if (allCategories == null || !allCategories.Any())
                            {
                                return new NotFoundObjectResult("No categories found.");
                            }

                            return new OkObjectResult(allCategories);
                        }
                        break;

                    case "update":
                        if (string.IsNullOrEmpty(categoryRequest.CategoryId))
                            return new BadRequestObjectResult("Category ID must be provided for update operation.");

                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{categoryRequest.CategoryId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            name = categoryRequest.name,
                            image_url = categoryRequest.image_url
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PatchAsync(categoryUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Category updated successfully." });
                        }
                        break;

                    case "delete":
                        if (string.IsNullOrEmpty(categoryRequest.CategoryId))
                            return new BadRequestObjectResult("Category ID must be provided for delete operation.");

                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{categoryRequest.CategoryId}.json";
                        response = await _client.DeleteAsync(categoryUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Category deleted successfully." });
                        }
                        break;

                    default:
                        return new BadRequestObjectResult("Invalid flag. Valid flags are 'create', 'view_all', 'view_by_id', 'update', and 'delete'.");
                }

                // Return appropriate status code if no valid operation was performed
                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred during the operation: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }








    }
}
