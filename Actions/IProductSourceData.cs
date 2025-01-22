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
    /// This interface is used to implement product actions for external services with source data (e.g. ProductGt).
    /// </summary>
    public interface IProductSourceData
    {
        Task<List<ProductCreateMinimalDto>>? GetAsync(string fieldName, object fieldValue);
        Task<ProductCreateMinimalDto>? GetByIdAsync(int customerId);
        Task<ProductUpdateBlockInventoryDto>? GetInventoryByIdAsync(int customerId);
        Task<ProductUpdateBlockPriceDto>? GetProductPriceByIdAsync(int productId);
    }
}
