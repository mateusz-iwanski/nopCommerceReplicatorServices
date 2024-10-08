using Newtonsoft.Json.Linq;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Django
{
    /// <summary>
    /// Attribute from Django source data
    /// </summary>
    public class AttributeSpecificationDjango
    {
        private DBConnector dbConnector { get; set; }

        public AttributeSpecificationDjango()
        {
            dbConnector = new DBConnector("Django", "postgresql");
            dbConnector.Initialize();
            return;
        }

        
    }
}
