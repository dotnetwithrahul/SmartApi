using FirebaseApiMain.Dtos;

namespace FirebaseApiMain.Application.Interfaces
{
    public interface ICustomerProductService
    {
        Task<Dictionary<string, object>> GetAllProductsAsync();
        //Task<ProductDTO> GetProductByIdAsync(int id);
    }
}

