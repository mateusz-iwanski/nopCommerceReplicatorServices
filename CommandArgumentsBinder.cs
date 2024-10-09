using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// Bind data to CommandArguments
    /// </summary>
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

        public CommandArgumentsBinder(
            Option<int> repCustomerIdOption,
            Option<int> shCustomerIdOption,
            Option<bool> helpOption,
            Option<bool> showDetailsOption,
            Option<string> serviceToReplicate,
            Option<int> shProductIdOption,
            Option<int> repProductIdOption,
            Option<int> repInventoryProductIdOption,
            Option<int> repAttributeSpecificationProductIdOption
        )
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
        }

        protected override CommandArguments GetBoundValue(BindingContext bindingContext)
        {
            return new CommandArguments
            {
                RepCustomerId = bindingContext.ParseResult.GetValueForOption(_repCustomerIdOption),
                ShCustomerId = bindingContext.ParseResult.GetValueForOption(_shCustomerIdOption),
                Help = bindingContext.ParseResult.GetValueForOption(_helpOption),
                ShowDetailsOption = bindingContext.ParseResult.GetValueForOption(_showDetailsOption),
                ServiceToReplicate = bindingContext.ParseResult.GetValueForOption(_serviceToReplicate),
                ShProductIdOption = bindingContext.ParseResult.GetValueForOption(_shProductIdOption),
                RepProductIdOption = bindingContext.ParseResult.GetValueForOption(_repProductIdOption),
                RepInventoryProductIdOption = bindingContext.ParseResult.GetValueForOption(_repInventoryProductIdOption),
                RepAttributeSpecificationProductIdOption = bindingContext.ParseResult.GetValueForOption(_repAttributeSpecificationProductIdOption)
            };
        }
    }
}
