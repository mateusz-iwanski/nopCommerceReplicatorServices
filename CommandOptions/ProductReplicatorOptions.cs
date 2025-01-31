﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Django;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Objects.Manufacturer;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class ProductReplicatorOptions
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ProductReplicatorOptions(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a product in nopCommerce from SubiektGT with minimal data. 
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        /// <returns></returns>
        public async Task ReplicateProductAsync(string serviceToReplicate, int repProductIdOption, bool showDetailsOption)
        {
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    // get customer service which is marked for replication
            //    var productService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product") ??
            //        throw new CustomException($"In configuration Service->{serviceToReplicate}->Product not exists"); 

            //    var productNopCommerceService = scope.ServiceProvider.GetRequiredService<ProductNopCommerce>();
            //    IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

            //    IEnumerable<HttpResponseMessage>? response = await productNopCommerceService.CreateMinimalProductAsync(repProductIdOption, productDataSourceService, Enum.Parse<Service>(serviceToReplicate));

            //    if (response == null)
            //    {
            //        Console.WriteLine($"Adding failed. Product with ID: {repProductIdOption} already exists in the database.");
            //        return;
            //    }
            //    else
            //    {
            //        var responseList = response.ToList();
            //        Console.WriteLine($"Replicate product with ID: {repProductIdOption} --- Status code: {(int)responseList[0].StatusCode} ({responseList[0].StatusCode}).");
            //        Console.WriteLine($"Update product Gtin with ID: {repProductIdOption} --- Status code: {(int)responseList[1].StatusCode} ({responseList[1].StatusCode}).");

            //        if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("CreateMinimalProductAsync", responseList);
            //    }
            //}
            DjangoDataFromSQL djFrom = new DjangoDataFromSQL(_serviceProvider);

            // ONce
            //await djFrom.O_AddCategory();
            //await djFrom.O_ManufacturerCreateDto();

            var djangoProducts = djFrom.Django_GetAllProducts();

            await djFrom.SetAllProducts();

            int count = 0;

            foreach (var productDjango in djangoProducts)
            {
                if (!await djFrom.O_ProductGetBySku(productDjango.Upc))
                {

                    try
                    {
                        count++;
                        var existing = djFrom.Django_CataloguProduct(productDjango.Id);

                        //var existsInNopCommerce = await djFrom.O_ProductExists(productDjango.Id);

                        if (existing.Id != 0)
                        {
                            await djFrom.O_ProductCreateMinimalDto(productDjango.Id);
                            if (count == 50) break;
                        }
                    }
                    catch { continue; }
                }

            }

            djFrom.closeConnection();
        }

        /// <summary>
        /// Updates the inventory of a product in nopCommerce asynchronously.
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        public async Task ReplicateProductInventoryAsync(string serviceToReplicate, int repProductIdOption, bool showDetailsOption)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // get customer service which is marked for replication
                var productService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product") ??
                    throw new CustomException($"In configuration Service->{serviceToReplicate}->Product not exists");

                var productNopCommerceService = scope.ServiceProvider.GetRequiredService<ProductNopCommerce>();
                IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

                HttpResponseMessage response = await productNopCommerceService.UpdateProductInventoryAsync(repProductIdOption, productDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                Console.WriteLine($"Replicate product inventory with ID: {repProductIdOption} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");

                if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("UpdateProductInventoryAsync", response);
            }
        }

        /// <summary>
        /// Create attributes specifictaion for a product in nopCommerce asynchronously.
        /// Product has to be unpublished.
        /// If the data has been previously bound do nothing. Throw CustomException if product not found.
        /// </summary>
        public async Task ReplicateProductAttributeSpecificationAsync(string serviceToReplicate, int repProductIdOption, bool showDetailsOption)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // get attribute specification external service which is marked for replication
                var productAttributeService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Attribute") ?? 
                    throw new CustomException($"In configuration not set Service with section {serviceToReplicate} and Attribute");

                // product api services
                var productNopCommerceService = scope.ServiceProvider.GetRequiredService<IProductService>();

                // check if product exists in the database
                var product = await productNopCommerceService.GetByIdAsync(repProductIdOption) ?? throw new CustomException($"There is no product in nopCommerce with ID: {repProductIdOption}");

                // only unpublished products can have new attributes
                if (product.Published == true)
                    throw new CustomException($"Unable to add attributes to the published product with ID: '{repProductIdOption}'. Unpublished product and try again.");

                // get attribute data source service
                IAttributeSpecificationSourceData attributeDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IAttributeSpecificationSourceData>>()(productAttributeService);

                // get product specification attribute mapping api service
                var productSpecificationAttributeMappingNopCommerce = scope.ServiceProvider.GetRequiredService<IProductSpecificationAttributeMapping>();

                // create attributes for the product
                List<HttpResponseMessage>? responses = await productSpecificationAttributeMappingNopCommerce.CreateAsync(repProductIdOption, attributeDataSourceService);
                
                if (responses == null)
                {
                    Console.WriteLine($"Attribute addition failed. External product ID: {repProductIdOption} does not have any attributes in the external service {serviceToReplicate}.");
                    return;
                }

                if (responses.Count == 0)
                {
                    Console.WriteLine($"External product ID: {repProductIdOption} has already added all attributes from the external service {serviceToReplicate} in nopCommerce.");
                    return;
                }

                foreach (var response in responses)
                {
                    Console.WriteLine($"Replicate product attribute specification with ID: {repProductIdOption} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");
                    if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductSpecificationAttributeMappingNopCommerce>("CreateAsync", response);
                }
            }
        }


        /// <summary>
        /// Updates price for a product in nopCommerce asynchronously.
        /// </summary>
        public async Task ReplicateProductPriceAsync(string serviceToReplicate, int repProductIdOption, bool showDetailsOption)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // get customer service which is marked for replication
                var productService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product") ??
                    throw new CustomException($"In configuration Service->{serviceToReplicate}->Product not exists");

                var productNopCommerceService = scope.ServiceProvider.GetRequiredService<ProductNopCommerce>();
                IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

                HttpResponseMessage response = await productNopCommerceService.UpdateProductPriceAsync(repProductIdOption, productDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                Console.WriteLine($"Replicate product price with ID: {repProductIdOption} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");

                if (showDetailsOption) await AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("UpdateProductPriceAsync", response);
            }
        }        
    }
}
