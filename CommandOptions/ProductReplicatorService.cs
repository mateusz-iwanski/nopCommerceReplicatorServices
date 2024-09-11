using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.nopCommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    internal class ProductReplicatorService
    {
        public async Task ReplicateProductAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int repProductIdOption, bool showDetailsOption)
        {
            if (string.IsNullOrEmpty(serviceToReplicate))
            {
                Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                // get customer service which is marked for replication
                var productService = configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product");

                IProduct productNopCommerceService = scope.ServiceProvider.GetRequiredService<Func<string, IProduct>>()("ProductNopCommerce");
                IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

                IEnumerable<HttpResponseMessage>? response = await productNopCommerceService.CreateMinimalProductAsync(repProductIdOption, productDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                if (response == null)
                {
                    Console.WriteLine($"Adding failed. Product with ID: {repProductIdOption} already exists in the database.");
                    return;
                }
                else
                {
                    var responseList = response.ToList();
                    Console.WriteLine($"Replicate product with ID: {repProductIdOption} --- Status code: {(int)responseList[0].StatusCode} ({responseList[0].StatusCode}).");
                    Console.WriteLine($"Update product Gtin with ID: {repProductIdOption} --- Status code: {(int)responseList[1].StatusCode} ({responseList[1].StatusCode}).");

                    if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("CreateMinimalProductAsync", responseList);
                }
            }
        }
    }
}
