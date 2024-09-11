using Azure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Objects.Customer;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Represents a class that interacts with the nopCommerce product API.
    /// </summary>
    public class ProductNopCommerce : IProduct
    {
        /// <summary>
        /// Gets the service key name for the product API.
        /// </summary>
        public string ServiceKeyName { get => "Product"; }

        private IProductService _productApi { get; set; }
        private readonly IServiceProvider _serviceProvider;
        private readonly IDtoMapper _dtoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductNopCommerce"/> class.
        /// </summary>
        /// <param name="apiServices">The API configuration services.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public ProductNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider, IDtoMapper dtoMapper)
        {
            _productApi = apiServices.ProductService;
            _serviceProvider = serviceProvider;
            _dtoMapper = dtoMapper;
        }

        /// <summary>
        /// Gets a product by its ID asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product DTO.</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            return await _productApi.GetByIdAsync(productId);
        }

        /// <summary>
        /// Creates a product in nopCommerce from SubiektGT with minimal data.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="productExternal">The external product source data.</param>
        /// <param name="setService">The chosen service.</param>
        /// <returns>The list of HTTP response messages.</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<IEnumerable<HttpResponseMessage>>? CreateMinimalProductAsync(int productId, IProductSourceData productExternal, Service setService)
        {
            ProductCreateMinimalDto? product = await productExternal.GetByIdAsync(productId) ?? throw new CustomException($"Product does not exist in the source data");

            using var scope = _serviceProvider.CreateScope();
            var dataBindingService = scope.ServiceProvider.GetRequiredService<DataBinding.DataBinding>();

            if (dataBindingService.GetKeyBinding(setService, ServiceKeyName, productId.ToString()) != null)
            {
                return null;
            }

            // create product with minimal data, leave default data
            HttpResponseMessage? response = await _productApi.CreateMinimalAsync(product);


            if (response.IsSuccessStatusCode)
            {
                var productDtoString = await response.Content.ReadAsStringAsync();
                var newProductDtoFromResponse = JsonConvert.DeserializeObject<ProductDto>(productDtoString);

                // bind data between nopCommerce and external services
                dataBindingService.BindKey(newProductDtoFromResponse.Id, setService.ToString(), ServiceKeyName, productId.ToString());

                // map the product data to the product block information DTO
                var producInrmationBlockDto = _dtoMapper.Map<ProductUpdateBlockInformationDto, ProductDto>(newProductDtoFromResponse, new Dictionary<string, object> { { "Gtin", product.Gtin } });

                // update block information with Gtin
                // CreateMinimalAsync() doesn't have Gtin, so we need to update it
                var updateResponse = await _productApi.UpdateBlockInformationAsync(newProductDtoFromResponse.Id, producInrmationBlockDto);

                return new List<HttpResponseMessage> { response, updateResponse };
            }

            return new List<HttpResponseMessage> { response };
        }
    }
}
