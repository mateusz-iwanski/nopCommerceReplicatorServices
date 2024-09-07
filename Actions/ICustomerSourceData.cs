using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Services
{
    /// <summary>
    /// This interface is used to implement customer actions for external services with source data (e.g. CustomerGt, CustomerDjango, ...).
    /// </summary>
    public interface ICustomerSourceData
    {
        IEnumerable<CustomerDto>? Get(string fieldName, object fieldValue);
        CustomerDto? GetById(int customerId);
    }
}
