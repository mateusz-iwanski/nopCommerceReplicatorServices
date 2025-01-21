using Google.Api.Gax.Grpc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.NoSQLDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    /// <summary>
    /// BindAsync data between nopCommerce and external services.
    /// Data are stored in the azure cosmos db.
    /// </summary>
    /// <remarks>
    /// For example, associate a customer ID from nopCommerce with a customer ID from an external service,
    /// so that we know which nopCommerce customer is which customer on the external site or 
    /// whether the client has already been replicated from an external service.
    /// </remarks>
    public class DataBinding
    {
        private INoSqlDbService _noSqlDbService;

        public DataBinding(INoSqlDbService noSqlDbService)
        {
            _noSqlDbService = noSqlDbService;
        }

        /// <summary>
        /// BindAsync a ID from nopCommerce with a serviceName (ID, symbol etc.) from an external service.
        /// </summary>
        /// <param name="nopCommerceId">The ID from nopCommerce</param>
        /// <param name="service">The key name of the external service. For example Django, GtvApi etc.</param>  
        /// <param name="objectToBind">The name of object we want to bind. For example Customer, Product etc.</param>
        /// <param name="externalId">The value of the external service. For example - if ID - "1","2","3" ... If symbol - "u1" "sas3" ... </param>
        public async Task BindKeyAsync(int nopCommerceId, Service service, ObjectToBind objectToBind, int externalId)
        {
            var customerKeyBinding = new DataBindingDto
            (
                nopCommerceId: nopCommerceId,
                bindedService: service.ToString(),
                bindedObject: objectToBind.ToString(),
                externalId: externalId
            );

            await _noSqlDbService.CreateItemAsync(customerKeyBinding);
        }

        /// <summary>
        /// Get binding object by external service ID.
        /// </summary>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="bindedObject">The object which want to find Product, Customer, etc.</param>
        /// <param name="externalId">The ID of the external service</param>
        public async Task<List<DataBindingDto>> GetByQueryAsync(string serviceName, string bindedObject, int externalId)
        {
            var containerName = DataBindingDto.ContainerNameStatic();
            var query = $"SELECT * FROM c WHERE c.bindedService = @serviceName AND c.bindedObject = @bindedObject AND c.externalId = @externalId";
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@serviceName", serviceName)
                .WithParameter("@bindedObject", bindedObject)
                .WithParameter("@externalId", externalId);

            return await _noSqlDbService.GetByQueryAsync<DataBindingDto>(queryDefinition, containerName);
        }

        /// <summary>
        /// Get binding object by nopCommerce ID.
        /// </summary>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="bindedObject">The object which want to find Product, Customer, etc.</param>
        /// <param name="nopCommerceId">The ID of the nopCommerce</param>
        public async Task<DataBindingDto?> GetKeyBindingByNopCommerceIdAsync(Service serviceName, ObjectToBind bindedObject, int nopCommerceId)
        {
            var containerName = DataBindingDto.ContainerNameStatic();
            var query = $"SELECT * FROM c WHERE c.bindedService = @serviceName AND c.bindedObject = @bindedObject AND c.nopCommerceId = @nopCommerceId";
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@serviceName", serviceName.ToString())
                .WithParameter("@bindedObject", bindedObject.ToString())
                .WithParameter("@nopCommerceId", nopCommerceId);

            var results = await _noSqlDbService.GetByQueryAsync<DataBindingDto>(queryDefinition, containerName);
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Get binding object by external service ID.
        /// </summary>
        /// <param name="serviceName">The key name of the external service (should be the same as ICustomer.ServiceKeyName)</param>  
        /// <param name="bindedObject">The object which want to find Product, Customer, etc.</param>
        /// <param name="externalId">The ID of the external service</param>
        public async Task<DataBindingDto?> GetKeyBindingByExternalIdAsync(Service serviceName, ObjectToBind bindedObject, int externalId)
        {
            var containerName = DataBindingDto.ContainerNameStatic();
            var query = $"SELECT * FROM c WHERE c.bindedService = @serviceName AND c.bindedObject = @bindedObject AND c.externalId = @externalId";
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@serviceName", serviceName.ToString())
                .WithParameter("@bindedObject", bindedObject.ToString())
                .WithParameter("@externalId", externalId);

            var results = await _noSqlDbService.GetByQueryAsync<DataBindingDto>(queryDefinition, containerName);
            return results.FirstOrDefault();
        }
    }
}
