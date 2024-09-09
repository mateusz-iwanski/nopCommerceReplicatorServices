﻿using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Objects.Product;

namespace nopCommerceReplicatorServices.Actions
{
    /// <summary>
    /// This interface is used to implement product actions for target data (e.g. ProductNopCommerce).
    /// </summary>
    public interface IProduct
    {
        string ServiceKeyName { get; }
        Task<IEnumerable<HttpResponseMessage>>? CreateProductWithMinimalDataAsync(int customerId, IProductSourceData productGate, Service setService);
    }
}