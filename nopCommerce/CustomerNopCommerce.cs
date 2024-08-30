using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Objects.Customer;
using Refit;
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

        /// <summary>
        /// The default password is email
        /// </summary>
        [DeserializeResponse]
        public async Task<HttpResponseMessage> CreatePL(int customerId, ICustomerSourceData customerGate)
        {
            IEnumerable<CustomerCreatePLDto>? customerFromGate = customerGate.Get("kH_Id", customerId.ToString()) ?? throw new Exception($"Customer does not exist in the source data");

            var response = await _customerApi.CreatePLAsync(customerFromGate.FirstOrDefault()); 

            return response;
        }

        [DeserializeResponse]
        public async Task<IEnumerable<CustomerDto>> GetAllAsync() => await _customerApi.GetAllAsync();

        [DeserializeResponse]
        public async Task<CustomerDto> GetByIdAsync(int id) => await _customerApi.GetByIdAsync(id);

        [DeserializeResponse]
        public async Task<HttpResponseMessage> ConnectToAddressAsync(Guid customerGuid, int addressId) => await _customerApi.ConnectToAddressAsync(customerGuid, addressId);

        [DeserializeResponse]
        public async Task<CustomerDto> UpdatePLAsync(CustomerPLUpdateDto updateCustomerDto) => await _customerApi.UpdatePLAsync(updateCustomerDto);

        [DeserializeResponse]
        public async Task<HttpResponseMessage> UpdatePasswordAsync(Guid customerGuid, string newPassword) => await _customerApi.UpdatePasswordAsync(customerGuid, new Password(newPassword));
    }
}
