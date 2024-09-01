using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Infrastructure.Interface;

namespace FirebaseApiMain.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository ProductRepository { get; }
        ICustomerProductService CustomerProductServicecs { get; }
        //ICategoryRepository CategoryRepository { get; }
        //Task<int> CompleteAsync();
    }
}
