using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Object is used to map attribute specification (from external source) to product
    /// </summary>
    /// <param name="GroupName">Product, Accessories, Board etc.</param>        
    /// <param name="OptionName">Color, Opening angle, etc.</param>
    /// <param name="Value">Red, Black, Left corner etc.</param>        
    public record AttributeSpecificationMapperDto(string GroupName, string OptionName, string Value);
}
