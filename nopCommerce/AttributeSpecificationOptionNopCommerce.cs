using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class AttributeSpecificationOptionNopCommerce
    {
        private ISpecificationAttributeOptionService _specificationAttributeOptionApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationOptionNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributeOptionApi = apiServices.SpecificationAttributeOptionService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Get a spec attribute option by name
        /// </summary>
        /// <param name="name">name of attribute option</param>
        /// <returns>If exists SpecificationAttributeOptionDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeOptionDto>? GetByNameAsync(string name)
        {
            var allSpecAttrOption =  await _specificationAttributeOptionApi.GetAllAsync();
            
            return allSpecAttrOption.FirstOrDefault(x => x.Name == name);
        }
    }
}
