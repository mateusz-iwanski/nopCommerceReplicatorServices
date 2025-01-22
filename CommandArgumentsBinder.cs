using nopCommerceReplicatorServices;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Diagnostics;

public class CommandArgumentsBinder : BinderBase<CommandArguments>
{
    private readonly Option<int> _repCustomerIdOption;
    private readonly Option<int> _shCustomerIdOption;
    private readonly Option<bool> _helpOption;
    private readonly Option<bool> _showDetailsOption;
    private readonly Option<string> _serviceToReplicate;
    private readonly Option<int> _shProductIdOption;
    private readonly Option<int> _repProductIdOption;
    private readonly Option<int> _repInventoryProductIdOption;
    private readonly Option<int> _repAttributeSpecificationProductIdOption;
    private readonly Option<int> _repProducPricetIdOption;

    public CommandArgumentsBinder(
        Option<int> repCustomerIdOption,
        Option<int> shCustomerIdOption,
        Option<bool> helpOption,
        Option<bool> showDetailsOption,
        Option<string> serviceToReplicate,
        Option<int> shProductIdOption,
        Option<int> repProductIdOption,
        Option<int> repInventoryProductIdOption,
        Option<int> repAttributeSpecificationProductIdOption,
        Option<int> repProducPricetIdOption)
    {
        _repCustomerIdOption = repCustomerIdOption;
        _shCustomerIdOption = shCustomerIdOption;
        _helpOption = helpOption;
        _showDetailsOption = showDetailsOption;
        _serviceToReplicate = serviceToReplicate;
        _shProductIdOption = shProductIdOption;
        _repProductIdOption = repProductIdOption;
        _repInventoryProductIdOption = repInventoryProductIdOption;
        _repAttributeSpecificationProductIdOption = repAttributeSpecificationProductIdOption;
        _repProducPricetIdOption = repProducPricetIdOption;
    }

    protected override CommandArguments GetBoundValue(BindingContext bindingContext)
    {
        // Log the values from the bindingContext
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_repCustomerIdOption): {bindingContext.ParseResult.GetValueForOption(_repCustomerIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_shCustomerIdOption): {bindingContext.ParseResult.GetValueForOption(_shCustomerIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_helpOption): {bindingContext.ParseResult.GetValueForOption(_helpOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_showDetailsOption): {bindingContext.ParseResult.GetValueForOption(_showDetailsOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_serviceToReplicate): {bindingContext.ParseResult.GetValueForOption(_serviceToReplicate)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_shProductIdOption): {bindingContext.ParseResult.GetValueForOption(_shProductIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_repProductIdOption): {bindingContext.ParseResult.GetValueForOption(_repProductIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_repInventoryProductIdOption): {bindingContext.ParseResult.GetValueForOption(_repInventoryProductIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_repAttributeSpecificationProductIdOption): {bindingContext.ParseResult.GetValueForOption(_repAttributeSpecificationProductIdOption)}");
        Debug.WriteLine($"bindingContext.ParseResult.GetValueForOption(_repProducPricetIdOption): {bindingContext.ParseResult.GetValueForOption(_repProducPricetIdOption)}");

        var rpProductIdOption = bindingContext.ParseResult.GetValueForOption(_repProductIdOption);

        var args = new CommandArguments
        {
            RepCustomerId = bindingContext.ParseResult.GetValueForOption(_repCustomerIdOption),
            ShCustomerId = bindingContext.ParseResult.GetValueForOption(_shCustomerIdOption),
            Help = bindingContext.ParseResult.GetValueForOption(_helpOption),
            ShowDetailsOption = bindingContext.ParseResult.GetValueForOption(_showDetailsOption),
            ServiceToReplicate = bindingContext.ParseResult.GetValueForOption(_serviceToReplicate),
            ShProductIdOption = bindingContext.ParseResult.GetValueForOption(_shProductIdOption),
            RepProductIdOption = rpProductIdOption,
            RepInventoryProductIdOption = bindingContext.ParseResult.GetValueForOption(_repInventoryProductIdOption),
            RepAttributeSpecificationProductIdOption = bindingContext.ParseResult.GetValueForOption(_repAttributeSpecificationProductIdOption),
            RepProducPricetIdOption = bindingContext.ParseResult.GetValueForOption(_repProducPricetIdOption)
        };

        // Log the parsed arguments for debugging
        Debug.WriteLine($"RepCustomerId: {args.RepCustomerId}");
        Debug.WriteLine($"ShCustomerId: {args.ShCustomerId}");
        Debug.WriteLine($"Help: {args.Help}");
        Debug.WriteLine($"ShowDetailsOption: {args.ShowDetailsOption}");
        Debug.WriteLine($"ServiceToReplicate: {args.ServiceToReplicate}");
        Debug.WriteLine($"ShProductIdOption: {args.ShProductIdOption}");
        Debug.WriteLine($"RepProductIdOption: {args.RepProductIdOption}");
        Debug.WriteLine($"RepInventoryProductIdOption: {args.RepInventoryProductIdOption}");
        Debug.WriteLine($"RepAttributeSpecificationProductIdOption: {args.RepAttributeSpecificationProductIdOption}");
        Debug.WriteLine($"RepProducPricetIdOption: {args.RepProducPricetIdOption}");
        
        return args;
    }
}

