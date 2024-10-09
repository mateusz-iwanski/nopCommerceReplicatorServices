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
    internal class ProductReplicatorOptions
    {
        /// <summary>
        /// Creates a product in nopCommerce from SubiektGT with minimal data. 
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the inventory of a product in nopCommerce asynchronously.
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        public async Task ReplicateProductInventoryAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int repProductIdOption, bool showDetailsOption)
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

                HttpResponseMessage response = await productNopCommerceService.UpdateProductInventoryAsync(repProductIdOption, productDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                Console.WriteLine($"Replicate product inventory with ID: {repProductIdOption} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");

                if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("UpdateProductInventoryAsync", response);
            }
        }

        /// <summary>
        /// Create attributes specifictaion for a product in nopCommerce asynchronously.
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        public async Task ReplicateProductAttributeSpecificationAsync(string serviceToReplicate, IServiceProvider serviceProvider, IConfiguration configuration, int repProductIdOption, bool showDetailsOption)
        {
            if (string.IsNullOrEmpty(serviceToReplicate))
            {
                Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                // get attribute specification external service which is marked for replication
                var productAttributeService = configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Attribute");

                IProductSpecificationAttributeMapping productNopCommerceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSpecificationAttributeMapping>>()("AttributeSpecificationNopCommerce");
                IAttributeSpecificationSourceData attributeDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IAttributeSpecificationSourceData>>()(productAttributeService);

                List<HttpResponseMessage> responses = await productNopCommerceService.CreateAsync(repProductIdOption, attributeDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                foreach (var response in responses)
                {
                    Console.WriteLine($"Replicate product attribute specification with ID: {repProductIdOption} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");
                    if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("UpdateProductAttributeSpecificationAsync", response);
                }
            }
        }
    }
}
