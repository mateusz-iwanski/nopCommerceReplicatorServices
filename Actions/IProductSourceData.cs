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
    /// <summary>
    /// This interface is used to implement _source actions for external services with source data (e.g. ProductGt).
    /// </summary>
    public interface IProductSourceData
    {        
        Task<ProductCreateMinimalDto>? GetByIdAsync(int customerId);        
    }
}
