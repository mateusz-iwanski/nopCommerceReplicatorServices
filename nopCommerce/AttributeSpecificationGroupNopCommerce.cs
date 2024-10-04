using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
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

        public Task CreateSpecificationAttributeGroupAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateSpecificationAttributeGroupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
