using Google.Api.Gax.Grpc;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.nopCommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
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
    public class DataBinding : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly KeyBindingDbContext _dbContext;

        public DataBinding(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dbContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<KeyBindingDbContext>();
        }

        public KeyBindingDbContext GetDbContext()
        {
            return _dbContext;
        }

        /// <summary>
        /// Bind a ID from nopCommerce with a serviceName (ID, symbol etc.) from an external service.
        /// </summary>
        /// <param name="nopCommerceId">The ID from nopCommerce</param>
        /// <param name="service">The key name of the external service. For example Django, GtvApi etc.</param>  
        /// <param name="objectToBind">The name of object we want to bind. For example Customer, Product etc.</param>
        /// <param name="externalId">The value of the external service. For example - if ID - "1","2","3" ... If symbol - "u1" "sas3" ... </param>
        public void BindKey(int nopCommerceId, Service service, ObjectToBind objectToBind, int externalId)
        {
            var customerKeyBinding = new DataBindingEntity
            {
                NopCommerceId = nopCommerceId,
                Service = service,
                BindedObject = objectToBind,
                ExternalId = externalId
            };

            _dbContext.DataBinding.Add(customerKeyBinding);
            _dbContext.SaveChanges();
        }       

        /// <summary>
        /// Get binding object by external service ID.
        /// </summary>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="bindedObject">The object which want to find Product, Customer, etc.</param>
        /// <param name="externalId">The ID of the external service</param>
        public DataBindingEntity? GetKeyBindingByExternalId(Service serviceName, ObjectToBind bindedObject, int externalId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                return dbContext.DataBinding.FirstOrDefault(x =>
                    x.Service == serviceName &&
                    x.BindedObject == bindedObject &&
                    x.ExternalId == externalId
                );
            }
        }

        public DataBindingEntity? GetKeyBindingByNopCommerceId(Service serviceName, ObjectToBind bindedObject, int nopCommerceId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KeyBindingDbContext>();

                return dbContext.DataBinding.FirstOrDefault(x =>
                    x.Service == serviceName &&
                    x.BindedObject == bindedObject &&
                    x.NopCommerceId == nopCommerceId
                );
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
