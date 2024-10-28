using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
namespace nopCommerceReplicatorServices.DataBinding
{
    /// <summary>
    /// Associate a data from an external website with the nopCommerce data
    /// </summary>
    /// <remarks>
    /// For example:
    /// When adding a new customer to nopCommerce - adding a new KeyBinding object to the database.
    /// When you update a client in nopCommerce - look for the BindendObject, read the NopCommerceId and update it
    /// </remarks>
    public class DataBindingEntity
    {
        public int Id { get; set; }

        // for ex. SubiektGT, Django, etc.
        public Service Service { get; set; }

        // for ex. customer, product, etc.  
        public ObjectToBind BindendObject { get; set; }

        // customer id, customer symbol, etc. (unique with ServiceName)
        public int ExternalId { get; set; }

        // nopCommerce customer id
        public int NopCommerceId { get; set; }

        // if true replicate stock by product ExternalId from service ServiceName to nopCommerce
        public bool IsStockReplicated { get; set; }
    }
}
