using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    public class DataBinding
    {
        IServiceProvider _serviceProvider;

        public DataBinding(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void AddKeyBinding(int nopCommerceCustomerId, string serviceName, string serviceKey)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                var customerKeyBinding = new CustomerDataBinding
                {
                    NopCommerceId = nopCommerceCustomerId,
                    ServiceName = serviceName,
                    ServiceKey = serviceKey
                };

                dbContext.Customers.Add(customerKeyBinding);
                dbContext.SaveChanges();
            }
        }

        public CustomerDataBinding? GetKeyBinding(Service serviceName, string serviceKey)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                return dbContext.Customers.FirstOrDefault(x => x.ServiceName == serviceName.ToString() && x.ServiceKey == serviceKey);
            }
        }
    }
}
