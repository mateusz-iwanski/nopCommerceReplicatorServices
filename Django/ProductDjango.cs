using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Django
{
    /// <summary>
    /// Product source data from Django-Oscar ecommerce.
    /// </summary>
    public class ProductDjango : IProductSourceData
    {
        private DBConnector dbConnector { get; set; }

        private ITax _tax { get; set; }

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

        public ProductDjango(ITax tax)
        {
            dbConnector = new DBConnector("Django", "postgresql");

            _tax = tax;

            dbConnector.Initialize();
            return;
        }

        public async Task<IEnumerable<ProductCreateMinimalDto>>? GetAsync(string fieldName, object fieldValue, PriceLevelGT priceLevel = PriceLevelGT.tc_CenaNetto1)
        {

            List<ProductCreateMinimalDto> products = new List<ProductCreateMinimalDto>();

            var query = _productMainQuery + $" WHERE {fieldName} = '{fieldValue}';";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, async (reader) =>
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    string? name = reader.IsDBNull("title") ? null : reader.GetString(reader.GetOrdinal("title"));
                    string? sku = reader.IsDBNull("upc") ? null : reader.GetString(reader.GetOrdinal("upc"));

                    // price get from subiekt GT
                    int subiekt_gt_id = reader.GetInt32(reader.GetOrdinal("subiekt_gt_id"));

                    string? shortDesctiprion = reader.IsDBNull("description") ? null : reader.GetString(reader.GetOrdinal("description"));
                    string? supplierSymbol = reader.IsDBNull("numer_artykulu_producenta") ? null : reader.GetString(reader.GetOrdinal("numer_artykulu_producenta"));
                    string? gtin = null;
                    decimal weight = reader.IsDBNull("waga_w_kg") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("waga_w_kg"));

                    decimal width = getMeasures(id, "szerokość") ?? 0.0m;// reader.IsDBNull("tw_Szerokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Szerokosc"));
                    decimal length = getMeasures(id, "długość") ?? 0.0m;
                    decimal depth = getMeasures(id, "głębokość") ?? 0.0m;

                    decimal vatValue = reader.GetDecimal(reader.GetOrdinal("vat_Stawka"));

                    ProductCreateMinimalDto product = new ProductCreateMinimalDto
                    {
                        Name = name,
                        Sku = sku,
                        Price = 0,
                        TaxCategoryId = 0,
                        Weight = weight,
                        Length = length,
                        Width = width,
                        Height = depth,
                        Gtin = gtin,
                        ShortDescription = shortDesctiprion,
                        ManufacturerPartNumber = supplierSymbol,
                        VatValue = vatValue,
                        SubiektGtId = subiekt_gt_id
                    };

                    products.Add(product);
                }
            });

            await FillInDataByApiAsync(products, priceLevel);

            dbConnector.CloseConnection();

            return products;
        }

        /// <summary>
        /// Add data from web api to the product for ProductCreateMinimalDto.
        /// </summary>
        /// <remarks>
        /// Some data can't be retrieved from Subiekt GT, so we need to get it from the web api.
        /// We can't do that from web api when we use DbDataReader because is not async.
        /// </remarks>
        /// <returns>Completed product data</returns>
        private async Task FillInDataByApiAsync(List<ProductCreateMinimalDto> productList, PriceLevelGT priceLevel)
        {
            var productGt = new ProductGt(_tax);

            for (int i = 0; i < productList.Count; i++)
            {
                var taxCategoryId = await _tax.GetCategoryByNameAsync((VatLevel)(int)productList[i].VatValue);
                var priceBlock = await productGt.GetProductPriceByIdAsync(productList[i].SubiektGtId ?? 0, priceLevel);
                var price = priceBlock != null ? priceBlock.Price : 0;

                productList[i] = productList[i] with { TaxCategoryId = taxCategoryId, Price = price };
            }
        }

        // Get product measures attribute value by name from django ecommerce
        private decimal? getMeasures(int productId, string productAttributeName) =>
            Decimal.TryParse(GetProductAttributeValueByName(productId, productAttributeName).Replace("mm", ""), out _) == true ?
            Decimal.Parse(GetProductAttributeValueByName(productId, productAttributeName)) : null;

        // Get product attribute value by name from django ecommerce
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
