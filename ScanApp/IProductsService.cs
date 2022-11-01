using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScanApp
{
    public interface IProductsService
    {
        Task<List<Product>> GetFreeProductsAsync(string userId);

        Task<string> OrderProductAsync(Product product);
    }
}
