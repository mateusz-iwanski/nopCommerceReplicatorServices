using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Objects.Product;
using nopCommerceWebApiClient.Objects.ProductSpecificationAttributeMapping;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Connect attribute specyfication with product
    /// </summary>
    public class ProductSpecificationAttributeMappingNopCommerce
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProductSpecificationAttributeMappingService _productSpecificationAttributeMappingService;

        public ProductSpecificationAttributeMappingNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _productSpecificationAttributeMappingService = apiServices.ProductSpecificationAttributeMappingService;
        }

        /// <summary>
        /// Create a new specification attribute mapping with the product. If such a mapping exists 
        /// where it contains the product ID and the specification attribute option id, just return it.
        /// </summary>
        /// <remarks>
        /// Look on GetByIds, if checking over product ID and the specification attribute option ID is 
        /// not enough, change it.
        /// </remarks>
        /// <param name="ProductDto">ProductDto</param>
        /// <param name="SpecificationAttributeOptionDto">SpecificationAttributeOptionDto</param>
        /// <returns>ProductSpecificationAttributeMappingDto or throw CustomException</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<ProductSpecificationAttributeMappingDto> Create(ProductDto product, SpecificationAttributeOptionDto specificationAttributeOptionDto)
        {
            try
            {
                // if exists return
                var existing = await GetByIds(product.Id, specificationAttributeOptionDto.Id);
                if (existing != null) return existing;

                // if not exists add new
                var apiResponse = await _productSpecificationAttributeMappingService.CreateAsync(
                    new ProductSpecificationAttributeMappingCreateDto
                    {
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = specificationAttributeOptionDto.Id
                    });

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new CustomException($"Failed to link to product and specification attribute. {apiResponse.ReasonPhrase}");
                }

                return await apiResponse.Content.ReadFromJsonAsync<ProductSpecificationAttributeMappingDto>();
            }
            catch (Exception ex)
            {
                throw new CustomException($"Failed to link to product and specification attribute. {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specification attribute mapping
        /// </summary>
        /// <param name="productId">product ID</param>
        /// <param name="attributeSpecificationOptionId">attribute specification option ID</param>
        /// <returns>If exists ProductSpecificationAttributeMappingDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<ProductSpecificationAttributeMappingDto>? GetByIds(int productId, int attributeSpecificationOptionId)
        {
            var maps = await _productSpecificationAttributeMappingService.GetAllAsync();
            return maps.FirstOrDefault(x => x.ProductId == productId && x.SpecificationAttributeOptionId == attributeSpecificationOptionId);
        }
    }
}
