using FirebaseApiMain.Dtos;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IProductRepository _productRepository;

        private readonly IFileService fileService;
        public AdminController(IProductRepository productRepository, IFileService _fileService)
        {
            _productRepository = productRepository;
            fileService = _fileService;
        }






        [HttpPost("ManageProductAsync")]
        public async Task<IActionResult> ManageProductAsync([FromBody] ProductRequest productRequest)
        {
            return await _productRepository.ManageProductAsync(productRequest);
        }





        [HttpPost("ManageCategory")]

        public async Task<IActionResult> ManageCategory( CategoryRequest categoryRequest)
        {

            
            return await _productRepository.ManageCategoryAsync(categoryRequest);
        }



        [HttpPost("AddingCategory")]

        public async Task<IActionResult> AddingCategory(CategoryRequest categoryRequest)
        {


            return await _productRepository.ManageCategoryAsync(categoryRequest);
        }

        //[HttpPost("manage")]
        //public async Task<IActionResult> ManageCustomerAsync([FromBody] CustomerRequest customerRequest)
        //{
        //    return await _productRepository.ManageCustomerAsync(customerRequest);
        //}



        [HttpPost("ManageCustomerImage")]
        public async Task<IActionResult> ManageCustomerImage( IFormFile? ImageFile)
        {
            string[] allowedFileExtentions = [".jpg", ".jpeg", ".png"];
            //string createdImageName = await fileService.SaveFileAsync(ImageFile, allowedFileExtentions);

            return Ok();



        }
    }
}
