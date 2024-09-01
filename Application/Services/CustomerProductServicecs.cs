using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Dtos;
using FirebaseApiMain.Infrastructure.Interface;

namespace FirebaseApiMain.Application.Services
{
    public class CustomerProductServicecs : ICustomerProductService
    {
        private readonly IProductRepository _productRepository;

        public CustomerProductServicecs(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<Dictionary<string, object>> GetAllProductsAsync()
        {
            // Call the repository to get the combined categories and products
            var result = await _productRepository.GetAllProductsAsync();
            return result;
        }


        //public async Task<ProductDTO> GetProductByIdAsync(int id)
        //{
        //    // Fetch the product by ID
        //    var product = await _productRepository.GetProductByIdAsync(id);
        //    if (product == null)
        //    {
        //        return null;
        //    }

        //    // Convert Product entity to ProductDTO
        //    var productDto = new ProductDTO
        //    {
        //        Id = product.Id,
        //        Name = product.Name,
        //        Weight = product.Weight,
        //        PricePerBag = product.PricePerBag,
        //        PricePerQuintal = product.PricePerQuintal,
        //        Amc = product.Amc,
        //        ImageUrl = product.ImageUrl,
        //        CategoryId = product.CategoryId
        //    };

        //    return productDto;
        //}
    }
}
