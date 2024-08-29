using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    public class CustomerNopCommerce : ICustomer
    {
        private ICustomerService _customerApi { get; set; }

        public CustomerNopCommerce(IApiConfigurationServices apiServices)
        {
            _customerApi = apiServices.CustomerService;
        }

        [DeserializeResponse]
        public async Task<HttpResponseMessage> CreatePL(int customerId, ICustomerSourceData customerGate)
        {
            CustomerCreatePLDto customerFromGate = customerGate.GetCustomer(customerId) ?? throw new Exception($"Customer with ID {customerId} does not exist in the source data");
            var response = await _customerApi.CreatePLAsync(customerFromGate);

            return response;
        }
    }
}
