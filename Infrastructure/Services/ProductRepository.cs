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



namespace FirebaseApiMain.Infrastructure.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly HttpClient _client;


        private readonly IMemoryCache _cache;
        private const string OtpCacheKeyPrefix = "Otp_";
        private const int OtpExpiryMinutes = 5; // OTP validity duration in m
        public ProductRepository(HttpClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
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

                        //categoryRequest.image_url = null;

                        //if (categoryRequest.image_url == null)
                        //{
                        //    categoryRequest.image_url = await SavecategoriesImageToFileSystem(categoryRequest.imageFile, newCategoryId);
                        //}


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
            // Generate OTP and store it in memory cache with an expiry of 5 minutes
            string otp = GenerateOtp();
            Console.WriteLine(otp);

            string cacheKey = OtpCacheKeyPrefix + otpRequest.email;

            // Remove any existing OTP if present
            _cache.Remove(cacheKey);



            _cache.Set(OtpCacheKeyPrefix + otpRequest.email, (otp, DateTime.UtcNow), TimeSpan.FromMinutes(OtpExpiryMinutes));

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
        }
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


                        if (!VerifyOtp(customerRequest.email, customerRequest.otp))
                            return new BadRequestObjectResult("Invalid or expired OTP.");



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
                            Country = customerRequest.Country,
                            Nearby = customerRequest.Nearby,
                            city = customerRequest.city,
                            state = customerRequest.state,
                            zipCode = customerRequest.zipCode,
                            dateRegistered = DateTime.UtcNow.ToString("o"), // ISO 8601 format
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

                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers/{customerRequest.customerId}.json";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                           
                            firstName = customerRequest.firstName,
                            lastName = customerRequest.lastName,
                            email = customerRequest.email,
                            passwordHash = HashPassword(customerRequest.passwordHash),
                            phoneNumber = customerRequest.phoneNumber,
                            customerImageUrl = customerRequest.customerImageUrl,
                            Addressline1 = customerRequest.Addressline1,
                            Country = customerRequest.Country,
                            Nearby = customerRequest.Nearby,
                            city = customerRequest.city,
                            state = customerRequest.state,
                            zipCode = customerRequest.zipCode,
                            dateRegistered = DateTime.UtcNow.ToString("o"), // ISO 8601 format
                            isActive = true
                        }), Encoding.UTF8, "application/json");

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
                        if (string.IsNullOrEmpty(customerRequest.email) || string.IsNullOrEmpty(customerRequest.passwordHash))
                            return new BadRequestObjectResult("Email and password must be provided for login.");

                        // Fetch customer by email
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/customers.json";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, CustomerRequest>>(allCustomersData);

                            if (allCustomers == null || !allCustomers.Any())
                            {
                                return new NotFoundObjectResult("No customers found.");
                            }

                            var customer = allCustomers.Values.FirstOrDefault(c => c.email == customerRequest.email);

                            if (customer == null)
                            {
                                return new BadRequestObjectResult("Customer not found.");
                            }

                            // Verify password
                            bool isPasswordValid = VerifyPassword(customerRequest.passwordHash, customer.passwordHash);
                            if (!isPasswordValid)
                            {
                                return new BadRequestObjectResult("Invalid password.");
                            }

                            return new OkObjectResult(new { Message = "Login successful.", Customer = customer });
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
