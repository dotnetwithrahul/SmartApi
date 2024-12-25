using FirebaseApiMain.Infrastructure.Auth.Interface;
using FirebaseApiMain.Infrastructure.Auth.Models;
using FirebaseApiMain.Infrastructure.Entities;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuperAdminController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        private readonly IFileService fileService;
        public SuperAdminController(IProductRepository productRepository, IFileService _fileService)
        {
            _productRepository = productRepository;
            fileService = _fileService;
        }







        [HttpPost("GetAdminOrders")]
        public async Task<IActionResult> GetAdminOrders(string pageNumber, string pageSize)
        {
            return await _productRepository.GetAdminOrders(pageNumber, pageSize);
        }







    }
}
