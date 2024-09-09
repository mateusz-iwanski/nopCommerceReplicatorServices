using Azure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using nopCommerceReplicatorServices.Actions;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductNopCommerce"/> class.
        /// </summary>
        /// <param name="apiServices">The API configuration services.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public ProductNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _productApi = apiServices.ProductService;
            _serviceProvider = serviceProvider;
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
        public async Task<IEnumerable<HttpResponseMessage>>? CreateWithMinimalData(int productId, IProductSourceData productExternal, Service setService)
        {
            ProductCreateMinimalDto? product = await productExternal.GetByIdAsync(productId) ?? throw new Exception($"Product does not exist in the source data");

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

                // ProductCreateMinimalDto does not have a GTIN number, in the GT subiekt we have a GTIN number, so we update the information block in the product only with GTIN data
                ProductUpdateBlockInformationDto updateProductInformation = new ProductUpdateBlockInformationDto
                {
                    Id = newProductDtoFromResponse.Id,
                    ShortDescription = newProductDtoFromResponse.ShortDescription,
                    FullDescription = newProductDtoFromResponse.FullDescription,
                    ManufacturerPartNumber = newProductDtoFromResponse.ManufacturerPartNumber,
                    Published = newProductDtoFromResponse.Published,
                    Deleted = newProductDtoFromResponse.Deleted,
                    Gtin = product.Gtin,
                    ProductTypeId = newProductDtoFromResponse.ProductTypeId,
                    ProductTemplateId = newProductDtoFromResponse.ProductTemplateId,
                    VendorId = newProductDtoFromResponse.VendorId,
                    RequireOtherProducts = newProductDtoFromResponse.RequireOtherProducts,
                    RequiredProductIds = newProductDtoFromResponse.RequiredProductIds,
                    AutomaticallyAddRequiredProducts = newProductDtoFromResponse.AutomaticallyAddRequiredProducts,
                    ShowOnHomepage = newProductDtoFromResponse.ShowOnHomepage,
                    DisplayOrder = newProductDtoFromResponse.DisplayOrder,
                    ParentGroupedProductId = newProductDtoFromResponse.ParentGroupedProductId,
                    VisibleIndividually = newProductDtoFromResponse.VisibleIndividually,
                    SubjectToAcl = newProductDtoFromResponse.SubjectToAcl,
                    LimitedToStores = newProductDtoFromResponse.LimitedToStores,
                    AvailableStartDateTimeUtc = newProductDtoFromResponse.AvailableStartDateTimeUtc,
                    AvailableEndDateTimeUtc = newProductDtoFromResponse.AvailableEndDateTimeUtc,
                    MarkAsNew = newProductDtoFromResponse.MarkAsNew,
                    MarkAsNewStartDateTimeUtc = newProductDtoFromResponse.MarkAsNewStartDateTimeUtc,
                    MarkAsNewEndDateTimeUtc = newProductDtoFromResponse.MarkAsNewEndDateTimeUtc,
                    AdminComment = newProductDtoFromResponse.AdminComment,
                    UpdatedOnUtc = newProductDtoFromResponse.UpdatedOnUtc
                };

                var updateResponse = await _productApi.UpdateBlockInformationAsync(newProductDtoFromResponse.Id, updateProductInformation);

                return new List<HttpResponseMessage> { response, updateResponse };
            }

            return new List<HttpResponseMessage> { response };
        }
    }
}
