using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// Arguments for Command Line
    /// </summary>
    public class CommandArguments
    {
        public int RepCustomerId { get; set; }
        public int ShCustomerId { get; set; }
        public bool Help { get; set; }
        public bool ShowDetailsOption { get; set; }
        public string ServiceToReplicate { get; set; }
        public int ShProductIdOption { get; set; }
        public int RepProductIdOption { get; set; }
        public int RepInventoryProductIdOption { get; set; }
        public int RepAttributeSpecificationProductIdOption { get; set; }
        public int RepProducPricetIdOption { get; set; }
    }   
}
