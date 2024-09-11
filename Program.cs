using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nopCommerceReplicatorServices;
using nopCommerceReplicatorServices.Services;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Objects.Customer;
using nopCommerceReplicatorServices.nopCommerce;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using nopCommerceReplicatorServices.Actions;
using Microsoft.Extensions.Logging;
using nopCommerceReplicatorServices.CommandOptions;

internal partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var serviceProvider = host.Services;

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Define the command-line options
        var repCustomerIdOption = new Option<int>(
            "--replicate_customer",
            "The client ID from the external service to be replicated."
        );

        var shCustomerIdOption = new Option<int>(
            "--show_service_customer",
            "The customer ID from external service that is to be show."
        );

        var shProductIdOption = new Option<int>(
            "--show_service_product",
            "The product ID from external service that is to be show."
        );

        var repProductIdOption = new Option<int>(
            "--replicate_product",
            "The product ID from the external service to be replicated."
        );

        // Create a help option
        var helpOption = new Option<bool>(
            "--help",
            "Show help information"
        );

        // Show details output
        var showDetailsOption = new Option<bool>(
            "--show_details",
            "Show details output"
        );

        var serviceToReplicate = new Option<string>(
            "--external_service",
            $"Set external service to use. Available services - {string.Join(',', Enum.GetNames(typeof(Service)))}"
        );

        // Create a root command with the defined options
        var rootCommand = new RootCommand
                {
                    serviceToReplicate,
                    repCustomerIdOption,
                    shCustomerIdOption,
                    shProductIdOption,
                    repProductIdOption,
                    helpOption,
                    showDetailsOption
                };

        rootCommand.Description = "nopCommerce Replicator Service";

        rootCommand.SetHandler(async (int repCustomerId, int shCustomerId, bool help, bool showDetailsOption, string serviceToReplicate, int shProductIdOption, int repProductIdOption) =>
        {
            Service service;

            // if an external service is selected, check if it exists in the list of enum services
            if (!string.IsNullOrEmpty(serviceToReplicate))
            {
                if (!Enum.TryParse<Service>(serviceToReplicate, out service))
                {
                    Console.WriteLine($"Invalid service to replicate: {serviceToReplicate}");
                    return;
                }
            }

            // show help
            if (help)
            {
                rootCommand.Invoke("-h");
                return;
            }

            // replicate customer data from external service
            // service has to be marked
            if (repCustomerId > 0)
            {
                var commandOption = new CustomerReplicatorService();
                await commandOption.ReplicateCustomerAsync(serviceToReplicate, serviceProvider, configuration, repCustomerId, showDetailsOption);
            }

            // show customer data from external service 
            // service has to be marked
            if (shCustomerId > 0)
            {
               var commandOption = new ExternalCustomerDisplayService();
               await commandOption.ShowCustomerAsync(serviceToReplicate, serviceProvider, configuration, shCustomerId, showDetailsOption);
            }

            // show product data from external service
            if (shProductIdOption > 0)
            {
                var commandLine = new ExternalProductDisplayService();
                await commandLine.ShowProductAsync(serviceToReplicate, serviceProvider, configuration, shProductIdOption, showDetailsOption);
            }

            // replicate product data from external service
            // service has to be marked
            if (repProductIdOption > 0)
            {
                var commandLine = new ProductReplicatorService();
                await commandLine.ReplicateProductAsync(serviceToReplicate, serviceProvider, configuration, repProductIdOption, showDetailsOption);
            }


        }, repCustomerIdOption, shCustomerIdOption, helpOption, showDetailsOption, serviceToReplicate, shProductIdOption, repProductIdOption);

        // Invoke the root command
        return await rootCommand.InvokeAsync(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .ConfigureServices((_, services) =>
           {
               var startup = new Startup();
               startup.ConfigureServices(services);
           });
}
