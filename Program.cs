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

internal partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var serviceProvider = host.Services;

        // Define the command-line options
        var repCustomerIdOption = new Option<int>(
            "--replicate_subiekt_gt_customerId",
            "The customer ID in Subiekt GT that is to be replicated. If not exists add, if exists update."
        );

        var shCustomerIdOption = new Option<int>(
            "--show_service_customerId",
            "The customer ID in Subiekt GT that is to be show"
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
            "--replicate_service",
            $"Set sercice to replicate. Available services - {string.Join(',', Enum.GetNames(typeof(Service)))}"
        );

        // Create a root command with the defined options
        var rootCommand = new RootCommand
        {
            serviceToReplicate,
            repCustomerIdOption,
            shCustomerIdOption,
            helpOption,
            showDetailsOption
        };

        rootCommand.Description = "nopCommerce Replicator Services";

        rootCommand.SetHandler(async (int repCustomerId, int shCustomerId, bool help, bool showDetailsOption, string serviceToReplicate) =>
        {
            Service service;

            if (!string.IsNullOrEmpty(serviceToReplicate))
            {
                if (!Enum.TryParse<Service>(serviceToReplicate, out service))
                {
                    Console.WriteLine($"Invalid service to replicate: {serviceToReplicate}");
                    return;
                }
            }

            if (help)
            {
                rootCommand.Invoke("-h");
                return;
            }

            if (repCustomerId > 0)
            {                     
                if (string.IsNullOrEmpty(serviceToReplicate))
                {
                    Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                    return;
                }

                ICustomer customerNopCommerceService = serviceProvider.GetRequiredService<Func<string, ICustomer>>()("CustomerNopCommerce");
                ICustomerSourceData customerDataSourceService = serviceProvider.GetRequiredService<Func<string, ICustomerSourceData>>()("CustomerGT");

                HttpResponseMessage? response = await customerNopCommerceService.CreatePL(repCustomerId, customerDataSourceService, Enum.Parse<Service>(serviceToReplicate));

                if (response == null)
                {
                    Console.WriteLine($"Adding failed. Customer with ID: {repCustomerId} already exists in the database.");
                    return;
                }
                else 
                { 
                    Console.WriteLine($"Replicate customer with ID: {repCustomerId} --- Status code: {(int)response.StatusCode} ({response.StatusCode}).");

                    if (showDetailsOption) await AttributeHelper.DeserializeResponseAsync("CreatePL", response);
                }
            }

            if (shCustomerId > 0)
            {
                if (string.IsNullOrEmpty(serviceToReplicate))
                {
                    Console.WriteLine("Invalid or missing service to replicate. Use replicate_service to set up a service for replication.");
                    return;
                }

                Console.WriteLine($"Show customer with ID: {shCustomerId}.");

                ICustomerSourceData customerDataSourceService = serviceProvider.GetRequiredService<Func<string, ICustomerSourceData>>()("CustomerGT");

                var response = customerDataSourceService.Get("kH_Id", shCustomerId);

                if (response!=null)
                    foreach (var customer in response)
                        Console.WriteLine($"Response: {customer.ToString()}");
            }

        }, repCustomerIdOption, shCustomerIdOption, helpOption, showDetailsOption, serviceToReplicate);

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
