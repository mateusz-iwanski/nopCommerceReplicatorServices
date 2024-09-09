using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    /// <summary>
    /// Bind data between nopCommerce and external services.
    /// Data are stored in the database.
    /// </summary>
    /// <remarks>
    /// For example, associate a customer ID from nopCommerce with a customer ID from an external service,
    /// so that we know which nopCommerce customer is which customer on the external site or 
    /// whether the client has already been replicated from an external service.
    /// </remarks>
    public class DataBinding
    {
        IServiceProvider _serviceProvider;

        public DataBinding(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Bind a ID from nopCommerce with a serviceName (ID, symbol etc.) from an external service.
        /// </summary>
        /// <param name="nopCommerceCustomerId">The customer ID from nopCommerce</param>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="serviceKey">The key name of the external service. Identifier, symbol... any unique you want to use)</param>
        /// <param name="serviceValue">The value of the external service. For example - if ID - "1","2","3" ... If symbol - "u1" "sas3" ... </param>
        public void BindKey(int nopCommerceCustomerId, string serviceName, string serviceKey, string serviceValue)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                var customerKeyBinding = new DataBindingEntity
                {
                    NopCommerceId = nopCommerceCustomerId,
                    ServiceName = serviceName,
                    ServiceKey = serviceKey,
                    ServiceValue = serviceValue
                };

                dbContext.DataBinding.Add(customerKeyBinding);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get client binding information.
        /// If it isn't found, we know it hasn't been replicated yet.
        /// </summary>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="serviceKey">The key name of the external service. Identifier, symbol... any unique you used)</param>
        /// <param name="serviceValue">The value of the external service. For example - if used ID - "1","2","3" ... If used symbol - "u1" "sas3" ... </param>
        public DataBindingEntity? GetKeyBinding(Service serviceName, string serviceKey, string serviceValue)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                return dbContext.DataBinding.FirstOrDefault(x =>
                    x.ServiceName == serviceName.ToString() &&
                    x.ServiceKey == serviceKey &&
                    x.ServiceValue == serviceValue
                );
            }
        }
    }
}
