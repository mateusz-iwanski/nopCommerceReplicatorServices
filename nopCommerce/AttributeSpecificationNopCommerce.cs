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
        private ISpecificationAttributeOptionService _specificationAttributeOptionService { get; set; }
        private ISpecificationAttributeGroupService _specificationAttributeGroupService { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationNopCommerce(
            ISpecificationAttributeService specificationAttributerApi,
            ISpecificationAttributeOptionService specificationAttributeOptionService,
            ISpecificationAttributeGroupService specificationAttributeGroupService,
            IServiceProvider serviceProvider)
        {
            _specificationAttributerApi = specificationAttributerApi;
            _specificationAttributeOptionService = specificationAttributeOptionService;
            _specificationAttributeGroupService = specificationAttributeGroupService;
            _serviceProvider = serviceProvider;
        }
    }
}
