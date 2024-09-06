using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddDbContext<KeyBindingDbContext>();            

            services.AddScoped<CustomerNopCommerce>();
            services.AddScoped<ProductNopCommerce>();            

            services.AddScoped<CustomerGT>();
            services.AddScoped<CustomerDjango>();

            services.AddScoped<ProductGt>();

            services.AddScoped<ITax, TaxNopCommerce>();

            services.AddScoped<DataBinding.DataBinding>();



            services.AddScoped<IApiConfigurationServices, ApiConfigurationServices>();

            

            // nopCommerce services
            services.AddScoped<Func<string, ICustomer>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerNopCommerce" => serviceProvider.GetService<CustomerNopCommerce>() as ICustomer,                    
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });

            // source services
            services.AddScoped<Func<string, ICustomerSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerGT" => serviceProvider.GetService<CustomerGT>() as ICustomerSourceData,
                    "CustomerDjango" => serviceProvider.GetService<CustomerDjango>() as ICustomerSourceData,
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });

            services.AddScoped<Func<string, IProductSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "ProductGT" => serviceProvider.GetService<ProductGt>() as IProductSourceData,
                    //"ProductDjango" => serviceProvider.GetService<ProductDjango>() as IProductSourceData,
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });
        }
    }
}
