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
        public string ServiceKeyName => ((IProduct)this).ServiceKeyName;

        private IProductService _productApi { get; set; }
        private readonly IServiceProvider _serviceProvider;

        public ProductNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _productApi = apiServices.ProductService;
            _serviceProvider = serviceProvider;
        }

        [DeserializeWebApiNopCommerceResponse]
        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            return await _productApi.GetByIdAsync(productId);
        }
    }
}
