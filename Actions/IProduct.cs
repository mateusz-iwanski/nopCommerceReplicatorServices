using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Objects.Product;

namespace nopCommerceReplicatorServices.Actions
{
    /// <summary>
    /// This interface is used to implement _source actions for target data (e.g. ProductNopCommerce).
    /// </summary>
    public interface IProduct
    {
        string ServiceKeyName { get; }
        Task<IEnumerable<HttpResponseMessage>>? CreateProductWithMinimalDataAsync(int customerId, IProductSourceData productGate, Service setService);
    }
}