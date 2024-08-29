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

internal partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        ServiceConfigurationBuilder serviceConfigurationBuilder = new ServiceConfigurationBuilder();

        // Define the command-line options
        var repCustomerIdOption = new Option<int>(
            "--replicate_subiekt_gt_customerId",
            "The customer ID in Subiekt GT that is to be replicated"
        );

        var shCustomerIdOption = new Option<int>(
            "--show_subiekt_gt_customerId",
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

        // Create a root command with the defined options
        var rootCommand = new RootCommand
        {
            repCustomerIdOption,
            shCustomerIdOption,
            helpOption,
            showDetailsOption
        };

        rootCommand.Description = "nopCommerce Replicator Services";

        rootCommand.SetHandler(async (int repCustomerId, int shCustomerId, bool help, bool showDetailsOption) =>
        {
            if (help)
            {
                rootCommand.Invoke("-h");
                return;
            }

            if (repCustomerId == 0 && shCustomerId == 0)
            {
                Console.WriteLine("Invalid or missing customer ID.");
                return;
            }

            if (repCustomerId > 0)
            {                                
                ICustomer customerNopCommerceService = serviceConfigurationBuilder.GetService<ICustomer>("CustomerNopCommerce");
                ICustomerSourceData customerDataSourceService = serviceConfigurationBuilder.GetService<ICustomerSourceData>("CustomerGT");

                var response = await customerNopCommerceService.CreatePL(repCustomerId, customerDataSourceService);

                Console.WriteLine($"Replicate customer with ID: {repCustomerId} from SubiektGT --- Status code: {(int)response.StatusCode}");

                if (showDetailsOption) await AttributeHelper.DeserializeResponseAsync("CreatePL", response);
            }

            if (shCustomerId > 0)
            {

                Console.WriteLine($"Show customer with ID: {shCustomerId} from SubiektGT");

                ICustomerSourceData customerDataSourceService = serviceConfigurationBuilder.GetService<ICustomerSourceData>("CustomerGT");

                var response = customerDataSourceService.GetCustomer(shCustomerId);

                Console.WriteLine($"Response: {response}");
            }

        }, repCustomerIdOption, shCustomerIdOption, helpOption, showDetailsOption);

        // Invoke the root command
        return await rootCommand.InvokeAsync(args);
    }
}
