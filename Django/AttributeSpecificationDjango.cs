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

        /// <summary>
        /// Get attribute by product ID
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>List of AttributeSpecificationMapperDto</returns>
        public IEnumerable<AttributeSpecificationMapperDto>? Get(int productId)
        {

            var attributeSpecfificationList = new List<AttributeSpecificationMapperDto>();

            var query = $@"
                    SELECT 
                        catalogue_product.id,
                        attr.name as ""value"",
                        attrgr.name
                    FROM public.catalogue_product
                    FULL JOIN catalogue_productattributevalue as attrval on attrval.product_id = catalogue_product.id
                    FULL JOIN catalogue_productattribute as attr on attrval.attribute_id = attr.id
                    FULL JOIN catalogue_attributeoptiongroup as attrgr on attrgr.id = attr.option_group_id
                    WHERE value_boolean = true and catalogue_product.id = {productId};
            ";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var attributeSpecfification = new AttributeSpecificationMapperDto(
                        "Product", 
                        reader.GetString(reader.GetOrdinal("value")), 
                        reader.GetString(reader.GetOrdinal("name"))
                        );

                    attributeSpecfificationList.Add(attributeSpecfification);
                }
            });

            dbConnector.CloseConnection();

            return attributeSpecfificationList.Count > 0 ? attributeSpecfificationList : null;
        }
    }
}
