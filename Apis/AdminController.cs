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

        [HttpPost("ManageCustomerAsync")]
        public async Task<IActionResult> ManageCustomerAsync([FromBody] CustomerRequest customerRequest)
        {
            return await _productRepository.ManageCustomerAsync(customerRequest);
        }




        [HttpPost("AddingCategory")]

        public async Task<IActionResult> AddingCategory(CategoryImageRequest categoryRequest)
        {

            var data = await _productRepository.AddCategoryAsync(categoryRequest);

            return Ok(data);
        }



        [HttpPost("AddingProdcut")]

        public async Task<IActionResult> AddingProdcut(ProductImageRequest productImageRequest)
        {

            var data = await _productRepository.AddingProdcutAsync(productImageRequest);



            return Ok(data);
        }





        [HttpPost("UpdateProductAsync")]

        public async Task<IActionResult> UpdateProductAsync(ProductImageRequest productImageRequest)
        {

            var data = await _productRepository.UpdateProductAsync(productImageRequest);

             return Ok(data);
        }







        //[HttpPost("ManageCustomerImage")]
        //public async Task<IActionResult> ManageCustomerImage( IFormFile? ImageFile)
        //{
        //    string[] allowedFileExtentions = [".jpg", ".jpeg", ".png"];
        //    //string createdImageName = await fileService.SaveFileAsync(ImageFile, allowedFileExtentions);

        //    return Ok();


        //}
    }
}
