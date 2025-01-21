using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.NoSQLDB;
using nopCommerceReplicatorServices.Services;
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
        private readonly IDtoMapper _dtoMapper;
        private readonly INoSqlDbService _noSqlDbService;
        private readonly IProductDataBinder _productDataBinder;
        private readonly DataBinding.DataBinding _dataBinding;

        public ProductNopCommerce() { return; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductNopCommerce"/> class.
        /// </summary>
        /// <param name="apiServices">The API configuration services.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public ProductNopCommerce(IApiConfigurationServices apiServices, IDtoMapper dtoMapper, INoSqlDbService noSqlDbService, IProductDataBinder productDataBinder)
        {
            _noSqlDbService = noSqlDbService;
            _productApi = apiServices.ProductService;
            _dtoMapper = dtoMapper;
            _productDataBinder = productDataBinder;
            _dataBinding = new DataBinding.DataBinding(_noSqlDbService);
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
        /// Creates a product in nopCommerce from SubiektGT with minimal data. Product after creation is added to BindData database 
        /// for linking nopCommerce Product with the external service.
        /// </summary>
        /// <param name="externalProductId">The ID of the product from external service.</param>
        /// <param name="productExternal">The external product source data.</param>
        /// <param name="setService">The chosen service.</param>
        /// <returns>The list of HTTP response messages. Null if the data has been previously bound. Throw CustomException if product not found</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<IEnumerable<HttpResponseMessage>>? CreateMinimalProductAsync(int externalProductId, IProductSourceData productExternal, Service setService)
        {
            ProductCreateMinimalDto? product = await productExternal.GetByIdAsync(externalProductId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");

            // if product has been previously bound, do nothing
            if (await _dataBinding.GetKeyBindingByExternalIdAsync(setService, ObjectToBind.Product, externalProductId) != null)
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
                _productDataBinder.BindAsync(newProductDtoFromResponse.Id, externalProductId);

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
        /// DataBinding has to be set to replicate (IsStockReplicated) the product by external service.
        /// </summary>
        /// <param name="externalProductId">The ID of the product from external service.</param>
        /// <param name="productExternal">The external product source data.</param>   
        /// <param name="setService">The chosen service.</param>
        /// <returns>The HTTP response message. Throw UnreplicatedDataException when product not found in external service, source service or hasn't been replicated yet.</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage> UpdateProductInventoryAsync(int externalProductId, IProductSourceData productExternal, Service setService)
        {
            ProductUpdateBlockInventoryDto? productInventoryBlock = await productExternal.GetInventoryByIdAsync(externalProductId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");
            
            // get nopCommerce product id by external service product id
            var dataBinding = await  _dataBinding.GetKeyBindingByExternalIdAsync(setService, ObjectToBind.Product, externalProductId) ??
                throw new UnreplicatedDataException($"Can't find link data with nopCommerce product in DataBinding for service {setService} with external service ID: '{externalProductId}'. " +
                    $"Link data between nopCommerce and external service in DataBinding and try again.");

            // DataBinding has to be set to replicate the product by service
            if (dataBinding.IsStockReplicated == false)
            {
                throw new UnreplicatedDataException($"nopCommerce Product with ID: '{dataBinding.NopCommerceId}' is not set to be replicated in DataBinding by " +
                    $"external service '{setService.ToString()}' with product ID: '{externalProductId}'.");
            }

            ProductDto product = await GetProductByIdAsync(dataBinding.NopCommerceId) ?? throw new Exceptions.CustomException($"Product does not exist in the nopCommerce data. " +
                $"If it was deleted manually from nopCommerce, you have to manually delete it from DataBinding data.");

            // map the product data to the product block inventory DTO
            var producInventoryBlockDto = _dtoMapper.Map<ProductUpdateBlockInventoryDto, ProductDto>(product, new Dictionary<string, object> { { "StockQuantity", productInventoryBlock.StockQuantity } });

            // update inventory block with StockQuantity
            var response = await _productApi.UpdateBlockInventoryAsync(dataBinding.NopCommerceId, producInventoryBlockDto);

            return response;
        }


        /// <summary>
        /// Update product price in nopCommerce from external service.
        /// </summary>
        /// <param name="externalProductId">The ID of the product from external service.</param>
        /// <param name="productExternal">The external product source data.</param>   
        /// <param name="setService">The chosen service.</param>
        /// <returns>The HTTP response message. Throw UnreplicatedDataException when product not found in external service, source service or hasn't been replicated yet.</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage> UpdateProductPriceAsync(int externalProductId, IProductSourceData productExternal, Service setService)
        {
            // get nopCommerce product id by external service product id
            var dataBinding = await _dataBinding.GetKeyBindingByExternalIdAsync(setService, ObjectToBind.Product, externalProductId) ??
                throw new UnreplicatedDataException("Product hasn't been replicated yet.");

            ProductUpdateBlockPriceDto? productPriceBlock = await productExternal.GetProductPriceByIdAsync(externalProductId) ?? 
                throw new Exceptions.CustomException($"Product with ID - {externalProductId} does not exist in the external service data");

            var response = await _productApi.UpdateBlockPriceAsync(dataBinding.NopCommerceId, productPriceBlock);

            return response;
        }
    }
}
