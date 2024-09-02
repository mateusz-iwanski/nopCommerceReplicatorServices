using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Services
{
    public interface ICustomerSourceData
    {
        IEnumerable<CustomerCreatePLDto>? Get(string fieldName, object fieldValue);
    }
}
