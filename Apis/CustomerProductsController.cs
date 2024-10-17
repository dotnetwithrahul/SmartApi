using Firebase.Storage;
using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;

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
                emailOrPhone = loginRequest.emailOrPhone,
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
                Flag = otpRequest.Flag, 
                email = otpRequest.email,
                otp = otpRequest.otp
            };
            return await productRepository.SendOtpAsync(otpRequests);
        }


        [HttpPost("send-otpV2")]
        public async Task<IActionResult> SendOtpAsyncV2([FromBody] OtpRequest otpRequest)
        {
            var otpRequests = new OtpRequest
            {
                Flag = otpRequest.Flag,
                email = otpRequest.email,
                otp = otpRequest.otp
            };
            return await productRepository.SendOtpAsync(otpRequests);
        }

        //[HttpPost("managetpAsync")]
        //public async Task<IActionResult> managetpAsync([FromBody] OtpRequest otpRequest)
        //{
        //    var otpRequests = new OtpRequest
        //    {
        //        Flag = otpRequest.Flag,
        //        email = otpRequest.email,
        //        otp = otpRequest.otp
        //    };
        //    return await productRepository.ManageOtpAsync(otpRequests);
        //    }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomerAsync([FromBody] CustomerRequest customerRequest)
        {
            if (customerRequest.Flag.ToLower() == "create")
            {
                var customerRequests = new CustomerRequest
                {

                    email = customerRequest.email,
                    passwordHash = customerRequest.passwordHash,
                    firstName = customerRequest.firstName,
                    lastName = customerRequest.lastName,
                    phoneNumber = customerRequest.phoneNumber,
                    customerImageUrl = customerRequest.customerImageUrl,
                    isActive = customerRequest.isActive,
                    Addressline1 = customerRequest.Addressline1,
                    Addressline2 = customerRequest.Addressline2,
                    Country = customerRequest.Country,
                    Nearby = customerRequest.Nearby,
                    city = customerRequest.city,
                    state = customerRequest.state,
                    zipCode = customerRequest.zipCode,
                    Flag = "create"
                };

                return await productRepository.ManageCustomerAsync(customerRequests);
            }


            if (customerRequest.Flag.ToLower() == "update")
            {
                var customerRequests = new CustomerRequest
                {

                    email = customerRequest.email,
                    customerId = customerRequest.customerId,
                    passwordHash = customerRequest.passwordHash,
                    firstName = customerRequest.firstName,
                    lastName = customerRequest.lastName,
                    phoneNumber = customerRequest.phoneNumber,
                    customerImageUrl = customerRequest.customerImageUrl,
                    isActive = customerRequest.isActive,
                    Addressline1 = customerRequest.Addressline1,
                    Addressline2 = customerRequest.Addressline2,
                    Country = customerRequest.Country,
                    Nearby = customerRequest.Nearby,
                    city = customerRequest.city,
                    state = customerRequest.state,
                    zipCode = customerRequest.zipCode,
                    Flag = "update"
                };

                return await productRepository.ManageCustomerAsync(customerRequests);
            }


            if (customerRequest.Flag.ToLower() == "view_by_id")
            {
                var customerRequests = new CustomerRequest
                {

                    email = customerRequest.email,
                    customerId = customerRequest.customerId,
                    passwordHash = customerRequest.passwordHash,
                    firstName = customerRequest.firstName,
                    lastName = customerRequest.lastName,
                    phoneNumber = customerRequest.phoneNumber,
                    customerImageUrl = customerRequest.customerImageUrl,
                    isActive = customerRequest.isActive,
                    Addressline1 = customerRequest.Addressline1,
                    Addressline2 = customerRequest.Addressline2,
                    Country = customerRequest.Country,
                    Nearby = customerRequest.Nearby,
                    city = customerRequest.city,
                    state = customerRequest.state,
                    zipCode = customerRequest.zipCode,
                    Flag = "view_by_id"
                };

                return await productRepository.ManageCustomerAsync(customerRequests);
            }


            else
            {
               return  BadRequest("invalid flag");
            }
        }




        [HttpPost("ManageWishlistAsync")]
        public async Task<IActionResult> ManageWishlistAsync([FromBody] WishlistRequest wishlistRequest)
        {

            return await productRepository.ManageWishlistAsync(wishlistRequest);
        }

        [HttpPost("ManageCartAsync")]
        public async Task<IActionResult> ManageCartAsync([FromBody] CartRequest cartRequest)
        {

            return await productRepository.ManageCartAsync(cartRequest);
        }



        [HttpPost("ManageCouponAsync")]
        public async Task<IActionResult> ManageCouponAsync([FromBody] CouponRequest couponRequest)
        {

            return await productRepository.ManageCouponAsync(couponRequest);
        }


        [HttpPost("ManageOrderAsync")]
        public async Task<IActionResult> ManageOrderAsync(OrderRequest orderRequest)
        {

            return await productRepository.ManageOrderAsync(orderRequest);
        }



    }
}
