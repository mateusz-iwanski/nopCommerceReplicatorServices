﻿using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Objects.Product;

namespace nopCommerceReplicatorServices.Actions
{
    /// <summary>
    /// This interface is used to implement product actions for target data (e.g. ProductNopCommerce).
    /// </summary>
    public interface IProduct
    {
        /// <summary>
        /// Gets the service key name for the product API.
        /// </summary>
        string ServiceKeyName { get; }

        /// <summary>
        /// Creates a product in nopCommerce from SubiektGT with minimal data.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="productExternal">The external product source data.</param>
        /// <param name="setService">The chosen service.</param>
        /// <returns>The list of HTTP response messages.</returns>
        Task<IEnumerable<HttpResponseMessage>>? CreateMinimalProductAsync(int customerId, IProductSourceData productGate, Service setService);
    }
}