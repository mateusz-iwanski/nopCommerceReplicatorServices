using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using nopCommerceReplicatorServices.CommandOptions;
using nopCommerceReplicatorServices;
using System.CommandLine;
using System.CommandLine.Binding;
using Microsoft.Extensions.DependencyInjection;

internal partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var serviceProvider = host.Services;

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Define the command-line options
        var repCustomerIdOption = new Option<int>("--replicate_customer", "The client ID from the external service to be replicated.");
        var shCustomerIdOption = new Option<int>("--show_service_customer", "The customer ID from external service that is to be show.");
        var shProductIdOption = new Option<int>("--show_service_product", "The product ID from external service that is to be show.");
        var repProductIdOption = new Option<int>("--replicate_product", "The product ID from the external service to be replicated.");
        var repInventoryProductIdOption = new Option<int>("--replicate_product_inventory", "The product ID from the external service to be replicated.");
        var repAttributeSpecificationProductIdOption = new Option<int>("--replicate_product_attribute", "The product ID from the external service to be replicated.");
        var helpOption = new Option<bool>("--help", "Show help information");
        var showDetailsOption = new Option<bool>("--show_details", "Show details output");
        var serviceToReplicate = new Option<string>("--external_service", $"Set external service to use. Available services - {string.Join(',', Enum.GetNames(typeof(Service)))}");

        // Create a root command with the defined options
        var rootCommand = new RootCommand
        {
            serviceToReplicate,
            repCustomerIdOption,
            shCustomerIdOption,
            shProductIdOption,
            repProductIdOption,
            repInventoryProductIdOption,
            repAttributeSpecificationProductIdOption,
            helpOption,
            showDetailsOption
        };

        rootCommand.Description = "nopCommerce Replicator Service";

        rootCommand.SetHandler(
            async (CommandArguments args) =>
            {
                await HandleCommand(args, serviceProvider, configuration, rootCommand);
            },
            new CommandArgumentsBinder(
                repCustomerIdOption,
                shCustomerIdOption,
                helpOption,
                showDetailsOption,
                serviceToReplicate,
                shProductIdOption,
                repProductIdOption,
                repInventoryProductIdOption,
                repAttributeSpecificationProductIdOption
            )
        );

        // Invoke the root command
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleCommand(
        CommandArguments args,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        RootCommand rootCommand
    )
    {
        Service service;

        // if an external service is selected, check if it exists in the list of enum services
        if (string.IsNullOrEmpty(args.ServiceToReplicate))
        {
            Console.WriteLine($"Invalid service to replicate: {args.ServiceToReplicate}. Use --external_service to set up a service for replication.");
            return;
        }

        if (!Enum.TryParse<Service>(args.ServiceToReplicate, out service))
        {
            Console.WriteLine($"Invalid service to replicate: {args.ServiceToReplicate}. Use --external_service to set up a service for replication.");
            return;
        }

        // show help
        if (args.Help)
        {
            rootCommand.Invoke("-h");
            return;
        }

        // replicate customer data from external service
        // service has to be marked
        if (args.RepCustomerId > 0)
        {
            var commandOption = new CustomerReplicatorOptions();
            await commandOption.ReplicateCustomerAsync(args.ServiceToReplicate, serviceProvider, configuration, args.RepCustomerId, args.ShowDetailsOption);
        }

        // show customer data from external service 
        // service has to be marked
        if (args.ShCustomerId > 0)
        {
            var commandOption = new ExternalCustomerDisplayOptions();
            await commandOption.ShowCustomerAsync(args.ServiceToReplicate, serviceProvider, configuration, args.ShCustomerId, args.ShowDetailsOption);
        }

        // show product data from external service
        if (args.ShProductIdOption > 0)
        {
            var commandLine = new ExternalProductDisplayOptions();
            await commandLine.ShowProductAsync(args.ServiceToReplicate, serviceProvider, configuration, args.ShProductIdOption, args.ShowDetailsOption);
        }

        // replicate product data from external service
        // service has to be marked
        if (args.RepProductIdOption > 0)
        {
            var commandLine = new ProductReplicatorOptions();
            await commandLine.ReplicateProductAsync(args.ServiceToReplicate, serviceProvider, configuration, args.RepProductIdOption, args.ShowDetailsOption);
        }

        // replicate product inventory from external service
        if (args.RepInventoryProductIdOption > 0)
        {
            var commandLine = new ProductReplicatorOptions();
            await commandLine.ReplicateProductInventoryAsync(args.ServiceToReplicate, serviceProvider, configuration, args.RepInventoryProductIdOption, args.ShowDetailsOption);
        }

        // replicate product attribute specification from external service
        if (args.RepAttributeSpecificationProductIdOption > 0)
        {
            var commandLine = new ProductReplicatorOptions();
            await commandLine.ReplicateProductAttributeSpecificationAsync(args.ServiceToReplicate, serviceProvider, configuration, args.RepAttributeSpecificationProductIdOption, args.ShowDetailsOption);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .ConfigureServices((_, services) =>
           {
               var startup = new Startup();
               startup.ConfigureServices(services);
           });
}
