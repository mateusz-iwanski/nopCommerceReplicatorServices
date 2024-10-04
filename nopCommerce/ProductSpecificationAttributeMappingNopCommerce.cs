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
