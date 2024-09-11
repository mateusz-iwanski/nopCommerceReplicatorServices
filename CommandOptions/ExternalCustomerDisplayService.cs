﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class ExternalCustomerDisplayService
    {
        public async Task ShowCustomerAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int shCustomerId, bool showDetailsOption)
        {
            if (string.IsNullOrEmpty(serviceToReplicate))
            {
                Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                return;
            }

            Console.WriteLine($"Show customer with ID: {shCustomerId}.");

            // get customer service which is marked for replication
            var customerService = configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Customer");

            using (var scope = serviceProvider.CreateScope())
            {
                ICustomerSourceData customerDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, ICustomerSourceData>>()(customerService);

                var customerDto = customerDataSourceService.GetById(shCustomerId);

                if (customerDto != null)
                    Console.WriteLine($"Response: {customerDto.ToString()}");
            }
        }
    }
}
