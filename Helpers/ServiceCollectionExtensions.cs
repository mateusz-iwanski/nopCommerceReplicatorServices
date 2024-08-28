using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Helpers
{
    /// <summary>
    /// <c>AddFactory</c> universal method to register the factory in the DI container configuration builder
    /// <code>
    /// private readonly Dictionary<string, Type> _customerImplementations = new Dictionary<string, Type>
    /// {    
    ///    { "CustomerGT", typeof(CustomerGT) },
    /// };
    /// services.AddFactory<ICustomer>(_customerImplementations);
    /// 
    /// Next we can register over name of the implementation
    /// 
    /// services.AddScoped<Func<string, ICustomer>>(serviceProvider => key =>
    /// {
    ///     return key switch
    ///     {
    ///          "CustomerGT" => serviceProvider.GetService<CustomerGT>() as ICustomer,  
    ///          _ => throw new ArgumentException($"Unknown key: {key}")
    ///     };
    /// });
    /// 
    /// </code>
    /// </summary>
    /// <remarks>
    /// It's useful when we want to register to objects into the DI container that implements the same interface
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        public static void AddFactory<TInterface>(this IServiceCollection services, Dictionary<string, Type> implementations)
            where TInterface : class
        {
            foreach (var implementation in implementations.Values)
            {
                services.AddScoped(implementation);
            }

            services.AddScoped<Func<string, TInterface>>(serviceProvider => key =>
            {
                if (implementations.TryGetValue(key, out var implementationType))
                {
                    return serviceProvider.GetService(implementationType) as TInterface;
                }
                throw new ArgumentException($"Unknown key: {key}");
            });
        }
    }
}
