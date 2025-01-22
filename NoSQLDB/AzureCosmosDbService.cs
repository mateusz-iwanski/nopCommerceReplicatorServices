﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.NoSQLDB
{
    public class AzureCosmosDbService : CosmosDbBase, INoSqlDbService
    {
        public AzureCosmosDbService(IConfiguration configuration)
            : base(
                  configuration.GetSection("Azure:CosmosDb:Connection").Value ?? throw new ArgumentNullException("Azure:CosmosDb:Connection not exists in settings file"),
                  configuration.GetSection("Azure:CosmosDb:CosmosDbDatabaseName").Value ?? throw new ArgumentNullException("Azure:CosmosDb:CosmosDbDatabaseName"),
                  int.Parse(configuration.GetSection("Azure:CosmosDb:Throughput").Value ?? throw new ArgumentNullException("Azure:CosmosDb:CosmosDbDatabaseName"))
                  )
        {
        }
        private async Task<Container> getOrCreateContainerAsync(string containerName, string partitionKeyPath)
        {
            // partitionKeyPath must start with 
            // Ensure the partition key path starts with a leading slash

            if (!partitionKeyPath.StartsWith("/"))
            {
                throw new ArgumentException("Azure Cosmos DB partition key must start with /. Look on your DTO inheriter from CosmosDbBase.");
            }

            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
            var container = await database.Database.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath);

            return container.Container;
        }

        public async Task<ItemResponse<T>> CreateItemAsync<T>(T cosmosDto) where T : CosmosDbDtoBase
        {
            var containerName = cosmosDto.ContainerName();
            var partitionKeyName = cosmosDto.PartitionKeyName();

            await getOrCreateContainerAsync(containerName, partitionKeyName);
            var container = _cosmosClient.GetContainer(_databaseName, containerName);

            return await container.CreateItemAsync(cosmosDto, new PartitionKey(cosmosDto.PartitionKeyData()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">CosmosDbDtoBase</typeparam>
        /// <param name="id">The Cosmos item id</param>
        /// <param name="containerName">The name of the caontainer</param>
        /// <param name="partitionKeyData">The partition key for the item.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ItemResponse<T>> GetItemAsync<T>(string id, string containerName, string partitionKeyData) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            return await container.ReadItemAsync<T>(id, new PartitionKey(partitionKeyData));
        }

        /// <summary>
        /// Get items by query from a container by query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// Example query:
        /// 
        ///     var query = $"SELECT * FROM c WHERE c.uuid = @uuid";
        ///     QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@uuid", uuid);
        /// 
        /// </remarks>
        public async Task<List<T>> GetByQueryAsync<T>(QueryDefinition query, string containerName) where T : CosmosDbDtoBase
        {            
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            
            var queryResultSetIterator = container.GetItemQueryIterator<T>(query);

            var results = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the item to update.</param>
        /// <param name="item">The item to update.</param>
        /// <returns>The updated item response.</returns>
        /// <exception cref="ArgumentException">Thrown when the item or partition key (uri) is null or empty.</exception>
        public async Task<ItemResponse<T>> UpdateItemAsync<T>(T item) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, item.ContainerName());
            return await container.ReplaceItemAsync(item, item.Id.ToString());
        }

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ItemResponse<T>> DeleteItemAsync<T>(string itemId, string containerName, string partitionKeyData) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            return await container.DeleteItemAsync<T>(itemId, new PartitionKey(partitionKeyData));
        }

    }
}
