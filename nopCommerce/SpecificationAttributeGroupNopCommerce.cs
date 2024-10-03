using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class SpecificationAttributeGroupNopCommerce
    {
        private ISpecificationAttributeGroupService _specificationAttributeGroupApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public SpecificationAttributeGroupNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributeGroupApi = apiServices.SpecificationAttributeGroupService;
            _serviceProvider = serviceProvider;
        }
    }
}
