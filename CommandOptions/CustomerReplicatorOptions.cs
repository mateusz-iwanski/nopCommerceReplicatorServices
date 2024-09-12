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
        public async Task ReplicateCustomerAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int repCustomerId, bool showDetailsOption)
        {
            if (string.IsNullOrEmpty(serviceToReplicate))
            {
                Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                // get customer service which is marked for replication
                var customerService = configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Customer");

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
