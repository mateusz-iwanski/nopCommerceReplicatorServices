using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Django;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.Services;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient;
using System;
using System.Collections.Generic;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// Represents a builder for configuring services.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the ConfigureServices class.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IConfiguration>((ConfigurationBuilder) =>
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .Build();
            });

            // Add NLog as the logging provider
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); // Clear any existing logging providers
                loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set the minimum log level

                // Add NLog
                loggingBuilder.AddNLog();
            });
            // Register NLog's ILogger
            services.AddSingleton<NLog.ILogger>(provider => NLog.LogManager.GetCurrentClassLogger());

            services.AddDbContext<KeyBindingDbContext>();            

            // Nopcommerce
            services.AddScoped<CustomerNopCommerce>();
            services.AddScoped<ProductNopCommerce>();

            services.AddScoped<AttributeSpecificationNopCommerce>();
            services.AddScoped<AttributeSpecificationOptionNopCommerce>();
            services.AddScoped<AttributeSpecificationGroupNopCommerce>();
            services.AddScoped<ProductSpecificationAttributeMappingNopCommerce>();

            services.AddScoped<ITax, TaxNopCommerce>();

            // external 
            services.AddScoped<CustomerGT>();
            services.AddScoped<CustomerDjango>();
            services.AddScoped<ProductGt>();                   

            // utils
            services.AddScoped<DataBinding.DataBinding>();
            services.AddScoped<IDtoMapper, DtoMapper>();

            // rest
            services.AddScoped<IApiConfigurationServices, ApiConfigurationServices>();

            // nopCommerce customer services
            services.AddScoped<Func<string, ICustomer>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerNopCommerce" => serviceProvider.GetService<CustomerNopCommerce>() as ICustomer,                    
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });

            // nopCommerce _source services
            services.AddScoped<Func<string, IProduct>>(serviceProvider => key =>
            {
                return key switch
                {
                    "ProductNopCommerce" => serviceProvider.GetService<ProductNopCommerce>() as IProduct,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });


            // source customer services
            services.AddScoped<Func<string, ICustomerSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerGT" => serviceProvider.GetService<CustomerGT>() as ICustomerSourceData,
                    "CustomerDjango" => serviceProvider.GetService<CustomerDjango>() as ICustomerSourceData,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });

            // source _source services
            services.AddScoped<Func<string, IProductSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "ProductGT" => serviceProvider.GetService<ProductGt>() as IProductSourceData,
                    //"ProductDjango" => serviceProvider.GetService<ProductDjango>() as IProductSourceData,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });
        }
    }
}
