using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Interfaces;
using nopCommerceWebApiClient.Objects.Customer;
using nopCommerceWebApiClient.Objects.Product;
using Refit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.SubiektGT
{
    public class ProductGt : IProductSourceData
    {
        private DBConnector dbConnector { get; set; }
        private ITax _tax { get; set; }

        public ProductGt(ITax tax)
        {
            _tax = tax;

            dbConnector = new DBConnector("SubiektGTConnection", "mssql");
            dbConnector.Initialize();
            return;
        }

        //[DeserializeResponse]
        //public Task<HttpResponseMessage>? CreateWithMinimalData(int customerId, IProductSourceData productGate, Service setService);

        public async Task<ProductCreateMinimalDto>? GetById(int customerId)
        {
            var products = await Get("tw_Id", customerId.ToString());
            return products?.FirstOrDefault();
        }

        /// <summary>
        /// Gets a product by a specified field and value.
        /// </summary>
        /// <param name="fieldName">The field name to query by.</param>
        /// <param name="fieldValue">The field value to query by.</param>
        /// <param name="priceLevels">Price levels to be shown. By default this is the retail price.</param>
        /// <returns>A ProductDto object if found; otherwise, null.</returns>
        public async Task<IEnumerable<ProductCreateMinimalDto>>? Get(string fieldName, object fieldValue, PriceLevelGT priceLevel = PriceLevelGT.tc_CenaNetto1)
        {
            List<ProductCreateMinimalDto> products = new List<ProductCreateMinimalDto>();

            var vat = await _tax.GetCategoryByNameAsync((VatLevel)23);

            var query = 
                $@"
                    SELECT 
                        tw_Id,
                        tw_Nazwa,
                        tw_Symbol,
                        {priceLevel.ToString()},
                        tw_Opis,
                        tw_DostSymbol,
                        tw_PodstKodKresk,
                        tw_Masa,
                        tw_Szerokosc,
                        tw_Wysokosc,
                        tw_Glebokosc,
                        vat_Stawka
                    FROM tw__Towar 
                    INNER JOIN tw_Cena on tw_Id = tc_IdTowar  
                    INNER JOIN sl_StawkaVAT on vat_Id = tw_IdVatSp
                    WHERE 
                        tw_Zablokowany = 'false' AND 
                        tw_Usuniety = 'false' AND 
                        tw_Rodzaj = 1 AND 
                        {fieldName} = '{fieldValue}';
                ";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, async (reader) =>
            {
                while (reader.Read())
                {

                    int id = reader.GetInt32(reader.GetOrdinal("tw_Id"));
                    string? name = reader.IsDBNull("tw_Nazwa") ? null : reader.GetString(reader.GetOrdinal("tw_Nazwa"));
                    string? sku = reader.IsDBNull("tw_Symbol") ? null : reader.GetString(reader.GetOrdinal("tw_Symbol"));
                    decimal price = reader.GetDecimal(reader.GetOrdinal(priceLevel.ToString()));
                    string? shortDesctiprion = reader.IsDBNull("tw_Opis") ? null : reader.GetString(reader.GetOrdinal("tw_Opis"));
                    string? supplierSymbol = reader.IsDBNull("tw_DostSymbol") ? null : reader.GetString(reader.GetOrdinal("tw_DostSymbol"));
                    string? gtin = reader.IsDBNull("tw_PodstKodKresk") ? null : reader.GetString(reader.GetOrdinal("tw_PodstKodKresk"));
                    decimal weight = reader.IsDBNull("tw_Masa") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Masa"));
                    decimal width = reader.IsDBNull("tw_Szerokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Szerokosc"));
                    decimal length = reader.IsDBNull("tw_Wysokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Wysokosc"));
                    decimal depth = reader.IsDBNull("tw_Glebokosc") ? 0.0m : reader.GetDecimal(reader.GetOrdinal("tw_Glebokosc"));
                    decimal vatValue = reader.GetDecimal(reader.GetOrdinal("vat_Stawka"));

                    ProductCreateMinimalDto product = new ProductCreateMinimalDto
                    {
                        Name = name,
                        Sku = sku,
                        Price = price,
                        TaxCategoryId = 0,
                        Weight = weight,
                        Length = length,
                        Width = width,
                        Height = depth,
                        Gtin = gtin,
                        ShortDescription = shortDesctiprion,
                        ManufacturerPartNumber = supplierSymbol,
                        VatValue = vatValue
                    };

                    products.Add(product);
                }
            });

            dbConnector.CloseConnection();

            await FillInDataByApiAsync(products);

            return products.Count > 0 ? products : null;
        }

        /// <summary>
        /// Add data from web api to the product.
        /// </summary>
        /// <remarks>
        /// Some data can't be retrieved from Subiekt GT, so we need to get it from the web api.
        /// We can't do that from web api when we use DbDataReader because is not async.
        /// </remarks>
        /// <returns>Completed product data</returns>
        private async Task FillInDataByApiAsync(List<ProductCreateMinimalDto> productList)
        {
            for (int i = 0; i < productList.Count; i++)
            {
                var taxCategoryId = await _tax.GetCategoryByNameAsync((VatLevel)(int)productList[i].VatValue);
                productList[i] = productList[i] with { TaxCategoryId = taxCategoryId };
            }
        }
    }
}
