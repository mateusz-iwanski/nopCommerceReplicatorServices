using FirebaseManager.Firestore;
using Google.Cloud.Firestore.V1;
using GtvApiHubnopCommerceReplicatorServices.GtvFirebase.DTOs;
using Microsoft.Extensions.Configuration;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.GtvFirebase.DTOs;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient.Interfaces;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase
{
    /// <summary>
    /// Product source data from Firebase.
    /// </summary>
    public class ProductGtvFirebase
    {
        private readonly IFirestoreService _firestoreService;
        private readonly IProductSourceData _productSourceGt;
        private readonly IConfiguration _configuration;
        private ITax _tax { get; set; }

        public ProductGtvFirebase(
            IFirestoreService firestoreService, 
            IConfiguration configuration,
            IProductSourceData productGt,
            ITax tax
            )
        {
            _firestoreService = firestoreService;
            _productSourceGt = productGt;
            _configuration = configuration;
            _tax = tax;
        }

        /// <summary>
        /// Get product from Firebase by manufacturer code.
        /// Product must be in Subiekt GT with the same manufacturer code (Item Code / tw_DostSymbol).
        /// Only change Name and FullDescription from Subiekt GT product data.
        /// </summary>
        /// <param name="manufacturerCode">Item code / tw_DostSymbol</param>
        /// <param name="priceLevel">Available Subiekt GT price level</param>
        /// <returns></returns>
        public async Task<ProductCreateMinimalDto>? GetAsync(string manufacturerCode, PriceLevelGT priceLevel, LanguageCode languageCode)
        {

            var listProductGT = await _productSourceGt.GetAsync("tw_DostSymbol", manufacturerCode);
            var productGt = listProductGT.FirstOrDefault() ?? throw new CustomException($"Can't find product by 'tw_DostSymbol' - '{manufacturerCode}' in Subiekt GT");

            // get data from firebase by item code
            FirestoreItemDto firestoreProductGtv = await _firestoreService.ReadDocumentAsync<FirestoreItemDto>(new FirestoreItemDto().CollectionName, manufacturerCode);

            ItemDto item = firestoreProductGtv.Item.Where(x => x.LanguageCode == languageCode.ToString()).FirstOrDefault() ?? 
                throw new CustomException($"Can't find product by Item Code (manufacturer code - '{manufacturerCode}' in GTV data from api");

            var descriptionFromFirestore = firestoreProductGtv.Attributes.Where(
                x => x.AttributeType == AttributeType.Description.ToString() || x.AttributeType == AttributeType.AdditionalDescription.ToString());

            StringBuilder description = new StringBuilder();
            foreach (var desc in descriptionFromFirestore)
                description.Append(desc.Value);

            var product = new ProductCreateMinimalDto
            {
                Name = item.ItemName,
                Sku = productGt.Sku,
                Price = productGt.Price,
                TaxCategoryId = productGt.TaxCategoryId,
                Weight = productGt.Weight,
                Length = productGt.Length,
                Width = productGt.Width,
                Height = productGt.Height,
                Gtin = productGt.Gtin,
                VatValue = productGt.VatValue,
                SubiektGtId = productGt.SubiektGtId,
                //FullDescription = description.ToString()
            };


            return product;
        }

        /// <summary>
        /// Get ProductCreateMinimalDto by product ID from GTV Firestore db with data from API.
        /// </summary>
        /// <param name="productId">The ID of the product from external service (GTV Api data).</param>
        /// <returns>
        /// ProductCreateMinimalDto with data from GTV api and Subiekt GT. 
        /// From GTV Api data add to product informaction about Name and FullDesription
        /// </returns>
        /// <exception cref="CustomException"></exception>
        public async Task<ProductCreateMinimalDto>? GetByIdAsync(int productId)
        {
            var usagePriceLevel = _configuration.GetSection("Service").GetSection("SubiektGT").GetValue<string>("UsagePriceLevel")
            ?? throw new CustomException("Can't read from settings Service->SubiektGT->UsagePriceLevel");

            PriceLevelGT priceLevelGT = (PriceLevelGT)Enum.Parse(typeof(PriceLevelGT), usagePriceLevel);

            // get product document by colloection name and document id (document id is productId)
            FirestoreItemDto firestoreProductGtv = await _firestoreService.ReadDocumentAsync<FirestoreItemDto>(new FirestoreItemDto().CollectionName, productId.ToString());           

            var product = await GetAsync(firestoreProductGtv.ItemCode, priceLevelGT, LanguageCode.pl) ?? throw new CustomException($"Can't find by ID {productId} in Subiekt GT");

            return product;
        }

        /// <summary>
        /// Get the product's stock quantity from GTV API data, set the remaining available properties as default values.
        /// </summary>
        /// <remarks>
        /// After retrieving the item quantity, set the remaining properties from the nopCommerce product you want to update.
        /// </remarks>
        /// <param name="productId">Subiekt GT product ID</param>
        /// <returns>new ProductUpdateBlockInventoryDto</returns>
        public async Task<ProductUpdateBlockInventoryDto>? GetInventoryByIdAsync(int productId)
        {

            FirestoreItemDto firestoreProductGtv = await _firestoreService.ReadDocumentAsync<FirestoreItemDto>(new FirestoreItemDto().CollectionName, productId.ToString());
            
            StockDto inventory = firestoreProductGtv?.Stocks?.Where(x => x.GetWarehouse == WarehouseCode.M_MLP).FirstOrDefault() ??
                throw new CustomException($"Can't find stock quantity for product with ID {productId}");

            var productUpdateBlockInventoryDto = new ProductUpdateBlockInventoryDto
            {
                ManageInventoryMethodId = 0,
                StockQuantity = inventory.InStock,
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

            return productUpdateBlockInventoryDto;
        }

        /// <summary>
        /// Get the product's price from Subiekt GT, set the remaining available properties as default values.
        /// </summary>
        /// <param name="productId">Subiekt GT product ID</param>
        /// <param name="priceLevel">Price levels to be shown. By default this is the retail price.</param>
        /// <returns></returns>
        public async Task<ProductUpdateBlockPriceDto>? GetProductPriceByIdAsync(int productId)
        {
            var productGt = new ProductGt(_tax, _configuration);
            var priceBlock = await productGt.GetProductPriceByIdAsync(productId);
            return priceBlock;
        }
    }
}
