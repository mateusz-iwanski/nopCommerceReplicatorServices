﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.NoSQLDB;
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
        public string ServiceKeyName { get => "Customer"; }

        private ICustomerService _customerApi { get; set; }
        private readonly INoSqlDbService _noSqlDbService;
        private readonly DataBinding.DataBinding _dataBinding;

        public CustomerNopCommerce(IApiConfigurationServices apiServices, INoSqlDbService noSqlDbService, DataBinding.DataBinding dataBinding )
        {
            _noSqlDbService = noSqlDbService;
            _customerApi = apiServices.CustomerService;
            _dataBinding = dataBinding;
        }

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
        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage>? CreatePLAsync(int customerId, ICustomerSourceData customerGate, Service setService)
        {
            CustomerDto? customerDto = customerGate.GetById(customerId) ?? throw new Exceptions.CustomException($"Customer does not exist in the source data");

            var randomPassword = Guid.NewGuid().ToString();

            if (await _dataBinding.GetKeyBindingByExternalIdAsync(setService, ObjectToBind.Customer, customerId) == null)
            {
                CustomerCreatePLDto customerCreatePLDto = new CustomerCreatePLDto
                {
                    City = customerDto.City,
                    Company = customerDto.Company,
                    County = customerDto.County,
                    Email = customerDto.Email,
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Password = randomPassword, 
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

                    await _dataBinding.BindKeyAsync(newCustomerDtoFromResponse.Id, setService, ObjectToBind.Customer, customerId);
                }

                return response;
            }
            
            return null;
        }

        [DeserializeWebApiNopCommerceResponse]
        public async Task<IEnumerable<CustomerDto>> GetAllAsync() => await _customerApi.GetAllAsync();

        [DeserializeWebApiNopCommerceResponse]
        public async Task<CustomerDto> GetByIdAsync(int id) => await _customerApi.GetByIdAsync(id);

        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage> ConnectToAddressAsync(Guid customerGuid, int addressId) => await _customerApi.ConnectToAddressAsync(customerGuid, addressId);

        [DeserializeWebApiNopCommerceResponse]
        public async Task<CustomerDto> UpdatePLAsync(CustomerPLUpdateDto updateCustomerDto) => await _customerApi.UpdatePLAsync(updateCustomerDto);

        [DeserializeWebApiNopCommerceResponse]
        public async Task<HttpResponseMessage> UpdatePasswordAsync(Guid customerGuid, string newPassword) => await _customerApi.UpdatePasswordAsync(customerGuid, new Password(newPassword));
       
    }
}
