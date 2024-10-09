using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Objects.Product;
using nopCommerceWebApiClient.Objects.ProductSpecificationAttributeMapping;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Connect attribute specification with product
    /// </summary>
    public class ProductSpecificationAttributeMappingNopCommerce : IProductSpecificationAttributeMapping
    {
        public string ServiceKeyName { get => "Product"; }

        private readonly IServiceProvider _serviceProvider;
        private readonly IProductSpecificationAttributeMappingService _productSpecificationAttributeMappingService;

        public ProductSpecificationAttributeMappingNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _productSpecificationAttributeMappingService = apiServices.ProductSpecificationAttributeMappingService;
        }

        /// <summary>
        /// Create a new specification attribute mapping with the product. If such a mapping exists do nothing
        /// </summary>
        /// <remarks>
        /// Look on GetByIds, if checking over product ID and the specification attribute option ID is 
        /// not enough, change it.
        /// </remarks>
        /// <param name="productId">The ID of the product from external service</param>
        /// <param name="attributeSpecExternal">The external product attribute source dat</param>
        /// <param name="setService">The chosen service</param>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<List<HttpResponseMessage>> CreateAsync(int productId, IAttributeSpecificationSourceData attributeSpecExternal, Service setService)
        {

            List<HttpResponseMessage> httpResponses = new List<HttpResponseMessage>();

            // get nopCommerce product id by external service product id
            var dataBindingService = _serviceProvider.GetRequiredService<DataBinding.DataBinding>();
            var dataBinding = dataBindingService.GetKeyBinding(setService, ServiceKeyName, productId.ToString()) ?? throw new CustomException("Product doesn't exist in nopCOmmerce");

            var attributeSpecificationService = _serviceProvider.GetService<AttributeSpecificationNopCommerce>();

            var attributeSpecificationMapperDtoList = attributeSpecExternal.Get(productId) ?? throw new Exceptions.CustomException($"Product does not exist in the external service data");

            // one product can have multiple attribute specifications
            foreach (var attributeSpecificationMapperDto in attributeSpecificationMapperDtoList)
            {
                // create SpecificationAttribute with AttributeSpecificationOption and AttributeSpecificationGroup
                SpecificationAttributeDto attributeSpecificationDto = await attributeSpecificationService.CreateSetAsync(
                    attributeSpecificationMapperDto.GroupName,
                    attributeSpecificationMapperDto.Value,
                    attributeSpecificationMapperDto.OptionName
                    );

                // get AttributeSpecificationOption for mapping with product
                var attributeSpecificationOptionService = _serviceProvider.GetService<AttributeSpecificationOptionNopCommerce>();
                var specificationAttributeOptionDto = await attributeSpecificationOptionService.GetBySpecificationAttributeIdAsync(attributeSpecificationDto.Id) ??
                    throw new CustomException("SpecificationAttributeOption with specification attribute Id not exists");

                try
                {
                    // if exists return
                    var existing = await GetByIdsAsync(productId, specificationAttributeOptionDto.Id);
                    if (existing == null)
                    {

                        // if not exists add new
                        var apiResponse = await _productSpecificationAttributeMappingService.CreateAsync(
                            new ProductSpecificationAttributeMappingCreateDto
                            {
                                ProductId = dataBinding.NopCommerceId,
                                SpecificationAttributeOptionId = specificationAttributeOptionDto.Id,
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            });

                        if (apiResponse.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exceptions.CustomException($"Failed to link to product and specification attribute. {apiResponse.ReasonPhrase}");
                        }

                        httpResponses.Add(apiResponse);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exceptions.CustomException($"Failed to link to product and specification attribute. {ex.Message}");
                }

            }

            return httpResponses;
        }

        /// <summary>
        /// Get a specification attribute mapping
        /// </summary>
        /// <param name="productId">product ID from nopCOmmerce</param>
        /// <param name="attributeSpecificationOptionId">attribute specification option ID</param>
        /// <returns>If exists ProductSpecificationAttributeMappingDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<ProductSpecificationAttributeMappingDto>? GetByIdsAsync(int productId, int attributeSpecificationOptionId)
        {
            var maps = await _productSpecificationAttributeMappingService.GetAllAsync();
            return maps.FirstOrDefault(x => x.ProductId == productId && x.SpecificationAttributeOptionId == attributeSpecificationOptionId);
        }
    }
}
