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
        string ServiceKeyName => "Customer";
        Task<HttpResponseMessage>? CreatePLAsync(int customerId, ICustomerSourceData customerGate, Service setService);
    }
}
