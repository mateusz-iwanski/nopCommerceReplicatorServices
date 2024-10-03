using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class AttributeSpecificationNopCommerce
    {
        private ISpecificationAttributeService _specificationAttributerApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributerApi = apiServices.SpecificationAttributeService;
            _serviceProvider = serviceProvider;
        }
    }
}
