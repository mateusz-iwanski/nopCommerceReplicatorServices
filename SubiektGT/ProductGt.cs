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
using System.Diagnostics;
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

        public async Task<ProductCreateMinimalDto>? GetByIdAsync(int customerId)
        {
            var products = await GetAsync("tw_Id", customerId.ToString());
            return products?.FirstOrDefault();
        }

        /// <summary>
        /// Gets a product by a specified field and value.
        /// </summary>
        /// <param name="fieldName">The field name to query by.</param>
        /// <param name="fieldValue">The field value to query by.</param>
        /// <param name="priceLevels">Price levels to be shown. By default this is the retail price.</param>
        /// <returns>A ProductDto object if found; otherwise, null.</returns>
        public async Task<IEnumerable<ProductCreateMinimalDto>>? GetAsync(string fieldName, object fieldValue, PriceLevelGT priceLevel = PriceLevelGT.tc_CenaNetto1)
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
        /// Get the product's stock quantity from Subiekt GT, set the remaining available properties as default values.
        /// </summary>
        /// <remarks>
        /// After retrieving the item quantity, set the remaining properties from the nopCommerce product you want to update.
        /// </remarks>
        /// <param name="productId">Subiekt GT product ID</param>
        /// <returns>new ProductUpdateBlockInventoryDto</returns>
        public async Task<ProductUpdateBlockInventoryDto>? GetInventoryByIdAsync(int productId)
        {
            ProductUpdateBlockInventoryDto? productUpdateBlockInventoryDto = null;

            var query =
                $@"
                    SELECT 
                        st_Stan, 
                        tw__towar.tw_Nazwa, 
                        tw__towar.tw_Symbol, 
                        tw__towar.tw_Id, 
                        tw_JednMiary, 
                        st_StanRez 
                    FROM 
                        tw__towar
                        INNER JOIN tw_Stan ON tw__towar.tw_Id = tw_Stan.st_TowId 
                        where tw__towar.tw_Id = {productId}
                ";

            dbConnector.OpenConnection();

            dbConnector.ExecuteQuery(query, async (reader) =>
            {
                while (reader.Read())
                {

                    int id = reader.GetInt32(reader.GetOrdinal("tw_Id"));
                    decimal stockQuantity = reader.GetDecimal(reader.GetOrdinal("st_Stan"));
                    decimal stockReservation = reader.GetDecimal(reader.GetOrdinal("st_StanRez"));
                    decimal availableStockQuantity = stockQuantity - stockReservation;
                    
                    // create with only StockQuantity
                    productUpdateBlockInventoryDto = new ProductUpdateBlockInventoryDto
                    {
                            ManageInventoryMethodId = 0,
                            StockQuantity = (int)availableStockQuantity,
                            ProductAvailabilityRangeId = 0,
                            UseMultipleWarehouses = false,
                            WarehouseId = 0,
                            DisplayStockAvailability = false,
                            DisplayStockQuantity = false,
                            MinStockQuantity = 0,
                            LowStockActivityId = 0,
                            NotifyAdminForQuantityBelow = 0,
                            BackorderModeId = 0,
                            AllowBackInStockSubscriptions = false,
                            OrderMinimumQuantity = 0,
                            OrderMaximumQuantity = 0,
                            NotReturnable = false,
                            AllowedQuantities = null

                    };
                }
            });

            dbConnector.CloseConnection();

            return productUpdateBlockInventoryDto;
        }
        
        /// <summary>
        /// Add data from web api to the product for ProductCreateMinimalDto.
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
