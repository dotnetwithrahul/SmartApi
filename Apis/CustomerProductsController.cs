using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerProductsController : ControllerBase
    {

        private readonly ICustomerProductService _customerProductService;
        private readonly IProductRepository productRepository;
        public CustomerProductsController(ICustomerProductService customerProductService , IProductRepository _productRepository)
        {
            _customerProductService = customerProductService;
            productRepository = _productRepository;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _customerProductService.GetAllProductsAsync();
            return Ok(products);
        }



        [HttpPost("InitiatePaymentAsync")]
        public async Task<IActionResult> InitiatePaymentAsync([FromBody] PaymentRequest paymentRequest)
        {
            return await productRepository.InitiatePaymentAsync(paymentRequest);
        }







        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // Set up the customer request with the 'login' flag
            var customerRequest = new CustomerRequest
            {
                Flag = "login",
                email = loginRequest.Email,
                passwordHash = loginRequest.Password
            };

            // Call the service method
            var result = await productRepository.ManageCustomerAsync(customerRequest);

            return result;
        }







        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtpAsync([FromBody] OtpRequest otpRequest)
        {
            var otpRequests = new OtpRequest
            {
                email = otpRequest.email
            };
            return await productRepository.SendOtpAsync(otpRequests);
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomerAsync([FromBody] CustomerRequest customerRequest)
        {

            var customerRequests = new CustomerRequest {

                email = customerRequest.email,
                passwordHash = customerRequest.passwordHash,
                otp = customerRequest.otp,
                Flag = "create"
            };
            return await productRepository.ManageCustomerAsync(customerRequests);
        }
    }
}
