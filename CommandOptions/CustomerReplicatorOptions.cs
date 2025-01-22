using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class CustomerReplicatorOptions
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public CustomerReplicatorOptions(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task ReplicateCustomerAsync(string serviceToReplicate, int repCustomerId, bool showDetailsOption)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // get customer service which is marked for replication
                var customerService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Customer") ??
                    throw new CustomException($"In configuration Service->{serviceToReplicate}->Customer not exists"); 

                ICustomer customerNopCommerceService = scope.ServiceProvider.GetRequiredService<Func<string, ICustomer>>()("CustomerNopCommerce");
                ICustomerSourceData customerDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, ICustomerSourceData>>()(customerService);

                HttpResponseMessage? response = await customerNopCommerceService.CreatePLAsync(repCustomerId, customerDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                if (response == null)
                {
                    Console.WriteLine($"Adding failed. Customer with ID: {repCustomerId} already exists in the database.");
                    return;
                }
                else
                {
                    Console.WriteLine($"Replicate customer with ID: {repCustomerId} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");

                    if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<CustomerNopCommerce>("CreatePLAsync", response);
                }
            }
        }
    }
}
