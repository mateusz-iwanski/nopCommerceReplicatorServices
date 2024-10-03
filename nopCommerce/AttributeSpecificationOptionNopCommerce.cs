using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
