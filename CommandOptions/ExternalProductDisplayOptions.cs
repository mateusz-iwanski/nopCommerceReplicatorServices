using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class ExternalProductDisplayOptions
    {
        public async Task ShowProductAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int shProductIdOption, bool showDetailsOption)
        {
            if (string.IsNullOrEmpty(serviceToReplicate))
            {
                Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                return;
            }

            var productService = configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product");

            using (var scope = serviceProvider.CreateScope())
            {
                IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

                var productDto = await productDataSourceService.GetByIdAsync(shProductIdOption);

                if (productDto != null)
                    Console.WriteLine($"Response: {productDto.ToString()}");
            }
        }

    }
}
