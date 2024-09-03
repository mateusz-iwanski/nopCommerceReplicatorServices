using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Objects.Customer;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Represents a customer in nopCommerce
    /// </summary>
    public class CustomerNopCommerce : ICustomer
    {
        public string ServiceKeyName { get { return "Customer"; } }

        private ICustomerService _customerApi { get; set; }
        private readonly IServiceProvider _serviceProvider;

        public CustomerNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _customerApi = apiServices.CustomerService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a Polish customer in nopCommerce from SubiektGT
        /// </summary>
        /// <remarks>
        /// Add only if not previously added.
        /// The default password is e-mail.
        /// </remarks>
        /// <param name="customerId">The client ID of the external service</param>
        /// <param name="customerGate">External service with customer source data</param>
        /// <param name="setService">The service used for replication</param>
        /// <returns>Null when client added previously, HttpResponseMessage when client added</returns>
        [DeserializeResponse]
        public async Task<HttpResponseMessage>? CreatePL(int customerId, ICustomerSourceData customerGate, Service setService)
        {
            IEnumerable<CustomerDto>? customerFromGate = customerGate.Get("kH_Id", customerId.ToString()) ?? throw new Exception($"Customer does not exist in the source data");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dataBindingService = scope.ServiceProvider.GetRequiredService<DataBinding.DataBinding>();

                if (dataBindingService.GetKeyBinding(setService, ServiceKeyName, customerId.ToString()) == null)
                {

                    var customerDto = customerFromGate.FirstOrDefault();

                    CustomerCreatePLDto customerCreatePLDto = new CustomerCreatePLDto
                    {
                        City = customerDto.City,
                        Company = customerDto.Company,
                        County = customerDto.County,
                        Email = customerDto.Email,
                        FirstName = customerDto.FirstName,
                        LastName = customerDto.LastName,
                        Password = customerDto.Email,  // default password is email
                        Phone = customerDto.Phone,
                        StreetAddress = customerDto.StreetAddress,
                        StreetAddress2 = null,
                        Username = customerDto.Email, // default username is email
                        ZipPostalCode = customerDto.ZipPostalCode
                    };

                    HttpResponseMessage? response = await _customerApi.CreatePLAsync(customerCreatePLDto);

                    if (response.IsSuccessStatusCode)
                    {
                        var customerDtoString = await response.Content.ReadAsStringAsync();
                        var newCustomerDtoFromResponse = JsonConvert.DeserializeObject<CustomerDto>(customerDtoString);

                        dataBindingService.AddKeyBinding(newCustomerDtoFromResponse.Id, setService.ToString(), ServiceKeyName, customerId.ToString());
                    }

                    return response;
                }
            }
            
            return null;
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
