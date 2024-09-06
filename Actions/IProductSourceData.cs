using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient.Objects.Customer;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Actions
{
    internal interface IProductSourceData
    {
        Task<IEnumerable<ProductDto>>? Get(string fieldName, object fieldValue, PriceLevelGT priceLevel);
        Task<ProductDto>? GetById(int customerId);
    }
}
