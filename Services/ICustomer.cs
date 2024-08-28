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
        Task<HttpResponseMessage> CreatePLById(int customerId);
        Task<HttpResponseMessage> CreatePL(CustomerCreatePLDto customer);
    }
}
