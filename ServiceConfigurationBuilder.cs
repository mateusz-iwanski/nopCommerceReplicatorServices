using Microsoft.Extensions.DependencyInjection;
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
    public class ServiceConfigurationBuilder
    {
        private IServiceCollection services = new ServiceCollection();
        private readonly ServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the ServiceConfigurationBuilder class.
        /// </summary>
        public ServiceConfigurationBuilder()
        {
            var services = new ServiceCollection();

            // Register services with keys
            services.AddScoped<CustomerNopCommerce>();
            services.AddScoped<CustomerGT>();

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

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Gets the service of the specified type with the given key.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="key">The key associated with the service.</param>
        /// <returns>The service instance.</returns>
        public T GetService<T>(string key) where T : class
        {
            var factory = _serviceProvider.GetService<Func<string, T>>();
            if (factory == null)
            {
                throw new InvalidOperationException($"Factory for type {typeof(T).Name} not found.");
            }
            return factory(key);
        }
    }
}
