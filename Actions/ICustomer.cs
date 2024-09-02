using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Services
{
    public interface ICustomer
    {
        string ServiceKeyName { get; } // e.g. "Customer"
        Task<HttpResponseMessage>? CreatePL(int customerId, ICustomerSourceData customerGate, Service setService);
    }
}
