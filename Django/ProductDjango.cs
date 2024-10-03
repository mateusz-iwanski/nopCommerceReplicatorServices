using nopCommerceReplicatorServices.Actions;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Django
{
    public class ProductDjango : IProductSourceData
    {
        private DBConnector dbConnector { get; set; }

        private string _productMainQuery = $@"
                SELECT 
                    id, 
                    structure, 
                    is_public, 
                    upc, 
                    title, 
                    slug, 
                    description, 
                    rating, 
                    date_created, 
                    date_updated, 
                    is_discountable, 
                    cena_kartotekowa, 
                    is_new_product, 
                    sale_in_percent, 
                    subiekt_gt_id, 
                    katalog_id, 
                    parent_id, 
                    producent_id, 
                    product_class_id, 
                    zablokowany, 
                    numer_artykulu_producenta, 
                    waga_w_kg, 
                    podstawowa_jednostka_miary, 
                    opis_podstawowa_jednostka_miary, 
                    format, 
                    struktura, 
                    dluzyca_do_2_9, 
                    dluzyca_od_3, 
                    sledz_stan, 
                    termin_dostawy, 
                    hafele_api, 
                    hafele_api_catalog_link, 
                    gtv_api, 
                    gtv_api_catalog_link
                FROM public.catalogue_product";

        public ProductDjango()
        {
            dbConnector = new DBConnector("Django", "postgresql");
            dbConnector.Initialize();
            return;
        }

        public Task<IEnumerable<ProductCreateMinimalDto>>? GetAsync(string fieldName, object fieldValue)
        {

            var products = new List<ProductDto>();

            var query = _productMainQuery + $" WHERE {fieldName} = '{fieldValue}';";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    string? name = reader.IsDBNull("title") ? null : reader.GetString(reader.GetOrdinal("title"));
                    string? sku = reader.IsDBNull("upc") ? null : reader.GetString(reader.GetOrdinal("upc"));
                    
                    //decimal price = reader.GetDecimal(reader.GetOrdinal(priceLevel.ToString())); //////
                    
                    string? shortDesctiprion = reader.IsDBNull("description") ? null : reader.GetString(reader.GetOrdinal("description"));
                    string? supplierSymbol = reader.IsDBNull("numer_artykulu_producenta") ? null : reader.GetString(reader.GetOrdinal("numer_artykulu_producenta"));
                    string? gtin = null;
                    decimal weight = reader.IsDBNull("waga_w_kg") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("waga_w_kg"));

                    decimal width = GetProductAttributeValueByName(id, "szerokość");// reader.IsDBNull("tw_Szerokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Szerokosc"));
                    decimal length = reader.IsDBNull("tw_Wysokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Wysokosc"));
                    decimal depth = reader.IsDBNull("tw_Glebokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Glebokosc"));

                    decimal vatValue = reader.GetDecimal(reader.GetOrdinal("vat_Stawka"));
                }
            });
        }

        public string? GetProductAttributeValueByName(int productId, string productAttributeName)
        {

            string? attributeValue = null;

            string query = _productMainQuery +
                @$"
                    Select
                        attr.name as attribute_name,
                        attrgr.name as attribute_group_name,
                    left join catalogue_productattributevalue as attrval on attrval.product_id = catalogue_product.id
				    left join catalogue_productattribute as attr on attrval.attribute_id = attr.id 
				    left join catalogue_attributeoptiongroup as attrgr on attrgr.id = attr.option_group_id 
				    where attrgr.name like '{productAttributeName}' and catalogue_product.id = {productId};
                ";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    attributeValue = reader.IsDBNull("attribute_name") ? null : reader.GetString(reader.GetOrdinal("attribute_name"));
                }
            });

            return attributeValue;
        }
    }
}
