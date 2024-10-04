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

       
    }
}
