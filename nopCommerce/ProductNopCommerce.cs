using nopCommerceReplicatorServices.Actions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class ProductNopCommerce : IProduct
    {
        private IProductService _productApi { get; set; }
        private readonly IServiceProvider _serviceProvider;

        public ProductNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _productApi = apiServices.ProductService;
            _serviceProvider = serviceProvider;
        }

        [DeserializeResponse]
        public async Task<ProductDto?> GetProductById(int productId)
        {
            return await _productApi.GetByIdAsync(productId);
        }
    }
}
