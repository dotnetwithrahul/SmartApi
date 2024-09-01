using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
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

    }
}
