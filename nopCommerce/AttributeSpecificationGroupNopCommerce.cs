using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttributeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class AttributeSpecificationGroupNopCommerce
    {
        private ISpecificationAttributeGroupService _specificationAttributeGroupApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationGroupNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributeGroupApi = apiServices.SpecificationAttributeGroupService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Get a spec attribute group by name
        /// </summary>
        /// <param name="name">name of attribute option</param>
        /// <returns>If exists SpecificationAttributeOptionDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeGroupDto>? GetByNameAsync(string name)
        {
            var allSpecAttrGroup = await _specificationAttributeGroupApi.GetAllAsync();

            return allSpecAttrGroup.FirstOrDefault(x => x.Name == name);
        }
    }
}
