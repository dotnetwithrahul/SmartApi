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






        //[HttpPost("AddCategory")]
        //public async Task<IActionResult> AddCategory([FromBody] CategoryDTO categoryDto)
        //{
        //    if (categoryDto == null)
        //    {
        //        return BadRequest("Category data is null.");
        //    }

        //    var category = new Category
        //    {
        //        name = categoryDto.name,
        //        image_url = categoryDto.image_url
        //    };

        //    var success = await _productRepository.AddCategoryAsync(category);
        //    if (success)
        //    {
        //        return Ok("Category added successfully.");
        //    }

        //    return StatusCode(500, "An error occurred while adding the category.");
        //}

        //[HttpPost("addproduct")]
        //public async Task<IActionResult> AddProduct(Product productDto)
        //{
        //    if (productDto == null)
        //    {
        //        return BadRequest("Product data is null.");
        //    }

        //    var product = new Product
        //    {
        //        name = productDto.name,
        //        weight = productDto.weight,
        //        price_per_bag = productDto.price_per_bag,
        //        price_per_quintal = productDto.price_per_quintal,
        //        amc = productDto.amc,
        //        image_url = productDto.image_url,
        //        categoryId = productDto.categoryId // Existing category ID
        //    };

        //    var success = await _productRepository.AddProductAsync(product);
        //    if (success)
        //    {
        //        return Ok("Product added successfully.");
        //    }

        //    return StatusCode(500, "An error occurred while adding the product.");
        //}

    }
}
