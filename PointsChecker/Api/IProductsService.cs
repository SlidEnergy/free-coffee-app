using System.Collections.Generic;
using System.Threading.Tasks;

namespace PointsChecker
{
    public interface IProductsService
    {
        Task<List<Product>> GetFreeProductsAsync(string userId);

        Task<string> OrderProductAsync(Product product, string userId);
    }
}
