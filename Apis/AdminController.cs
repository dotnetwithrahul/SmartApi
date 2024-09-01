using FirebaseApiMain.Dtos;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IProductRepository _productRepository;


        public AdminController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }






        [HttpPost("ManageProductAsync")]
        public async Task<IActionResult> ManageProductAsync([FromBody] ProductRequest productRequest)
        {
            return await _productRepository.ManageProductAsync(productRequest);
        }





        [HttpPost("ManageCategory")]

        public async Task<IActionResult> ManageCategory([FromBody] CategoryRequest categoryRequest)
        {
            return await _productRepository.ManageCategoryAsync(categoryRequest);
        }


        [HttpPost("manage")]
        public async Task<IActionResult> ManageCustomerAsync([FromBody] CustomerRequest customerRequest)
        {
            return await _productRepository.ManageCustomerAsync(customerRequest);
        }



    }
}
