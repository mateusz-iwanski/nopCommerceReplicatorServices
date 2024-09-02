using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.DataBinding;
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
            services.AddDbContext<KeyBindingDbContext>();

            services.AddScoped<CustomerNopCommerce>();
            services.AddScoped<CustomerGT>();
            services.AddScoped<nopCommerceReplicatorServices.DataBinding.DataBinding>();

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
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });
        }
    }
}
