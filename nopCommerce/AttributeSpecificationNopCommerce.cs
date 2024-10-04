using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttributeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Attribute specification object. Consists of:
    /// AttributeSpecificationOption
    /// AttributeSpecificationGroup
    /// 
    /// For example: Product, Accessories, Board etc
    /// </summary>
    internal class AttributeSpecificationNopCommerce
    {
        private ISpecificationAttributeService _specificationAttributerApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributerApi = apiServices.SpecificationAttributeService;
            _serviceProvider = serviceProvider;
        }
        
        /// <summary>
        /// Get a specification attribute
        /// </summary>
        /// <param name="name">name of attribute</param>
        /// <returns>If exists SpecificationAttributeDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeDto>? GetByNameAsync(string name)
        {
            var allSpecAttrGroup = await _specificationAttributerApi.GetAllAsync();

            return allSpecAttrGroup.FirstOrDefault(x => x.Name == name);
        }

    }
}
