using NLog.Targets;
using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Services
{
    /// <summary>
    /// This interface is used to implement customer actions for target data (e.g. CustomerNopCommerce).
    /// </summary>
    public interface ICustomer
    {
        /// <summary>
        /// Gets the service key name for the customer API.
        /// </summary>
        string ServiceKeyName { get; }

        /// <summary>
        /// Create a Polish customer in nopCommerce from SubiektGT
        /// </summary>
        /// <remarks>
        /// Add only if not previously added.
        /// The default password is random guid.
        /// </remarks>
        /// <param name="customerId">The client ID of the external service</param>
        /// <param name="customerGate">External service with customer source data</param>
        /// <param name="setService">The service used for replication</param>
        /// <returns>Null when client added previously, HttpResponseMessage when client added</returns>
        Task<HttpResponseMessage>? CreatePLAsync(int customerId, ICustomerSourceData customerGate, Service setService);
    }
}
