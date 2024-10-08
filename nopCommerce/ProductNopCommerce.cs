using Azure;
using Microsoft.AspNetCore.Http;
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
using System.Reflection.Metadata.Ecma335;
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
        /// Gets a product from nopCommerce by its ID asynchronously.
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
        /// <param name="productId">The ID of the product from external service.</param>
        /// <param name="productExternal">The external product source data.</param>
        /// <param name="setService">The chosen service.</param>
        /// <returns>The list of HTTP response messages. Null if the data has been previously bound. Throw CustomException if product not found</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<IEnumerable<HttpResponseMessage>>? CreateMinimalProductAsync(int productId, IProductSourceData productExternal, Service setService)
        {
            ProductCreateMinimalDto? product = await productExternal.GetByIdAsync(productId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");

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

        /// <summary>
        /// Updates the inventory of a product in nopCommerce asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product from external service.</param>
        /// <param name="productExternal">The external product source data.</param>   
        /// <param name="setService">The chosen service.</param>
        /// <returns>The HTTP response message. Throw CustomException when product not found. Null if the product in nopCommerce does not exist for the selected service</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage> UpdateProductInventoryAsync(int productId, IProductSourceData productExternal, Service setService)
        {
            using var scope = _serviceProvider.CreateScope();
            var dataBindingService = scope.ServiceProvider.GetRequiredService<DataBinding.DataBinding>();

            // get nopCommerce product id by external service product id
            var dataBinding = dataBindingService.GetKeyBinding(setService, ServiceKeyName, productId.ToString());
            // if null, product hasn't been replicated yet
            if (dataBinding == null)
            {
                return null;
            }

            ProductUpdateBlockInventoryDto? productInventoryBlock = await productExternal.GetInventoryByIdAsync(productId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");
            ProductDto product = await GetProductByIdAsync(dataBinding.NopCommerceId) ?? throw new Exceptions.CustomException($"Product does not exist in the nopCommerce data");

            // map the product data to the product block inventory DTO
            var producInventoryBlockDto = _dtoMapper.Map<ProductUpdateBlockInventoryDto, ProductDto>(product, new Dictionary<string, object> { { "StockQuantity", productInventoryBlock.StockQuantity } });

            // update inventory block with StockQuantity
            var response = await _productApi.UpdateBlockInventoryAsync(dataBinding.NopCommerceId, producInventoryBlockDto);

            return response;
        }

        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponse> UpdateProductPriceAsync(int productId, IProductSourceData productExternal, Service setService)
        {
            using var scope = _serviceProvider.CreateScope();
            var dataBindingService = scope.ServiceProvider.GetRequiredService<DataBinding.DataBinding>();

            // get nopCommerce product id by external service product id
            var dataBinding = dataBindingService.GetKeyBinding(setService, ServiceKeyName, productId.ToString());
            // if null, product hasn't been replicated yet
            if (dataBinding == null)
            {
                return null;
            }

            ProductUpdateBlockPriceDto? productPriceBlock = await productExternal.GetProductPriceByIdAsync(productId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");
        }
    }
}
