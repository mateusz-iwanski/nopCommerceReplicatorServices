using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using nopCommerceReplicatorServices.NoSQLDB;
using Newtonsoft.Json;
namespace nopCommerceReplicatorServices.DataBinding
{
    /// <summary>
    /// Associate a data from an external website with the nopCommerce data
    /// </summary>
    /// <remarks>
    /// For example:
    /// When adding a new customer to nopCommerce - adding a new KeyBinding object to the database.
    /// When you update a client in nopCommerce - look for the BindedObject, read the NopCommerceId and update it
    /// </remarks>
    public class DataBindingDto : CosmosDbDtoBase
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        // in constructor set
        [JsonProperty("bindedService")]
        public string BindedService { get; set; } = string.Empty; // SubiektGT, Django, GtvApi etc.

        // in constructor set
        
        [JsonProperty("bindedObject")]
        public String BindedObject { get; set; } = string.Empty; // for ex. customer, product, etc.        

        // customer id, customer symbol, etc. (unique with ServiceName)
        [JsonProperty("externalId")]
        public int ExternalId { get; set; }


        // nopCommerce customer id
        [JsonProperty("nopCommerceId")]
        public int NopCommerceId { get; set; }

        // if true replicate stock by product ExternalId from service ServiceName to nopCommerce
        [JsonProperty("isStockReplicated")]
        public bool IsStockReplicated { get; set; }

        // if true replicate price by product ExternalId from service ServiceName to nopCommerce
        [JsonProperty("isPriceReplicated")]
        public bool IsPriceReplicated { get; set; }
        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat message content records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>

        public DataBindingDto(
            int nopCommerceId,
            string bindedService,
            string bindedObject,
            int externalId
            )
        {
            NopCommerceId = nopCommerceId;
            BindedService = bindedService;
            BindedObject = bindedObject;
            ExternalId = externalId;
        }


        public static string ContainerNameStatic() => "binded-objects-with-nopcommerce";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// This method returns the name of the partition key used to partition chat message content records.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "/bindedService";
        /// <summary>
        /// Gets the container name for the Cosmos DB.
        /// This method overrides the base class method to return the specific container name for chat message content.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public override string ContainerName() => ContainerNameStatic();

        /// <summary>
        /// Gets the partition key name for the Cosmos DB.
        /// This method overrides the base class method to return the specific partition key name for chat message content.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public override string PartitionKeyName() => PartitionKeyNameStatic();

        /// <summary>
        /// Gets the partition key data for the Cosmos DB.
        /// This method returns the value of the partition key, which is the conversation UUID.
        /// </summary>
        /// <returns>The partition key data.</returns>
        public override string PartitionKeyData() => BindedService.ToString();

    }
}
