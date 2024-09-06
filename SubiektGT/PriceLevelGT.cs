using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.SubiektGT
{
    /// <summary>
    /// SubiektGT default has 2 price levels, index price and retail price. 
    /// You can add more price levels in SubiektGT.
    /// </summary>
    public enum PriceLevelGT
    {
        tc_CenaNetto0, // index price
        tc_CenaNetto1, // retail price
        tc_CenaNetto2, // wholesale price
        tc_CenaNetto3, // special price
    }
}
