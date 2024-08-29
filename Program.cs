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

        // Create a root command with the defined options
        var rootCommand = new RootCommand
        {
            repCustomerIdOption,
            shCustomerIdOption,
            helpOption
        };

        rootCommand.Description = "nopCommerce Replicator Services";

        rootCommand.SetHandler(async (int repCustomerId, int shCustomerId, bool help) =>
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

                Console.WriteLine($"Replicate customer with ID: {repCustomerId} from SubiektGT");
                
                ICustomer customerService = serviceConfigurationBuilder.GetService<ICustomer>("CustomerGT");

                var response = await customerService.CreatePLById(repCustomerId);

                var methodInfo = typeof(CustomerGT).GetMethod("CreatePLById");

                if (methodInfo == null)
                {
                    Console.WriteLine("Method 'CreatePLById' not found in 'CustomerGT'.");
                    return;
                }

                await AttributeHelper.CheckAndDeserializeResponseAsync(methodInfo, response);

                Console.WriteLine($"Response: {await response.Content.ReadAsStringAsync()}");
            }

            if (shCustomerId > 0)
            {

                Console.WriteLine($"Show customer with ID: {shCustomerId} from SubiektGT");

                ICustomer customerService = serviceConfigurationBuilder.GetService<ICustomer>("CustomerGT");

                var response = customerService.GetCustomerFromSubiekt(shCustomerId);

                Console.WriteLine($"Response: {response}");
            }

        }, repCustomerIdOption, shCustomerIdOption, helpOption);

        // Invoke the root command
        return await rootCommand.InvokeAsync(args);
    }
}
