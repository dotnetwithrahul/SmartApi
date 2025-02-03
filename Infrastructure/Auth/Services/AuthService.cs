using FirebaseApiMain.Infrastructure.Auth.Interface;
using FirebaseApiMain.Infrastructure.Auth.Models;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Firebase;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FirebaseApiMain.Infrastructure.Auth.Services
{
    public class AuthService : IAuthInterface
    {



        private readonly HttpClient _client;




        private readonly IConfiguration Configuration;

        private readonly IMemoryCache _cache;
        private const string OtpCacheKeyPrefix = "Otp_";
        private const int OtpExpiryMinutes = 2; // OTP validity duration in m
        public AuthService(HttpClient client, IMemoryCache cache, IConfiguration configuration)
        {
            _client = client;
            _cache = cache;
            Configuration = configuration;
        }



        public async Task<IActionResult> ManageAdminAsync(AdminUsers AdminUsersRequest)
        {
            try
            {
                string customerUrl;
                StringContent content = null;
                HttpResponseMessage response = null;

                switch (AdminUsersRequest.Flag.ToLower())
                {
                    case "create":


                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/AdminUsers.json?auth={FirebaseContext.FirebaseAuthKey}";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, CustomerRequest>>(allCustomersData);


                            if (allCustomers != null && allCustomers.Values.Any(c => c.email == AdminUsersRequest.email))
                            {
                                return new BadRequestObjectResult("Already registered with this email. Go to Login");
                            }
                        }


                        string newadminIdId = "admin_" + Guid.NewGuid().ToString();
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/AdminUsers/{newadminIdId}.json?auth={FirebaseContext.FirebaseAuthKey}";

                        content = new StringContent(JsonSerializer.Serialize(new
                        {
                            adminId = newadminIdId,
                            firstName = AdminUsersRequest.firstName,
                            lastName = AdminUsersRequest.lastName,
                            email = AdminUsersRequest.email,
                            passwordHash = HashPassword(AdminUsersRequest.passwordHash),
                            Role = AdminUsersRequest.Role,
                            phoneNumber = AdminUsersRequest.phoneNumber,
                            dateRegistered = DateTime.Now.ToString("o")
                        }), Encoding.UTF8, "application/json");

                        response = await _client.PutAsync(customerUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            return new OkObjectResult(new { Message = "Admin registered successfully."});
                        }
                        break;

                  
              


                   
                    case "login":
                        if (string.IsNullOrEmpty(AdminUsersRequest.emailOrPhone) || string.IsNullOrEmpty(AdminUsersRequest.passwordHash))
                            return new BadRequestObjectResult(new { Status = false, Message = "Email/Phone and password must be provided for login." });

                        // Fetch all customers from Firebase
                        customerUrl = $"{FirebaseContext.FirebaseDatabaseUrl}/AdminUsers.json?auth={FirebaseContext.FirebaseAuthKey}";
                        response = await _client.GetAsync(customerUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var allCustomersData = await response.Content.ReadAsStringAsync();
                            var allCustomers = JsonSerializer.Deserialize<Dictionary<string, AdminUsers>>(allCustomersData);

                            if (allCustomers == null || !allCustomers.Any())
                            {
                                return new NotFoundObjectResult(new { Status = false, Message = "Invalid username or password. Please try again" });
                            }

                            // Find customer by email or phone number
                            var customer = allCustomers.Values.FirstOrDefault(c =>
                                c.email == AdminUsersRequest.emailOrPhone || c.phoneNumber == AdminUsersRequest.emailOrPhone);

                            if (customer == null)
                            {
                                return new BadRequestObjectResult(new { Status = false, Message = "Invalid username or password. Please try again" });
                            }

                            // Verify password
                            bool isPasswordValid = VerifyPassword(AdminUsersRequest.passwordHash, customer.passwordHash);
                            if (!isPasswordValid)
                            {
                                return new BadRequestObjectResult(new { Status = false, Message = "Invalid password." });
                            }

                            // Find the customer ID from the dictionary
                            var customerId = allCustomers.FirstOrDefault(x => x.Value.email == AdminUsersRequest.emailOrPhone || x.Value.phoneNumber == AdminUsersRequest.emailOrPhone).Key;

                            var token = GenerateJwtToken(customer);

                            return new OkObjectResult(new
                            {
                                Status = true,
                                Message = "Login successful.",
                                token
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
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }













        private string GenerateJwtToken(AdminUsers user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSection:Key"])); // Use a strong key and store it securely, e.g., Azure Key Vault
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create claims for role-based access
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.adminId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.email),
         new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

            var token = new JwtSecurityToken(
                issuer: Configuration["JwtSection:Issuer"],
                audience: Configuration["JwtSection:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // Token expiration
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }









    }
}
