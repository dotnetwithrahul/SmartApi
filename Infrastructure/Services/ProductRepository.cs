using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Firebase;
using FirebaseApiMain.Infrastructure.Interface;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using FirebaseApiMain.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;

//using Newtonsoft.Json;

using Paytm;
using Paytm.Checksum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Firebase.Storage;
//using Newtonsoft.Json;



namespace FirebaseApiMain.Infrastructure.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly HttpClient _client;

        private readonly IFileService fileService;

        private readonly IMemoryCache _cache;
        private const string OtpCacheKeyPrefix = "Otp_";
        private const int OtpExpiryMinutes = 2; // OTP validity duration in m
        public ProductRepository(HttpClient client, IMemoryCache cache , IFileService _fileService)
        {
            _client = client;
            _cache = cache;
            fileService = _fileService;
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

                if (products != null)
                {
                    // Assign Firebase key to product id
                    foreach (var productKey in products.Keys)
                    {
                        if (products[productKey].Id == null)
                        {
                            products[productKey].Id = productKey;
                        }
                    }
                }
                result["products"] = products ?? new Dictionary<string, Product>();
            }
            else
            {
                result["products"] = new Dictionary<string, Product>();
            }

            return result;
        }



        public async Task<bool> AddCategoryAsync(CategoryImageRequest category)
        {
            var result = false;
            try
            {
                // Generate a new UUID for the category ID with a custom prefix
                var categoryId = "cat_" + Guid.NewGuid().ToString();


                var imageUrl = await UploadCatImageToFirebaseStorageAsync(category.imageFile);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    Console.WriteLine("Image upload failed.");
                    return false;
                }

                // Create a new category object with the generated ID
                var categoryWithId = new
                {
                    id = categoryId,
                    name = category.name,
                    image_url = imageUrl
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




        public async Task<bool> AddingProdcutAsync(ProductImageRequest productRequest)
        {
            var result = false;
            try
            {
                string productUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                // Generate a new UUID for the product ID with a custom prefix
                string newProductId = "prod_" + Guid.NewGuid().ToString();

                // Upload the product image to Firebase storage and retrieve the image URL
                var imageUrl = await UploadPrdImageToFirebaseStorageAsync(productRequest.imageFile);

                // Check if image upload was successful
                if (string.IsNullOrEmpty(imageUrl))
                {
                    Console.WriteLine("Image upload failed.");
                    return false;
                }

                // Assign the uploaded image URL to the product request
                productRequest.image_url = imageUrl;

                // Generate a new UUID for the category ID with a custom prefix if it doesn't exist
                if (string.IsNullOrEmpty(productRequest.categoryId))
                {
                    productRequest.categoryId = "cat_" + Guid.NewGuid().ToString();
                }

                // Prepare the product URL in Firebase
                productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{newProductId}.json";

                // Serialize the product details to be stored in Firebase
                content = new StringContent(JsonSerializer.Serialize(new
                {
                    Id = newProductId,
                    productRequest.name,
                    productRequest.Description,
                    productRequest.ShortDescription,
                    productRequest.Weight,
                    productRequest.WeightUnit,
                    productRequest.StockQuantity,
                    productRequest.IsOutOfStock,
                    productRequest.RestockDate,
                    productRequest.Discount,
                    productRequest.Amount,
                    productRequest.image_url,
                    productRequest.categoryId,
                    productRequest.Rating,
                    productRequest.ReviewCount
                }), Encoding.UTF8, "application/json");

                // Send the product data to Firebase
                response = await _client.PutAsync(productUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    result = true; // Indicate success if the response is successful
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred while adding the product: {ex.Message}");
            }

            return result; // Return whether the product was successfully added or not
        }

        public async Task<string> UploadPrdImageToFirebaseStorageAsync(IFormFile imageFile)
        {
            try
            {
                // Generate a unique filename with prefix
                var fileName = "cat_img_" + Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Open the file stream for the image
                using (var stream = imageFile.OpenReadStream())
                {
                    // Initialize the FirebaseStorage object with your storage bucket
                    var firebaseStorage = new FirebaseStorage("superapiimages.appspot.com");

                    // Upload the file and get the download URL
                    var downloadUrl = await firebaseStorage
                        .Child("products") // Directory name in the storage bucket
                        .Child(fileName)     // Filename
                        .PutAsync(stream);

                    // Return the public download URL
                    return downloadUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                return null;
            }
        }


        public async Task<string> UploadCatImageToFirebaseStorageAsync(IFormFile imageFile)
        {
            try
            {
                // Generate a unique filename with prefix
                var fileName = "cat_img_" + Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Open the file stream for the image
                using (var stream = imageFile.OpenReadStream())
                {
                    // Initialize the FirebaseStorage object with your storage bucket
                    var firebaseStorage = new FirebaseStorage("superapiimages.appspot.com");

                    // Upload the file and get the download URL
                    var downloadUrl = await firebaseStorage
                        .Child("categories") // Directory name in the storage bucket
                        .Child(fileName)     // Filename
                        .PutAsync(stream);

                    // Return the public download URL
                    return downloadUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                return null;
            }
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
                            Id = newProductId,
                            productRequest.name,
                            productRequest.Description,
                            productRequest.ShortDescription,
                            productRequest.Weight,
                            productRequest.WeightUnit,
                            productRequest.StockQuantity,
                            productRequest.IsOutOfStock,
                            productRequest.RestockDate,
                            productRequest.Discount,
                            productRequest.Amount,
                            productRequest.image_url,
                            productRequest.categoryId,
                            productRequest.Rating,
                            productRequest.ReviewCount
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
                            var product = JsonSerializer.Deserialize<Product>(productData);
                            return new OkObjectResult(product);
                        }
                        break;

                    case "view_all":
                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products.json";
                        response = await _client.GetAsync(productUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allProductsData = await response.Content.ReadAsStringAsync();
                            var allProducts = JsonSerializer.Deserialize<Dictionary<string, Product>>(allProductsData);

                            return new OkObjectResult(allProducts);
                        }
                        break;

                    case "update":
                        if (string.IsNullOrEmpty(productRequest.ProductId))
                            return new BadRequestObjectResult("Product ID must be provided for update operation.");

                        // Fetch the existing product
                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";
                        response = await _client.GetAsync(productUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            return new NotFoundObjectResult("Product not found.");
                        }

                        var existingProductData = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(existingProductData) || existingProductData == "{}" || existingProductData == "null")
                        {
                            return new NotFoundObjectResult("Product not found.");
                        }

                        var existingProduct = JsonSerializer.Deserialize<Product>(existingProductData);

                        // Update only the fields that are not null in the request
                        var updatedProduct = new
                        {
                            name = productRequest.name ?? existingProduct.name,
                            Description = productRequest.Description ?? existingProduct.Description,
                            ShortDescription = productRequest.ShortDescription ?? existingProduct.ShortDescription,
                            Weight = productRequest.Weight ?? existingProduct.Weight,
                            WeightUnit = productRequest.WeightUnit ?? existingProduct.WeightUnit,
                            StockQuantity = productRequest.StockQuantity ?? existingProduct.StockQuantity,
                            IsOutOfStock = productRequest.IsOutOfStock ?? existingProduct.IsOutOfStock,
                            RestockDate = productRequest.RestockDate ?? existingProduct.RestockDate,
                            Discount = productRequest.Discount ?? existingProduct.Discount,
                            Amount = productRequest.Amount ?? existingProduct.Amount,
                            image_url = productRequest.image_url ?? existingProduct.image_url,
                            categoryId = productRequest.categoryId ?? existingProduct.categoryId,
                            Rating = productRequest.Rating ?? existingProduct.Rating,
                            ReviewCount = productRequest.ReviewCount ?? existingProduct.ReviewCount
                        };

                        content = new StringContent(JsonSerializer.Serialize(updatedProduct), Encoding.UTF8, "application/json");

                        // Send the update request
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
                    case "update_stock_status":
                        if (string.IsNullOrEmpty(productRequest.ProductId))
                            return new BadRequestObjectResult("Product ID must be provided for update_stock_status operation.");

                        // Fetch the existing product
                        productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";
                        response = await _client.GetAsync(productUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            return new NotFoundObjectResult("Product not found.");
                        }

                        var existingStockProductData = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(existingStockProductData) || existingStockProductData == "{}" || existingStockProductData == "null")
                        {
                            return new NotFoundObjectResult("Product not found.");
                        }

                        var existingStockProduct = JsonSerializer.Deserialize<Product>(existingStockProductData);

                        // Update stock status and restock date if provided
                        var updatedStockProduct = new
                        {
                            IsOutOfStock = productRequest.IsOutOfStock ?? existingStockProduct.IsOutOfStock,
                            RestockDate = productRequest.RestockDate ?? existingStockProduct.RestockDate
                        };

                        content = new StringContent(JsonSerializer.Serialize(updatedStockProduct), Encoding.UTF8, "application/json");

                        // Send the update request
                        response = await _client.PatchAsync(productUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Stock status updated successfully." });
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


        public async Task<bool> UpdateProductAsync(ProductImageRequest productRequest)
        {
            var result = false;
            try
            {
                if (string.IsNullOrEmpty(productRequest.ProductId))
                {
                    Console.WriteLine("Product ID must be provided for update operation.");
                    return false;
                }

                // Fetch the existing product from Firebase
                string productUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/products/{productRequest.ProductId}.json";
                var response = await _client.GetAsync(productUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Product not found.");
                    return false;
                }

                var existingProductData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(existingProductData) || existingProductData == "{}" || existingProductData == "null")
                {
                    Console.WriteLine("Product not found.");
                    return false;
                }

                // Deserialize the existing product data
                var existingProduct = JsonSerializer.Deserialize<Product>(existingProductData);

                // Check if there's a new image to upload; otherwise, keep the existing image
                if (productRequest.imageFile != null)
                {
                    var imageUrl = await UploadPrdImageToFirebaseStorageAsync(productRequest.imageFile);
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        Console.WriteLine("Image upload failed.");
                        return false;
                    }
                    productRequest.image_url = imageUrl;
                }
                else
                {
                    productRequest.image_url = existingProduct.image_url;
                }

                // Retain existing fields if the corresponding request fields are null
                var updatedProduct = new
                {
                    Id = existingProduct.Id,
                    name = productRequest.name ?? existingProduct.name,
                    Description = productRequest.Description ?? existingProduct.Description,
                    ShortDescription = productRequest.ShortDescription ?? existingProduct.ShortDescription,
                    Weight = productRequest.Weight ?? existingProduct.Weight,
                    WeightUnit = productRequest.WeightUnit ?? existingProduct.WeightUnit,
                    StockQuantity = productRequest.StockQuantity ?? existingProduct.StockQuantity,
                    IsOutOfStock = productRequest.IsOutOfStock ?? existingProduct.IsOutOfStock,
                    RestockDate = productRequest.RestockDate ?? existingProduct.RestockDate,
                    Discount = productRequest.Discount ?? existingProduct.Discount,
                    Amount = productRequest.Amount ?? existingProduct.Amount,
                    image_url = productRequest.image_url,
                    categoryId = productRequest.categoryId ?? existingProduct.categoryId,
                    Rating = productRequest.Rating ?? existingProduct.Rating,
                    ReviewCount = productRequest.ReviewCount ?? existingProduct.ReviewCount
                };

                // Serialize the updated product details
                var content = new StringContent(JsonSerializer.Serialize(updatedProduct), Encoding.UTF8, "application/json");

                // Send the update request to Firebase
                response = await _client.PatchAsync(productUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    result = true; // Indicate success if the response is successful
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred while updating the product: {ex.Message}");
            }

            return result; // Return whether the product was successfully updated or not
        }






        //public async Task<IActionResult> InitiatePaymentAsyncold(PaymentRequest paymentRequest)
        //{
        //    try
        //    {




        //        // Your Paytm credentials
        //        string paytmMerchantId = "GREENF18084341483743";
        //        string paytmMerchantKey = "YOUR_MERCHANT_KEY";
        //        string paytmWebsite = "APPPROD";
        //        string paytmChannelId = "WAP";
        //        string paytmIndustryType = "ecommerce";

        //        // Fetch product details
        //        var productRequest = new ProductRequest { Flag = "view_all" };
        //        var productDetailsResult = await ManageProductAsync(productRequest);

        //        // Check if the result is OK
        //        if (productDetailsResult is OkObjectResult okResult)
        //        {
        //            var productDetails = okResult.Value as Dictionary<string, ProductRequest>;

        //            if (productDetails == null)
        //            {
        //                return new BadRequestObjectResult("Unable to fetch product details.");
        //            }

        //            decimal totalAmount = 0;

        //            foreach (var productEntry in paymentRequest.Products)
        //            {
        //                string productId = productEntry.Key;
        //                int quantity = productEntry.Value;

        //                if (productDetails.TryGetValue(productId, out var product))
        //                {
        //                    // Use null-coalescing operator to handle possible null value
        //                    decimal productAmount = product.Amount ?? 0;
        //                    totalAmount += productAmount * quantity;
        //                }
        //                else
        //                {
        //                    return new BadRequestObjectResult($"Product with ID {productId} not found.");
        //                }
        //            }












        //            Dictionary<string, string> parameters = new Dictionary<string, string>();

        //            parameters.Add("MID", paytmMerchantId); //Provided by Paytm
        //            parameters.Add("ORDER_ID", orderid); //unique OrderId for every request








        //            // Generate checksum and create Paytm payment URL
        //            string checksum = GeneratePaytmChecksum(totalAmount, paytmMerchantKey);
        //            string paytmUrl = $"https://securegw.paytm.in/order/process?CHECKSUMHASH={checksum}{totalAmount}";



        //            // Return the Paytm payment URL to redirect the user
        //            return new OkObjectResult(new { PaymentUrl = paytmUrl });
        //        }
        //        else
        //        {
        //            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //}


        public async Task<IActionResult> InitiatePaymentAsync(PaymentRequest paymentRequest)
        {
            try
            {
                string paytmMerchantId = "GREENF3";
                string paytmMerchantKey = "uYGXQRQa";
                string paytmWebsite = "APPPROD";
                string paytmCallbackUrl = "https://yourwebsite.com/callback";

                // Fetch product details
                var productRequest = new ProductRequest { Flag = "view_all" };
                var productDetailsResult = await ManageProductAsync(productRequest);

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
                            decimal productAmount = product.Amount ?? 0;
                            totalAmount += productAmount * quantity;
                        }
                        else
                        {
                            return new BadRequestObjectResult($"Product with ID {productId} not found.");
                        }
                    }
                    var amount = 1.00;
                    string orderId = "ORDER_" + Guid.NewGuid().ToString();
                    Dictionary<string, object> body = new Dictionary<string, object>
                    {
                        { "requestType", "Payment" },
                        { "mid", paytmMerchantId },
                        { "websiteName", paytmWebsite },
                        { "orderId", orderId },
                        { "txnAmount", new Dictionary<string, string> { { "value", amount.ToString("F2") }, { "currency", "INR" } } },
                        { "userInfo", new Dictionary<string, string> { { "custId", paymentRequest.CustomerId } } },
                        { "callbackUrl", paytmCallbackUrl }
                    };


                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    parameters.Add("MID", "GREENF18084341483743");
                    parameters.Add("ORDER_ID", orderId);
                    parameters.Add("CUST_ID", paymentRequest.CustomerId);
                    parameters.Add("CHANNEL_ID", "WAP");
                    parameters.Add("TXN_AMOUNT", "1.00");
                    parameters.Add("WEBSITE", "APPPROD");
                    parameters.Add("CALLBACK_URL", "https://securegw.paytm.in/theia/paytmCallback?ORDER_ID=" + orderId);

                    var paytmChecksum = CheckSum.GenerateCheckSum(paytmMerchantKey, parameters);


                    //string paytmChecksum = Checksum.generateSignature(JsonSerializer.Serialize(body), paytmMerchantKey);
                    Dictionary<string, string> head = new Dictionary<string, string> { { "signature", paytmChecksum } };
                    Dictionary<string, object> requestBody = new Dictionary<string, object>
            {
                { "body", body },
                { "head", head }
            };

                    string post_data = JsonSerializer.Serialize(requestBody);


                   

                    string url = "https://securegw-stage.paytm.in/theia/api/v1/initiateTransaction?mid=" + paytmMerchantId + "&orderId=" + orderId;

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.ContentLength = post_data.Length;

                    using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        requestWriter.Write(post_data);
                    }

                    string responseData;
                    using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                    {
                        responseData = responseReader.ReadToEnd();
                    }

                    // Log the response or handle as needed
                    Console.WriteLine(responseData);

                    return new OkObjectResult(new { PaymentUrl = "https://securegw.paytm.in/order/process", Response = responseData });
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
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
                            image_url = categoryRequest.image_url,
                            slno = categoryRequest.slno // Save the slno
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

                            // Prepare a list with category IDs included and sort by slno
                            var sortedCategories = allCategories
                                .Select(kvp => new
                                {
                                    CategoryId = kvp.Key, // Include category ID
                                    kvp.Value.name,
                                    kvp.Value.image_url,
                                    kvp.Value.slno
                                })
                                .OrderBy(c => c.slno) // Sort by slno
                                .ToList();

                            return new OkObjectResult(sortedCategories);
                        }
                        break;


                    case "update":
                        if (string.IsNullOrEmpty(categoryRequest.CategoryId))
                            return new BadRequestObjectResult("Category ID must be provided for update operation.");

                        categoryUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/categories/{categoryRequest.CategoryId}.json";

                        var existingCategoryResponse = await _client.GetAsync(categoryUrl);
                        if (!existingCategoryResponse.IsSuccessStatusCode)
                        {
                            return new NotFoundObjectResult("Category not found.");
                        }
                        var existingCategoryData = await existingCategoryResponse.Content.ReadAsStringAsync();
                        var existingCategory = JsonSerializer.Deserialize<CategoryRequest>(existingCategoryData);

                        // Update only the fields that are provided
                        var updatedCategory = new
                        {
                            name = categoryRequest.name ?? existingCategory.name,
                            image_url = categoryRequest.image_url ?? existingCategory.image_url,
                            slno = categoryRequest.slno ?? existingCategory.slno // Update slno if provided
                        };

                        content = new StringContent(JsonSerializer.Serialize(updatedCategory), Encoding.UTF8, "application/json");
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





        private async Task<string> SavecategoriesImageToFileSystem(IFormFile imageFile, string categoryId)
        {
            // Define the path to save the image
            string folderPath = Path.Combine("wwwroot", "images", "categories");
            Directory.CreateDirectory(folderPath); // Ensure the folder exists
            string filePath = Path.Combine(folderPath, $"{categoryId}{Path.GetExtension(imageFile.FileName)}");

            // Save the image to the specified path
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return the relative path to the saved image
            return $"/images/categories/{Path.GetFileName(filePath)}";
        }






        public async Task<IActionResult> SendOtpAsync(OtpRequest otpRequest)
        {
            string cacheKey = OtpCacheKeyPrefix + otpRequest.email;

            switch (otpRequest.Flag.ToLower())
            {
                case "generate":
                    // Generate OTP and store it in memory cache with an expiry of 5 minutes
                    string otp = GenerateOtp();
                    //Console.WriteLine(otp);

                    // Remove any existing OTP if present
                    _cache.Remove(cacheKey);

                    // Store the OTP in cache with an expiration time
                    _cache.Set(cacheKey, (otp, DateTime.UtcNow), TimeSpan.FromMinutes(OtpExpiryMinutes));

                    // SMTP email sending configuration
                    string smtpServer = "smtp.gmail.com";
                    int smtpPort = 587; // or 465 for SSL
                    string smtpUsername = "facebookfire96@gmail.com";
                    string smtpPassword = "pbml emow qhsk oaws";
                    string fromEmail = "facebookfire96@gmail.com";

                    // Compose the email
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = "Your OTP Code",
                        Body = $"Your OTP code is: {otp}",
                        IsBodyHtml = false, // Set to true if using HTML body
                    };

                    mailMessage.To.Add(otpRequest.email);

                    try
                    {
                        using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                        {
                            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            smtpClient.EnableSsl = true; // Set to true if your SMTP server requires SSL
                            await smtpClient.SendMailAsync(mailMessage);
                        }

                        return new OkObjectResult(new { Message = "OTP sent to the provided email address." });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
                        return new StatusCodeResult(500); // Internal Server Error
                    }

                case "verify":
                    // Verify the provided OTP
                    if (_cache.TryGetValue(cacheKey, out (string cachedOtp, DateTime createdAt) cachedValue))
                    {
                        if (cachedValue.cachedOtp == otpRequest.otp && DateTime.UtcNow - cachedValue.createdAt < TimeSpan.FromMinutes(OtpExpiryMinutes))
                        {
                            return new OkObjectResult(new { Status = true, Message = "OTP verified successfully." });
                        }
                        else
                        {
                            return new OkObjectResult(new { Status = false, Message = "Invalid or expired OTP." });
                        }
                    }
                    return new OkObjectResult(new { Status = false, Message = "OTP not found." });

                default:
                    return new BadRequestObjectResult("Invalid flag. Valid flags are 'generate' and 'verify'.");
            }
        }




        public async Task<IActionResult> ManageOtpAsync(OtpRequest otpRequest)
        {
            try
            {
                string otpUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (otpRequest.Flag.ToLower())
                {
                    case "create":
                        string newCategoryId = "cat_" + Guid.NewGuid().ToString();
                       
                        otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps/{otpRequest.email}.json";
                        //string[] allowedFileExtentions = [".jpg", ".jpeg", ".png"];
                        //string imageUrl = await fileService.SaveFileAsync(categoryRequest.imageFile, new[] { ".jpg", ".jpeg", ".png" }, newCategoryId);

                        //string createdImageName = await fileService.SaveFileAsync(ImageFile, allowedFileExtentions);

                        otpRequest.otp = "123456";
                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            otp = otpRequest.otp,
                            createdAt = DateTime.UtcNow.ToString("o")
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PutAsync(otpUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Category created successfully.", CategoryId = newCategoryId });
                        }
                        break;

                

                    case "view_all":
                        otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps.json";
                        response = await _client.GetAsync(otpUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCategoriesData = await response.Content.ReadAsStringAsync();
                            var allCategories = JsonSerializer.Deserialize<Dictionary<string, OtpRequest>>(allCategoriesData);

                            if (allCategories == null || !allCategories.Any())
                            {
                                return new NotFoundObjectResult("No categories found.");
                            }

                            return new OkObjectResult(allCategories);
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



        public async Task<IActionResult> SendOtpAsyncV2(OtpRequest otpRequest)
        {
            try
            {
                string otpUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (otpRequest.Flag.ToLower())
                {
                    case "generate":
                        string otp = GenerateOtp();
                        otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps/{otpRequest.email.Replace(".", "_")}.json";

                        // Serialize OTP data
                        var otpData = new
                        {
                            otp = otp,
                            createdAt = DateTime.Now.ToString("o") // ISO 8601 format
                        };

                        string jsonOtpData = JsonSerializer.Serialize(otpData);
                        content = new StringContent(jsonOtpData, Encoding.UTF8, "application/json");

                        // Send PUT request to Firebase
                        response = await _client.PutAsync(otpUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            // Send OTP via SMTP
                            bool isEmailSent = await SendOtpEmailAsync(otpRequest.email, otp);
                            if (isEmailSent)
                            {
                                return new OkObjectResult(new { Message = "OTP sent to the provided email address." });
                            }
                            return new StatusCodeResult(500);
                        }
                        //if (response.IsSuccessStatusCode)
                        //{
                        //    // Send OTP via SMTP asynchronously
                        //    var emailSendingResult = await Task.Run(() => SendOtpEmailAsync(otpRequest.email, otp));
                        //    return new OkObjectResult(new { Message = "OTP sent to the provided email address." });
                        //}
                        break;

                    case "verify":
                        otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps/{otpRequest.email.Replace(".", "_")}.json";
                        response = await _client.GetAsync(otpUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var otpDatav = await response.Content.ReadAsStringAsync();

                            // Check if the data exists
                            if (string.IsNullOrEmpty(otpDatav) || otpDatav == "{}" || otpDatav == "null")
                            {
                                return new BadRequestObjectResult("OTP not found.");
                            }

                            // Deserialize the JSON response into OtpModel
                            OtpRequest storedOtp = JsonSerializer.Deserialize<OtpRequest>(otpDatav);

                            if (storedOtp != null)
                            {
                                // Convert 'createdAt' to DateTime
                                DateTime createdAt = DateTime.Parse(storedOtp.createdAt);

                                // Compare the OTP values and check expiry time
                                if (storedOtp.otp == otpRequest.otp && DateTime.Now - createdAt < TimeSpan.FromMinutes(OtpExpiryMinutes))
                                {
                                    // Remove OTP after successful verification
                                    //await _client.DeleteAsync(otpUrl);
                                    return new OkObjectResult(new { Status = true, Message = "OTP verified successfully." });
                                }
                                else
                                {
                                    return new OkObjectResult(new { Status = false, Message = "Invalid or expired OTP." });
                                }
                            }
                        }
                        break;



                    default:
                        return new BadRequestObjectResult("Invalid flag. Valid flags are 'generate' and 'verify'.");
                }

                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        private async Task<bool> SendOtpEmailAsync(string email, string otp)
        {
            try
            {
                // SMTP email sending configuration
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "facebookfire96@gmail.com";
                string smtpPassword = "pbml emow qhsk oaws";
                string fromEmail = "facebookfire96@gmail.com";

                // Compose the email
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = "Your OTP Code",
                    Body = $"Your OTP code is: {otp}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);

                // Send email asynchronously
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mailMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
                return false;
            }
        }

        //public async Task<IActionResult> SendOtpAsyncV2(OtpRequest otpRequest)
        //{
        //    try
        //    {
        //        string otpUrl;
        //        HttpResponseMessage response = null;

        //        switch (otpRequest.Flag.ToLower())
        //        {
        //            case "generate":
        //                string otp = GenerateOtp();
        //                string otpId = Guid.NewGuid().ToString(); // Unique ID for the OTP entry
        //                otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps/{otpRequest.email}.json";

        //                // Prepare data to store in Firebase
        //                var otpData = new
        //                {
        //                    otp = otp,
        //                    createdAt = DateTime.UtcNow.ToString("o") // ISO 8601 format
        //                };

        //                var content = new StringContent(JsonSerializer.Serialize(otpData), Encoding.UTF8, "application/json");

        //                // Remove any existing OTP if present
        //                await _client.DeleteAsync(otpUrl);

        //                // Store the new OTP in Firebase
        //                response = await _client.PutAsync(otpUrl, content);

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    // SMTP email sending configuration
        //                    string smtpServer = "smtp.gmail.com";
        //                    int smtpPort = 587;
        //                    string smtpUsername = "facebookfire96@gmail.com";
        //                    string smtpPassword = "pbml emow qhsk oaws";
        //                    string fromEmail = "facebookfire96@gmail.com";

        //                    // Compose the email
        //                    var mailMessage = new MailMessage
        //                    {
        //                        From = new MailAddress(fromEmail),
        //                        Subject = "Your OTP Code",
        //                        Body = $"Your OTP code is: {otp}",
        //                        IsBodyHtml = false
        //                    };

        //                    mailMessage.To.Add(otpRequest.email);

        //                    try
        //                    {
        //                        using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
        //                        {
        //                            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        //                            smtpClient.EnableSsl = true;
        //                            await smtpClient.SendMailAsync(mailMessage);
        //                        }

        //                        return new OkObjectResult(new { Message = "OTP sent to the provided email address." });
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
        //                        return new StatusCodeResult(500);
        //                    }
        //                }
        //                break;

        //            case "verify":
        //                otpUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/otps/{otpRequest.email}.json";
        //                response = await _client.GetAsync(otpUrl);

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    var otpData = await response.Content.ReadAsStringAsync();
        //                    if (string.IsNullOrEmpty(otpData) || otpData == "{}" || otpData == "null")
        //                    {
        //                        return new BadRequestObjectResult("OTP not found.");
        //                    }

        //                    var storedOtp = JsonSerializer.Deserialize<dynamic>(otpData);
        //                    string storedOtpValue = storedOtp.otp;
        //                    DateTime createdAt = DateTime.Parse(storedOtp.createdAt);

        //                    if (storedOtpValue == otpRequest.otp && DateTime.UtcNow - createdAt < TimeSpan.FromMinutes(OtpExpiryMinutes))
        //                    {
        //                        // Remove OTP after successful verification
        //                        await _client.DeleteAsync(otpUrl);
        //                        return new OkObjectResult(new { Status = true, Message = "OTP verified successfully." });
        //                    }
        //                    else
        //                    {
        //                        return new OkObjectResult(new { Status = false, Message = "Invalid or expired OTP." });
        //                    }
        //                }
        //                break;

        //            default:
        //                return new BadRequestObjectResult("Invalid flag. Valid flags are 'generate' and 'verify'.");
        //        }

        //        return new StatusCodeResult((int)response.StatusCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        return new StatusCodeResult(500);
        //    }
        //}

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();

        }


        private bool VerifyOtp(string email, string otp)
        {
            if (_cache.TryGetValue(OtpCacheKeyPrefix + email, out (string cachedOtp, DateTime createdAt) cachedValue))
            {
                // Check if the provided OTP matches the cached OTP and if it has not expired
                if (cachedValue.cachedOtp == otp && DateTime.UtcNow - cachedValue.createdAt < TimeSpan.FromMinutes(OtpExpiryMinutes))
                {
                    return true;
                }
            }
            return false;
        }




        public async Task<IActionResult> ManageCustomerAsync(CustomerRequest customerRequest)
        {
            try
            {
                string customerUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (customerRequest.Flag.ToLower())
                {
                    case "create":


                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers.json";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, CustomerRequest>>(allCustomersData);

                           
                            if (allCustomers != null && allCustomers.Values.Any(c => c.email == customerRequest.email))
                            {
                                return new BadRequestObjectResult("Already registered with this email. Go to Login");
                            }
                        }


                        string newCustomerId = "customer_" + Guid.NewGuid().ToString();
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers/{newCustomerId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            id = newCustomerId,
                            firstName = customerRequest.firstName,
                            lastName = customerRequest.lastName,
                            email = customerRequest.email,
                            passwordHash = HashPassword(customerRequest.passwordHash),
                            phoneNumber = customerRequest.phoneNumber,
                            customerImageUrl = customerRequest.customerImageUrl,
                            Addressline1 = customerRequest.Addressline1,
                            Addressline2 = customerRequest.Addressline2,
                            Country = customerRequest.Country,
                            Nearby = customerRequest.Nearby,
                            city = customerRequest.city,
                            state = customerRequest.state,
                            zipCode = customerRequest.zipCode,
                            dateRegistered = DateTime.Now.ToString("o"), // ISO 8601 format
                            isActive = true
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PutAsync(customerUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Customer registered successfully.", CustomerId = newCustomerId });
                        }
                        break;

                    case "view_by_id":
                        if (string.IsNullOrEmpty(customerRequest.customerId))
                            return new BadRequestObjectResult("Customer ID must be provided for view_by_id operation.");

                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers/{customerRequest.customerId}.json";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var customerData = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(customerData) || customerData == "{}" || customerData == "null")
                            {
                                return new NotFoundObjectResult("Customer not found.");
                            }
                            var customer = JsonSerializer.Deserialize<CustomerRequest>(customerData);
                            return new OkObjectResult(customer);
                        }
                        break;

                    case "view_all":
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers.json";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, CustomerRequest>>(allCustomersData);
                            return new OkObjectResult(allCustomers);
                        }
                        break;

                    case "update":
                        if (string.IsNullOrEmpty(customerRequest.customerId))
                            return new BadRequestObjectResult("Customer ID must be provided for update operation.");

                        // Fetch the existing customer data
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers/{customerRequest.customerId}.json";
                        response = await _client.GetAsync(customerUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            return new NotFoundObjectResult("Customer not found.");
                        }

                        var existingCustomerData = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(existingCustomerData) || existingCustomerData == "{}" || existingCustomerData == "null")
                        {
                            return new NotFoundObjectResult("Customer not found.");
                        }

                        // Deserialize the existing customer data
                        var existingCustomer = JsonSerializer.Deserialize<CustomerRequest>(existingCustomerData);

                        // Update fields only if they are provided (not null or empty)
                        var updatedCustomer = new
                        {
                            firstName = !string.IsNullOrEmpty(customerRequest.firstName) ? customerRequest.firstName : existingCustomer.firstName,
                            lastName = !string.IsNullOrEmpty(customerRequest.lastName) ? customerRequest.lastName : existingCustomer.lastName,
                            email = !string.IsNullOrEmpty(customerRequest.email) ? customerRequest.email : existingCustomer.email,
                            passwordHash = !string.IsNullOrEmpty(customerRequest.passwordHash) ? HashPassword(customerRequest.passwordHash) : existingCustomer.passwordHash,
                            phoneNumber = !string.IsNullOrEmpty(customerRequest.phoneNumber) ? customerRequest.phoneNumber : existingCustomer.phoneNumber,
                            customerImageUrl = !string.IsNullOrEmpty(customerRequest.customerImageUrl) ? customerRequest.customerImageUrl : existingCustomer.customerImageUrl,
                            Addressline1 = !string.IsNullOrEmpty(customerRequest.Addressline1) ? customerRequest.Addressline1 : existingCustomer.Addressline1,
                            Country = !string.IsNullOrEmpty(customerRequest.Country) ? customerRequest.Country : existingCustomer.Country,
                            Nearby = !string.IsNullOrEmpty(customerRequest.Nearby) ? customerRequest.Nearby : existingCustomer.Nearby,
                            city = !string.IsNullOrEmpty(customerRequest.city) ? customerRequest.city : existingCustomer.city,
                            state = !string.IsNullOrEmpty(customerRequest.state) ? customerRequest.state : existingCustomer.state,
                            zipCode = !string.IsNullOrEmpty(customerRequest.zipCode) ? customerRequest.zipCode : existingCustomer.zipCode,
                            dateRegistered = existingCustomer.dateRegistered, // Keep the original registration date
                            isActive = existingCustomer.isActive // Keep the original status
                        };

                        // Send the updated customer data
                        content = new StringContent(JsonSerializer.Serialize(updatedCustomer), Encoding.UTF8, "application/json");
                        response = await _client.PatchAsync(customerUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Customer updated successfully." });
                        }
                        break;


                    case "delete":
                        if (string.IsNullOrEmpty(customerRequest.customerId))
                            return new BadRequestObjectResult("Customer ID must be provided for delete operation.");

                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers/{customerRequest.customerId}.json";
                        response = await _client.DeleteAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Customer deleted successfully." });
                        }
                        break;

                    case "login":
                        if (string.IsNullOrEmpty(customerRequest.emailOrPhone) || string.IsNullOrEmpty(customerRequest.passwordHash))
                            return new BadRequestObjectResult(new { Status = false, Message = "Email/Phone and password must be provided for login." });

                        // Fetch all customers from Firebase
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers.json";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, CustomerRequest>>(allCustomersData);

                            if (allCustomers == null || !allCustomers.Any())
                            {
                                return new NotFoundObjectResult(new { Status = false, Message = "Invalid username or password. Please try again" });
                            }

                            // Find customer by email or phone number
                            var customer = allCustomers.Values.FirstOrDefault(c =>
                                c.email == customerRequest.emailOrPhone || c.phoneNumber == customerRequest.emailOrPhone);

                            if (customer == null)
                            {
                                return new BadRequestObjectResult(new { Status = false, Message = "Invalid username or password. Please try again" });
                            }

                            // Verify password
                            bool isPasswordValid = VerifyPassword(customerRequest.passwordHash, customer.passwordHash);
                            if (!isPasswordValid)
                            {
                                return new BadRequestObjectResult(new { Status = false, Message = "Invalid password." });
                            }

                            // Find the customer ID from the dictionary
                            var customerId = allCustomers.FirstOrDefault(x => x.Value.email == customerRequest.emailOrPhone || x.Value.phoneNumber == customerRequest.emailOrPhone).Key;

                            return new OkObjectResult(new
                            {
                                Status = true,
                                Message = "Login successful.",
                                Customer = new
                                {
                                    customerId = customerId,
                                    customer.firstName,
                                    customer.lastName,
                                    customer.email,
                                    customer.passwordHash,
                                    customer.phoneNumber,
                                    customer.dateRegistered,
                                    customer.customerImageUrl,
                                    customer.isActive,
                                    customer.Addressline1,
                                    customer.Addressline2,
                                    customer.Country,
                                    customer.Nearby,
                                    customer.city,
                                    customer.state,
                                    customer.zipCode
                                }
                            });
                        }
                        break;






                    default:
                        return new BadRequestObjectResult("Invalid flag. Valid flags are 'create', 'view_all', 'view_by_id', 'update', 'Login', and 'delete' .");
                }

                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        private string HashPassword(string password)
        {
            // Implement password hashing logic here (using a more secure algorithm in production)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string providedPassword, string storedPasswordHash)
        {
            // Encode the provided password in Base64
            string providedPasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(providedPassword));

            // Compare the provided hash with the stored hash
            return providedPasswordHash == storedPasswordHash;
        }

    }
}
