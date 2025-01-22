using FirebaseManager.Firebase;
using FirebaseManager.Firestore;
using FirebaseManager.Storage;
using Google.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.CommandOptions;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Django;
using nopCommerceReplicatorServices.GtvFirebase;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.NoSQLDB;
using nopCommerceReplicatorServices.Services;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            services.AddSingleton<IConfiguration>((ConfigurationBuilder) =>
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .Build();
            });

            // Add NLog as the logging provider
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); // Clear any existing logging providers
                loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set the minimum log level

                // Add NLog
                loggingBuilder.AddNLog();
            });
            // Register NLog's ILogger
            services.AddSingleton<NLog.ILogger>(provider => NLog.LogManager.GetCurrentClassLogger());

            
            
            services.AddScoped<AttributeSpecificationNopCommerce>();
            services.AddScoped<AttributeSpecificationOptionNopCommerce>();
            services.AddScoped<AttributeSpecificationGroupNopCommerce>();
            services.AddScoped<IProductSpecificationAttributeMapping, ProductSpecificationAttributeMappingNopCommerce>();

            services.AddScoped<ITax, TaxNopCommerce>();

            // data binder 
            
            // to wyleci services.AddScoped<GtvProductDataBinder>();
            // tutaj jeszcze przyjdzie na chwile django data binser
            services.AddScoped<IProductDataBinder, SubiektGtProductDataBinder>();


            // external 
            services.AddScoped<CustomerGT>();
            services.AddScoped<ProductGt>();
            services.AddScoped<ProductDjango>();
            services.AddScoped<CustomerDjango>();
            services.AddScoped<AttributeSpecificationDjango>();

            // replicator options 
            services.AddScoped<ProductReplicatorOptions>();
            services.AddScoped<CustomerReplicatorOptions>();
            services.AddScoped<ExternalCustomerDisplayOptions>();
            services.AddScoped<ExternalProductDisplayOptions>();


            // Add Firestore services
            services.AddScoped<IFirestoreConnector, FirestoreConnector>();
            services.AddScoped<IFirestoreService, FirestoreService>();
            services.AddScoped<IFirestorageConnector, FirestorageConnector>();
            services.AddScoped<IFirestorageService, FirestorageService>();
            services.AddScoped<INoSqlDbService, AzureCosmosDbService>();

            services.AddScoped<DataBinding.DataBinding>();

            services.Configure<FirebaseSettings>(context.Configuration.GetSection("Firebase"));

            // utils
            services.AddScoped<IDtoMapper, DtoMapper>();

            // rest
            services.AddScoped<IApiConfigurationServices, ApiConfigurationServices>();

            services.AddScoped<ProductGt>();
            
            // Nopcommerce
            services.AddScoped<CustomerNopCommerce>();
            services.AddScoped<ProductNopCommerce>();


            // nopCommerce customer services
            services.AddScoped<Func<string, ICustomer>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerNopCommerce" => serviceProvider.GetService<CustomerNopCommerce>() as ICustomer,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });

            //// nopCommerce _source services
            //services.AddScoped<Func<string, IProduct>>(serviceProvider => key =>
            //{
            //    return key switch
            //    {
            //        "ProductNopCommerce" => serviceProvider.GetService<ProductNopCommerce>() as IProduct,
            //        _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
            //    };
            //});

            // source customer services
            services.AddScoped<Func<string, ICustomerSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "CustomerGT" => serviceProvider.GetService<CustomerGT>() as ICustomerSourceData,
                    "CustomerDjango" => serviceProvider.GetService<CustomerDjango>() as ICustomerSourceData,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });

            // data binder services
            //services.AddScoped<Func<string, IProductDataBinder>>(serviceProvider => key =>
            //{
            //    return key switch
            //    {
            //        //nameof(Service.GtvApi) => serviceProvider.GetService<GtvProductDataBinder>() as IProductDataBinder,
            //        nameof(Service.SubiektGT) => serviceProvider.GetService<SubiektGtProductDataBinder>() as IProductDataBinder,
            //        _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
            //    };
            //});

            // source _source services
            services.AddScoped<Func<string, IProductSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "ProductGT" => serviceProvider.GetService<ProductGt>() as IProductSourceData,
                    "ProductDjango" => serviceProvider.GetService<ProductDjango>() as IProductSourceData,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });

            services.AddScoped<Func<string, IAttributeSpecificationSourceData>>(serviceProvider => key =>
            {
                return key switch
                {
                    "AttributeSpecificationDjango" => serviceProvider.GetService<AttributeSpecificationDjango>() as IAttributeSpecificationSourceData,
                    //"ProductDjango" => serviceProvider.GetService<ProductDjango>() as IProductSourceData,
                    _ => throw new Exceptions.ArgumentException($"Unknown key: {key}")
                };
            });




        }
    }
}
