using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class ExternalCustomerDisplayOptions
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ExternalCustomerDisplayOptions(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task ShowCustomerAsync(string serviceToReplicate, int shCustomerId, bool showDetailsOption)
        {
            Console.WriteLine($"Show customer with ID: {shCustomerId}.");

            // get customer service which is marked for replication
            var customerService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Customer") ??
                    throw new CustomException($"In configuration Service->{serviceToReplicate}->Customer not exists"); 

            using (var scope = _serviceProvider.CreateScope())
            {
                ICustomerSourceData customerDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, ICustomerSourceData>>()(customerService);

                var customerDto = customerDataSourceService.GetById(shCustomerId);

                if (customerDto != null)
                    Console.WriteLine($"Response: {customerDto.ToString()}");
            }
        }
    }
}
