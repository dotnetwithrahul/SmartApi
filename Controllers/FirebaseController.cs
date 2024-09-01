using FirebaseApiMain.Models;
using FirebaseApiMain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {


        public FirebaseService fc;
        public FirebaseController(FirebaseService _fc)
        {
            fc = _fc;   
        }


        //[HttpGet]

        //public async  Task<IActionResult> Get()
        //{
        //    var data = await  fc.GetAllProducts();

        //    return Ok(data);        
        //}


        //[HttpGet]
        //public async Task<ActionResult<Dictionary<string, Product>>> GetProducts()
        //{
        //    var products = await fc.GetAllProducts();
        //    return Ok(products);
        //}



        //[HttpPost]
        //public async Task<IActionResult> AddProduct([FromBody] Product product)
        //{
        //    if (product == null)
        //    {
        //        return BadRequest("Product data is null");
        //    }

        //    // Generate a unique product ID or pass it from the client
        //    var productId = "product_" + Guid.NewGuid().ToString(); // Example of generating a unique ID

        //    try
        //    {
        //        await fc.AddProductAsync(productId, product);
        //        return Ok(new { Message = "Product added successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = ex.Message });
        //    }
        //}




        //[HttpPut("{productId}")]
        //public async Task<IActionResult> UpdateProduct(string productId, [FromBody] Product product)
        //{
        //    if (string.IsNullOrEmpty(productId) || product == null)
        //    {
        //        return BadRequest("Invalid input");
        //    }

        //    try
        //    {
        //        await fc.UpdateProductAsync(productId, product);
        //        return Ok(new { Message = "Product updated successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = ex.Message });
        //    }
        //}



        //[HttpDelete("{productId}")]
        //public async Task<IActionResult> DeleteProduct(string productId)
        //{
        //    if (string.IsNullOrEmpty(productId))
        //    {
        //        return BadRequest("Product ID is required");
        //    }

        //    try
        //    {
        //        await fc.DeleteProductAsync(productId);
        //        return Ok(new { Message = "Product deleted successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = ex.Message });
        //    }
        //}

    }
}
