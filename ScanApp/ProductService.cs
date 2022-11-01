using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp
{
    public class ProductsService : IProductsService
    {
        public Task<List<Product>> GetFreeProductsAsync(string userId)
        {
            return Task.FromResult(new List<Product> { new Product() });
        }

        public Task<string> OrderProductAsync(Product product)
        {
            return Task.FromResult("");
        }
    }
}
