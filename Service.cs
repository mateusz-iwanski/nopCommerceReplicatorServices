using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// Represents the service to replicate data from.
    /// 
    /// In Service value 0 has the main service which is used to create and link products/clients etc. 
    /// with nopCommerce. Link data you can find in DataBinding database.
    /// </summary>
    public enum Service
    {
        SubiektGT = 0,
        Django,
    }
}
