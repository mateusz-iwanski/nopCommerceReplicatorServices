using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Helpers;
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

        private readonly Dictionary<string, Type> _customerImplementations = new Dictionary<string, Type>
            {
                { "CustomerGT", typeof(CustomerGT) },
            };

        /// <summary>
        /// Initializes a new instance of the ServiceConfigurationBuilder class.
        /// </summary>
        public ServiceConfigurationBuilder()
        {
            services.AddFactory<ICustomer>(_customerImplementations);

            services.AddScoped<IApiConfigurationServices, ApiConfigurationServices>();

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

        /// <summary>
        /// Builds the service provider and registers the all implementations.
        /// </summary>
        public void Build()
        {
            services.AddScoped<Func<string, ICustomer>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerGT" => serviceProvider.GetService<CustomerGT>() as ICustomer,
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();
        }
    }
}
