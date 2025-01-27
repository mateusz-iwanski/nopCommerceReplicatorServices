using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Objects.Category;
using nopCommerceWebApiClient.Objects.Manufacturer;
using nopCommerceWebApiClient.Objects.Picture;
using nopCommerceWebApiClient.Objects.Product;
using nopCommerceWebApiClient.Objects.ProductAvailabilityRange;
using nopCommerceWebApiClient.Objects.ProductCategory;
using nopCommerceWebApiClient.Objects.ProductManufacturer;
using nopCommerceWebApiClient.Objects.ProductPicture;
using nopCommerceWebApiClient.Objects.ProductSpecificationAttributeMapping;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttributeGroup;
using nopCommerceWebApiClient.Objects.UrlRecord;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace nopCommerceReplicatorServices.Django
{

    /// <summary>
    /// 0 - najpierw skopiować zdjęcia na bloba, później dodać zdjęcia do produktów lllllll
    /// 1 - O_AddCategory najpierw trzeba utworzyć kategorie i je ustawić ręcznie w nopCommerce display order to z django zagniezdzenie
    /// 2 - O_ManufacturerCreateDto 
    /// STOP - ustaw kategorie 
    /// 2 - O_ProductCreateMinimalDto
    /// // 3 - O_PictureCreate - pozniej nie wiem jeszcze jak
    /// 3 - O_SpecificationAttributeCreateDto  // in O_ProductCreateMinimalDto
    /// 4 - O_ProductAvailabilityRangeCreateDto  // in O_ProductCreateMinimalDto
    /// 5 - O_ProductCategoryMappingDtoCreateDto // in O_ProductCreateMinimalDto
    /// 7 - O_ProductManufacturerMappingCreateDto // in O_ProductCreateMinimalDto
    /// </summary>

    internal class DjangoDataFromSQL
    {
        private readonly DBConnector _dbConnector;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiConfigurationServices _apiServices;

        private IEnumerable<ProductDto> productNopCommerceDtos { get; set; }

        public DjangoDataFromSQL(IServiceProvider provider)
        {
            _dbConnector = new DBConnector("Django", "postgresql");
            _serviceProvider = provider;
            _dbConnector.Initialize();

            _apiServices = new ApiConfigurationServices();

            _dbConnector.OpenConnection();

            return;
        }

        public void closeConnection() => _dbConnector.CloseConnection();

        public Task SetAllProducts() => Task.Run(async () => productNopCommerceDtos = await _apiServices.ProductService.GetAllAsync());

        public static class MimeTypeHelper
        {
            private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".jfif", "image/jpeg" },
                { ".webp", "image/webp" },
                { ".bmp", "image/bmp" }
            };

            public static string GetMimeType(string filePath)
            {
                var extension = Path.GetExtension(filePath.ToLower());
                if (extension != null && MimeTypes.TryGetValue(extension, out var mimeType))
                {
                    return mimeType;
                }
                return "application/octet-stream"; // Default MIME type if not found
            }
        }


        public async Task<bool> O_ProductGetBySku(string sku)
        {
            var product = productNopCommerceDtos.FirstOrDefault(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine($"Product with SKU '{sku}' not found in nopCommerce.");
                return false;
            }

            return true;
        }


        public static string ConvertTitleToSlug(string title)
        {
            // Convert to lower case
            title = title.ToLowerInvariant();

            // Remove invalid characters
            title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces into one space
            title = Regex.Replace(title, @"\s+", " ").Trim();

            // Replace spaces with hyphens
            title = Regex.Replace(title, @"\s", "-");

            return title;
        }

        /// <summary>
        /// ad data from django picture to django db
        /// it doesn't store information about file uri
        /// </summary>
        /// <returns></returns>
        private async Task<PictureDto?> O_PictureCreate(int djangoProductId, int pictureId, string productTitle)
        {
            // Use Django_GetPicture to get picture data
            List<DjangoPicture> pictures = Django_GetPicture(djangoProductId, pictureId);
            PictureDto pictureNop = null;

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var picture in pictures)
                {
                    var pictureCreateDto = new PictureCreateDto
                    {
                        MimeType = MimeTypeHelper.GetMimeType(picture.Original), //"image/jpeg", // Assuming JPEG format, adjust as necessary
                        SeoFilename = ConvertTitleToSlug(productTitle), //Path.GetFileNameWithoutExtension(picture.Original),
                        AltAttribute = "",
                        TitleAttribute = "",
                        IsNew = false,
                        VirtualPath = ""//picture.Original // Assuming the original path is the virtual path
                    };

                    HttpResponseMessage? response = await _apiServices.PictureService.CreateAsync(pictureCreateDto);

                     if (response.IsSuccessStatusCode)
                    {
                        pictureNop = await response.Content.ReadFromJsonAsync<PictureDto>();
                        Console.WriteLine($"### Added Picture info : {pictureNop.Id} for Django picture ID: {picture.Id}");
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to add Picture info for Django picture ID: {picture.Id}");
                        return null;
                    }
                }

                return pictureNop;
            }
        }

        public async Task<bool> O_ProductPictureMappingCreateDto(int pictureId, int nopCommerceProductId, int displayOrder)
        {
            // Create the ProductPictureMappingCreateDto object
            var productPictureMappingCreateDto = new ProductPictureMappingCreateDto
            {
                PictureId = pictureId,
                ProductId = nopCommerceProductId,
                DisplayOrder = displayOrder
            };

            // Call the API to create the product picture mapping
            ProductPictureMappingDto response = await _apiServices.ProductPictureMappingService.Create(productPictureMappingCreateDto);

            if (response != null)
            {
                Console.WriteLine($"### Added ProductPictureMapping for Picture ID: {pictureId} to Product ID: {nopCommerceProductId}");
            }
            else
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"FAILED!!!!!! to add ProductPictureMapping for Picture ID: {pictureId} to Product ID: {nopCommerceProductId}");
                return false;
            }

            return true;
        }


        public async Task O_AddCategory()
        {
            List<DjanogCategory> djanogCategories = Django_GetCategory();

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var category in djanogCategories)
                {
                    var categoryCreateDto = new CategoryCreateDto
                    {
                        Name = category.Name,
                        Description = category.Description,
                        ParentCategoryId = 0,
                        CategoryTemplateId = 1,
                        PictureId = 0,
                        Published = false,
                        PageSize = 0,
                        AllowCustomersToSelectPageSize = false,
                        PageSizeOptions = "10,20,30",
                        ShowOnHomepage = false,
                        IncludeInTopMenu = false,
                        SubjectToAcl = false,
                        LimitedToStores = false,
                        DisplayOrder = category.Depth,
                        RestrictFromVendors = false
                    };

                    HttpResponseMessage? response = await _apiServices.CategoryService.CreateAsync(categoryCreateDto);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"### Added Category: {category.Name}");
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to add Category: {category.Name}");
                    }
                }
            }
        }
        private (decimal length, string attributeName) hangeAttributeDimension(int djangoId, int optionGroupId)
        {
            var (lengthString, attributeName) = Django_Attribute(djangoId, optionGroupId);
            decimal length = 0;

            if (!string.IsNullOrEmpty(lengthString))
            {
                // Check if the string contains "cm" and convert to millimeters
                if (lengthString.Contains("cm"))
                {
                    lengthString = lengthString.Replace("cm", "").Trim();
                    if (decimal.TryParse(lengthString, out decimal lengthInCm))
                    {
                        length = lengthInCm * 10; // Convert cm to mm
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to parse length in cm: {lengthString}");
                    }
                }
                // Check if the string contains "mm" and remove the unit
                else if (lengthString.Contains("mm"))
                {
                    lengthString = lengthString.Replace("mm", "").Trim();
                    if (decimal.TryParse(lengthString, out length))
                    {
                        // length is already in mm, no conversion needed
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to parse length in mm: {lengthString}");
                    }
                }
                // Check if the string contains "mm" and remove the unit
                else if (lengthString.Contains("m"))
                {
                    lengthString = lengthString.Replace("m", "").Trim();
                    if (decimal.TryParse(lengthString, out decimal lengthInM))
                    {
                        length = lengthInM * 1000; // Convert m to mm
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to parse length in mm: {lengthString}");
                    }
                }
                else if (!decimal.TryParse(lengthString, out length))
                {
                    Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                    // Handle the case where parsing fails
                    Console.WriteLine($"FAILED!!!!!! to parse length: {lengthString}");
                }
            }
            return (length, attributeName);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="djangoId"></param>
        /// <returns>nopCommerce product ID</returns>
        public async Task<int?> O_ProductCreateMinimalDto(int djangoId)
        {
            int addedNopCommerceId = 0;

            var (width, widthAttributeName) = hangeAttributeDimension(djangoId, 11);
            if (width == 0)
            {
                (width, widthAttributeName) = hangeAttributeDimension(djangoId, 14);
            }

            var (length, lengthAttributeName) = hangeAttributeDimension(djangoId, 14);
            var (height, heightAttributeName) = hangeAttributeDimension(djangoId, 10);

            DjangoCataloguProduct _django_cataloguProduct = Django_CataloguProduct(djangoId);

            (var price, var stock) = Django_ProductPriceAndStock(djangoId);

            var product = new ProductCreateMinimalDto()
            {
                Name = _django_cataloguProduct.Title,
                Sku = _django_cataloguProduct.Upc,
                Price = price,
                TaxCategoryId = 6,
                Weight = (decimal)(_django_cataloguProduct.WagaWKg ?? 0),
                Length = length,
                Width = width,
                Height = height,
                Gtin = ""
            };

            using (var scope = _serviceProvider.CreateScope())
            {
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine($"ADDING PRODUCT - {product.Sku}");
                HttpResponseMessage? response = await _apiServices.ProductService.CreateMinimalAsync(product);

                if (response.IsSuccessStatusCode)
                {
                    //_dbConnector.OpenConnection();

                    var productDto = await response.Content.ReadFromJsonAsync<ProductDto>();
                    addedNopCommerceId = productDto.Id;
                    Console.WriteLine($"### Added O_ProductCreateMinimalDto : {product.ToString()}");

                    await O_ProductPictureCreateDto(djangoId, addedNopCommerceId, product.Name);

                    await O_ProductUpdateBlockInformationDto(djangoId, productDto.Id);
                    await O_ProductUpdateBlockInventoryDto(addedNopCommerceId, stock, _django_cataloguProduct.SledzStan);
                    await O_ProductUpdateBlockReviewsDto(addedNopCommerceId);
                    await O_UrlRecordCreateDto(addedNopCommerceId, product.Name);

                    await O_SpecificationAttributeCreateDto(djangoId, productDto.Id);
                    await O_ProductAvailabilityRangeCreateDto(_django_cataloguProduct.TerminDostawy);
                    await O_ProductCategoryMappingDtoCreateDto(djangoId, productDto.Id);
                    
                    await O_ProductManufacturerMappingCreateDto(djangoId, productDto.Id);

                    //_dbConnector.CloseConnection();

                    return addedNopCommerceId;
                }
                else
                {
                    Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                    Console.WriteLine($"### Added FAILED!!!! O_ProductCreateMinimalDto: {product.ToString()}");
                    //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                }

            }
            
            return null;
        }

        private static readonly Dictionary<char, char> PolishToEnglishMap = new Dictionary<char, char>
        {
            { 'ą', 'a' }, { 'ć', 'c' }, { 'ę', 'e' }, { 'ł', 'l' },
            { 'ń', 'n' }, { 'ó', 'o' }, { 'ś', 's' }, { 'ź', 'z' },
            { 'ż', 'z' }, { 'Ą', 'A' }, { 'Ć', 'C' }, { 'Ę', 'E' },
            { 'Ł', 'L' }, { 'Ń', 'N' }, { 'Ó', 'O' }, { 'Ś', 'S' },
            { 'Ź', 'Z' }, { 'Ż', 'Z' }
        };

        private string GenerateSlug(string name)
        {
            // Convert to lower case
            name = name.ToLowerInvariant();

            // Replace Polish characters with English equivalents
            name = new string(name.Select(c => PolishToEnglishMap.ContainsKey(c) ? PolishToEnglishMap[c] : c).ToArray());

            // Remove invalid characters
            name = Regex.Replace(name, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces into one space
            name = Regex.Replace(name, @"\s+", " ").Trim();

            // Replace spaces with hyphens
            name = Regex.Replace(name, @"\s", "-");

            return name;
        }


        public async Task<bool> O_ProductPictureCreateDto(int djangoProductId, int nopCommerceProductId, string productTitle)
        {
            var productDjango = Django_GetPictureByProductId(djangoProductId);

            if (productDjango == null)
            {
                Console.WriteLine($"No pictures found for django product ID: {djangoProductId}");
                return false;
            }

            foreach (var picture in productDjango)
            {
                PictureDto nopPicture = await O_PictureCreate(djangoProductId, picture.Id, productTitle);

                // Download the image from the URL and convert it to a byte array
                byte[] binaryData;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        binaryData = await httpClient.GetByteArrayAsync(picture.Original);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download image from URL: {picture.Original}. Exception: {ex.Message}");
                    return false;
                }

                PictureBinaryCreateDto pictureBinaryCreateDto = new PictureBinaryCreateDto
                {
                    PictureId = nopPicture.Id,
                    BinaryData = binaryData
                };

                // Assuming you have a method to create the picture binary
                HttpResponseMessage? response = await _apiServices.PictureBinaryService.CreateAsync(pictureBinaryCreateDto);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"### Added Picture binary for nopPicture ID: {nopPicture.Id}");
                }
                else
                {
                    Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                    Console.WriteLine($"FAILED!!!!!! to add Picture binary for Picture nopPicture ID: {nopPicture.Id}");
                    return false;
                }

                await O_ProductPictureMappingCreateDto(nopPicture.Id, nopCommerceProductId, picture.DisplayOrder);
            }

            return true;
        }


        public async Task<bool> O_UrlRecordCreateDto(int nopCommerceId, string slugFromString)
        {
            var slug = GenerateSlug(slugFromString);

            var urlRecordUpdateDto = new UrlRecordCreateDto()
            {
                EntityId = nopCommerceId,
                EntityName = "Product",
                Slug = slug,
                LanguageId = 0,
                IsActive = true
            };

            var response = await _apiServices.UrlRecordService.CreateAsync(urlRecordUpdateDto);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"FAILED!!!!!! to add UrlRecordCreateDto : {urlRecordUpdateDto}");
                return false;
            }

            Console.WriteLine($"### Added UrlRecordCreateDto : {urlRecordUpdateDto}");

            return true;
        }

        public async Task<bool> O_ProductUpdateBlockInformationDto(int djangoId, int nopCommerceId)
        {
            var djangoProduct = Django_CataloguProduct(djangoId);

            var productUpdateBlockInformationDto = new ProductUpdateBlockInformationDto
            {
                ShortDescription = "", //djangoProduct.Description,
                FullDescription = djangoProduct.Description,
                ManufacturerPartNumber = djangoProduct.NumerArtykuluProducenta,
                Published = djangoProduct.IsPublic,
                Deleted = false, //djangoProduct.Zablokowany,
                Gtin = "", //djangoProduct.Upc,
                ProductTypeId = 5, // Assuming SimpleProduct type
                ProductTemplateId = 1, // Assuming default template
                VendorId = 0, // Assuming no vendor
                RequireOtherProducts = false, // Assuming no required products
                RequiredProductIds = null,
                AutomaticallyAddRequiredProducts = false,
                ShowOnHomepage = false,
                DisplayOrder = 0,
                ParentGroupedProductId = 0,
                VisibleIndividually = true,
                SubjectToAcl = false,
                LimitedToStores = false,
                AvailableStartDateTimeUtc = null,
                AvailableEndDateTimeUtc = null,
                MarkAsNew = false, //djangoProduct.IsNewProduct,
                MarkAsNewStartDateTimeUtc = null,
                MarkAsNewEndDateTimeUtc = null
            };

            HttpResponseMessage? response = await _apiServices.ProductService.UpdateBlockInformationAsync(nopCommerceId, productUpdateBlockInformationDto);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"### Updated ProductUpdateBlockInformationDto : {productUpdateBlockInformationDto}");
                return true;
            }
            else
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"### Updated FAILED!!!! ProductUpdateBlockInformationDto: {productUpdateBlockInformationDto}");
                return false;
            }
        }

        public async Task<bool> O_ProductUpdateBlockInventoryDto(int nopCommerceId, int stockQuantity, bool trackInventory)
        {
            ProductUpdateBlockInventoryDto productUpdateBlockInventoryDto = null;

            if (trackInventory)
            {
                 productUpdateBlockInventoryDto = new ProductUpdateBlockInventoryDto
                {
                    ManageInventoryMethodId = trackInventory == true ? 1 : 0, // Assuming ManageStock
                    StockQuantity = stockQuantity,
                    ProductAvailabilityRangeId = 0, // Assuming default value
                    UseMultipleWarehouses = false, // Assuming single warehouse
                    WarehouseId = 0, // Assuming default warehouse
                    DisplayStockAvailability = true, // Display stock availability
                    DisplayStockQuantity = true, // Display stock quantity
                    MinStockQuantity = 0, // Assuming no minimum stock quantity
                    LowStockActivityId = 0, // Assuming no action on low stock
                    NotifyAdminForQuantityBelow = 0, // Assuming no admin notification
                    BackorderModeId = 0, // Assuming no backorders
                    AllowBackInStockSubscriptions = true, // Allow back in stock subscriptions
                    OrderMinimumQuantity = 1, // Assuming minimum order quantity is 1
                    OrderMaximumQuantity = stockQuantity, // Assuming maximum order quantity is 1000
                    NotReturnable = true // Assuming product is not returnable
                };
            }
            else 
            {
                 productUpdateBlockInventoryDto = new ProductUpdateBlockInventoryDto
                {
                    ManageInventoryMethodId = 0, // Assuming ManageStock
                    AllowedQuantities = "1,2,3,4,5,6,7,8,9,10",
                    OrderMinimumQuantity = 1,
                    OrderMaximumQuantity = 1000,
                    NotReturnable = true
                 };
            }

            if (!trackInventory)
                productUpdateBlockInventoryDto.AllowedQuantities = "1,2,3,4,5,6,7,8,9,10";

            HttpResponseMessage ? response = await _apiServices.ProductService.UpdateBlockInventoryAsync(nopCommerceId, productUpdateBlockInventoryDto);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"### Updated ProductUpdateBlockInventoryDto : {productUpdateBlockInventoryDto}");
                return true;
            }
            else
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"### Updated FAILED!!!! ProductUpdateBlockInventoryDto: {productUpdateBlockInventoryDto}");
                return false;
            }
        }

        /// <summary>
        /// ## Product attributes add
        /// #### Note: Specification attribute need to have value! If not, it will be ignored in Product.
        /// #### For example: Product (specification attribute group) -> color (specification attribute) -> red, blue, green (specification attribute option)
        /// </summary>
        public async Task<bool> O_SpecificationAttributeCreateDto(int djangoId, int nopCommerceId)
        {
            SpecificationAttributeGroupCreateDto specificationAttributeGroupId;
            ProductSpecificationAttributeMappingCreateDto productSpecificationAttributeMapping;
            SpecificationAttributeCreateDto specificationAttributeCreateDto;
            SpecificationAttributeOptionCreateDto specificationAttributeOptionCreateDto;

            SpecificationAttributeGroupDto createdGroup = null; // step 1
            SpecificationAttributeDto createdAttribute = null; // step 2
            SpecificationAttributeOptionDto createdOption = null; // step 3
            ProductSpecificationAttributeMappingDto createdMapping = null; // step 4


            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var optionGrID in AttributeGroups)
                {
                    // step 1 - ONLY ONCE, can be only one object in database
                    var (attributeValue, attributeName) = Django_Attribute(djangoId, optionGrID.Key);

                    // if has attibute
                    if (!string.IsNullOrEmpty(attributeValue))
                    {

                        var httpResponseGroupByName = await _apiServices.SpecificationAttributeGroupService.GetByNameAsync("Product");
                        // if not exists create once
                        if (httpResponseGroupByName.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            specificationAttributeGroupId = new SpecificationAttributeGroupCreateDto
                            {
                                Name = "Product",
                                DisplayOrder = 0
                            };

                            HttpResponseMessage? response = await _apiServices.SpecificationAttributeGroupService.CreateAsync(specificationAttributeGroupId);

                            if (response.IsSuccessStatusCode)
                            {
                                createdGroup = await response.Content.ReadFromJsonAsync<SpecificationAttributeGroupDto>();
                                Console.WriteLine($"### Added SpecificationAttributeGroupCreateDto : {specificationAttributeGroupId.ToString()}");
                                return false;
                            }
                            else
                            {
                                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                                Console.WriteLine($"### Added FAILED!!!! SpecificationAttributeGroupCreateDto: {specificationAttributeGroupId.ToString()}");
                                //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                            }
                        }
                        // if exists set once
                        else
                        {
                            var response_ = await _apiServices.SpecificationAttributeGroupService.GetByNameAsync("Product");
                            createdGroup = await response_.Content.ReadFromJsonAsync<SpecificationAttributeGroupDto>();
                        }

                        // step 2
                        // can be only one in database with name and SpecificationAttributeGroupId

                        var existingAttributes = await _apiServices.SpecificationAttributeService.GetAllAsync();
                        createdAttribute = existingAttributes.FirstOrDefault(attr => attr.Name == attributeName && attr.SpecificationAttributeGroupId == createdGroup.Id);

                        if (createdAttribute == null)
                        {
                            specificationAttributeCreateDto = new SpecificationAttributeCreateDto
                            {
                                Name = attributeName,
                                DisplayOrder = 0,
                                SpecificationAttributeGroupId = createdGroup.Id
                            };

                            HttpResponseMessage? response = await _apiServices.SpecificationAttributeService.CreateAsync(specificationAttributeCreateDto);

                            if (response.IsSuccessStatusCode)
                            {
                                createdAttribute = await response.Content.ReadFromJsonAsync<SpecificationAttributeDto>();
                                Console.WriteLine($"### Added SpecificationAttributeCreateDto : {specificationAttributeCreateDto}");
                            }
                            else
                            {
                                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                                Console.WriteLine($"### Added FAILED!!!! SpecificationAttributeCreateDto: {specificationAttributeCreateDto}");
                                return false;
                            }
                        }

                        // step 3
                        // can be only one in database
                        // Check if the option already exists
                        var existingOptions = await _apiServices.SpecificationAttributeOptionService.GetAllBySpecificationAttributeIdAsync(createdAttribute.Id);
                        createdOption = existingOptions.FirstOrDefault(opt => opt.Name == attributeValue && opt.SpecificationAttributeId == createdAttribute.Id);

                        if (createdOption == null)
                        {
                            specificationAttributeOptionCreateDto = new SpecificationAttributeOptionCreateDto
                            {
                                Name = attributeValue,
                                DisplayOrder = 0,
                                SpecificationAttributeId = createdAttribute.Id
                            };

                            HttpResponseMessage? response = await _apiServices.SpecificationAttributeOptionService.CreateAsync(specificationAttributeOptionCreateDto);

                            if (response.IsSuccessStatusCode)
                            {
                                createdOption = await response.Content.ReadFromJsonAsync<SpecificationAttributeOptionDto>();
                                Console.WriteLine($"### Added SpecificationAttributeOptionCreateDto : {specificationAttributeOptionCreateDto}");
                            }
                            else
                            {
                                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                                Console.WriteLine($"### Added FAILED!!!! SpecificationAttributeOptionCreateDto: {specificationAttributeOptionCreateDto}");
                                return false;
                            }
                        }


                        // step 4
                        // Check if the mapping already exists
                        var existingMappings = await _apiServices.ProductSpecificationAttributeMappingService.GetByProductIdAsync(nopCommerceId);
                        createdMapping = existingMappings.FirstOrDefault(mapping => mapping.ProductId == nopCommerceId && mapping.SpecificationAttributeOptionId == createdOption.Id);

                        if (createdMapping == null)
                        {
                            productSpecificationAttributeMapping = new ProductSpecificationAttributeMappingCreateDto
                            {
                                ProductId = nopCommerceId,
                                SpecificationAttributeOptionId = createdOption.Id,
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };

                            HttpResponseMessage? response = await _apiServices.ProductSpecificationAttributeMappingService.CreateAsync(productSpecificationAttributeMapping);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"### Added ProductSpecificationAttributeMappingCreateDto : {productSpecificationAttributeMapping}");
                            }
                            else
                            {
                                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                                Console.WriteLine($"### Added FAILED!!!! ProductSpecificationAttributeMappingCreateDto: {productSpecificationAttributeMapping}");
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public async Task<bool> O_ProductUpdateBlockReviewsDto(int nopComerceProductId)
        {
            var productUpdateBlockReviewsDto = new ProductUpdateBlockReviewsDto
            {
                AllowCustomerReviews = true,
                ApprovedTotalReviews = 0,
                NotApprovedTotalReviews = 0
            };

            HttpResponseMessage? response = await _apiServices.ProductService.UpdateBlockReviewsAsync(nopComerceProductId, productUpdateBlockReviewsDto);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"### Updated ProductUpdateBlockReviewsDto : {productUpdateBlockReviewsDto}");
                return true;
            }
            else
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"### Updated FAILED!!!! ProductUpdateBlockReviewsDto: {productUpdateBlockReviewsDto}");
                return false;
            }
        }

        /// <summary>
        /// create name of Termin dostawy from django
        /// </summary>
        /// <param name="djangoId"></param>
        /// <returns>availability range id</returns>
        private async Task<int?> O_ProductAvailabilityRangeCreateDto(string terminDostawy)
        {
            // get name of Termin dostawy in django
            // if not exists add and return id else retun id
            IEnumerable<ProductAvailabilityRangeDto> responseQuestion = await _apiServices.ProductAvailabilityRangeService.GetAllAsync();
            var reponseList = responseQuestion.ToList().FirstOrDefault(x => x.Name == terminDostawy);

            if (reponseList == default && !string.IsNullOrEmpty(terminDostawy))
            { 
                ProductAvailabilityRangeCreateDto productAvailabilityRangeCreateDto = new ProductAvailabilityRangeCreateDto
                {
                    Name = terminDostawy, 
                    DisplayOrder = 0
                };

                HttpResponseMessage? response = await _apiServices.ProductAvailabilityRangeService.CreateAsync(productAvailabilityRangeCreateDto);

                if (response.IsSuccessStatusCode)
                {
                    var createdProductAvailabilityRange = await response.Content.ReadFromJsonAsync<ProductAvailabilityRangeDto>();
                    Console.WriteLine($"### Added ProductAvailabilityRangeCreateDto : {productAvailabilityRangeCreateDto.ToString()}");
                    return createdProductAvailabilityRange.Id;
                }
                else
                {
                    Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                    Console.WriteLine($"### Added FAILED!!!! ProductAvailabilityRangeCreateDto: {productAvailabilityRangeCreateDto.ToString()}");
                    //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                }
            }
            
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="djangoProductId"></param>
        /// <param name="nopCommerceProductId"></param>
        /// <returns>null false is eomthing get wrong</returns>
        public async Task<bool?> O_ProductCategoryMappingDtoCreateDto(int djangoProductId, int nopCommerceProductId)
        {
            // Get djangoCategory by Django product ID
            var djangoCategory = Django_GetCategoryByDjangoProductId(djangoProductId);

            if (djangoCategory == null)
            {
                Console.WriteLine($"Category not found for Django product ID: {djangoProductId}");
                return null;
            }

            var nopCommerceCategories = await _apiServices.CategoryService.GetAllAsync();
            var nopCommerceCategory = nopCommerceCategories.FirstOrDefault(c => c.Name.Equals(djangoCategory.Name, StringComparison.OrdinalIgnoreCase));

            if (nopCommerceCategory == null)
            {
                Console.WriteLine($"Category with name '{djangoCategory.Name}' not found in nopCommerce.");
                return false;
            }

            // Create ProductCategoryMappingCreateDto
            var productCategoryMappingCreateDto = new ProductCategoryMappingCreateDto
            {
                ProductId = nopCommerceProductId,
                CategoryId = nopCommerceCategory.Id,
                IsFeaturedProduct = false, // Assuming default value
                DisplayOrder = djangoCategory.Depth // Assuming depth as display order
            };

            // Call the API to create the mapping
            ProductCategoryMappingDto? response = await _apiServices.ProductCategoryMappingService.CreateAsync(productCategoryMappingCreateDto);

            if (response != null)
            {
                Console.WriteLine($"### Added ProductCategoryMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                return true;
            }
            else
            {
                Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                Console.WriteLine($"FAILED!!!!!! to add ProductCategoryMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                return false;
            }
        }

        public async Task<bool?> O_ManufacturerCreateDto()
        {
            // Use Django_GetManufacturer to get manufacturer data
            List<DjangoManufatorer> manufacturers = Django_GetManufacturer();

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var manufacturer in manufacturers)
                {
                    var manufacturerCreateDto = new ManufacturerCreateDto
                    {
                        Name = manufacturer.Nazwa,
                        Description = string.Empty, // Assuming no description for now
                        PictureId = 0, // Assuming no picture for now
                        PageSize = 10, // Default page size
                        Published = true,
                        DisplayOrder = 0, // Default display order
                        AllowCustomersToSelectPageSize = false,
                        ManufacturerTemplateId = 1           
                    };

                    ManufacturerDto? response = await _apiServices.ManufacturerService.CreateAsync(manufacturerCreateDto);

                    if (response != null)
                    {
                        Console.WriteLine($"### Added Manufacturer: {manufacturer.Nazwa}");
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to add Manufacturer: {manufacturer.Nazwa}");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// get nopcommerce manufacturer by id
        /// </summary>
        /// <param name="manufacturerName"></param>
        /// <returns></returns>
        private async Task<ManufacturerDto> O_ManufacturerDtoGetByName(string manufacturerName)
        {
            var manufacturers = await _apiServices.ManufacturerService.GetAllAsync();

            var manufacturer = manufacturers.FirstOrDefault(m => m.Name.Equals(manufacturerName, StringComparison.OrdinalIgnoreCase));

            if (manufacturer == null)
            {
                Console.WriteLine($"Manufacturer with name '{manufacturerName}' not found.");
            }

            return manufacturer;
        }
        public async Task<bool> O_ProductManufacturerMappingCreateDto(int djangoProductId, int nopCommerceProductId)
        {
            // Use Django_GetManufacturerGetByProductId to get manufacturer data
            List<DjangoManufatorer> djangoManufacturers = Django_GetManufacturerGetByProductId(djangoProductId);

            if (djangoManufacturers == null || djangoManufacturers.Count == 0)
            {
                Console.WriteLine($"No manufacturers found for Django product ID: {djangoProductId}");
                return false;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var djangoManufacturer in djangoManufacturers)
                {
                    // Get the corresponding nopCommerce manufacturer by name
                    ManufacturerDto nopCommerceManufacturer = await O_ManufacturerDtoGetByName(djangoManufacturer.Nazwa);

                    if (nopCommerceManufacturer == null)
                    {
                        Console.WriteLine($"Manufacturer with name '{djangoManufacturer.Nazwa}' not found in nopCommerce.");
                        return false;
                    }

                    var productManufacturerMappingCreateDto = new ProductManufacturerMappingCreateDto
                    {
                        ProductId = nopCommerceProductId,
                        ManufacturerId = nopCommerceManufacturer.Id,
                        IsFeaturedProduct = false, // Assuming default value
                        DisplayOrder = 0 // Default display order
                    };

                    HttpResponseMessage? response = await _apiServices.ProductManufaturerMappingService.CreateAsync(productManufacturerMappingCreateDto);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"### Added ProductManufacturerMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                    }
                    else
                    {
                        Console.WriteLine("ERROR-ERROR-ERROR-ERROR-ERROR-ERROR-ERROR");
                        Console.WriteLine($"FAILED!!!!!! to add ProductManufacturerMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                        return false;
                    }
                }
            }

            return true;
        }

        // DJANGO OBJECTS for above class to use it

        //main class product
        public DjangoCataloguProduct Django_CataloguProduct(int djangoProductId)
        {
            string query =
                $"""
                SELECT
                    id, 
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
                FROM public.catalogue_product
                WHERE id = {djangoProductId} and is_public = true;
                """;

            var product = new DjangoCataloguProduct();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(reader.GetOrdinal("id")))
                    {
                        Console.WriteLine("Product in django database with django id not found : " + djangoProductId.ToString());
                    }
                    else
                    {
                        product = new DjangoCataloguProduct
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            IsPublic = reader.GetBoolean(reader.GetOrdinal("is_public")),
                            Upc = reader.GetString(reader.GetOrdinal("upc")),
                            Title = reader.GetString(reader.GetOrdinal("title")),
                            Slug = reader.GetString(reader.GetOrdinal("slug")),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                            Rating = reader.IsDBNull(reader.GetOrdinal("rating")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("rating")),
                            DateCreated = reader.IsDBNull(reader.GetOrdinal("date_created")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("date_created")),
                            DateUpdated = reader.IsDBNull(reader.GetOrdinal("date_updated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("date_updated")),
                            IsDiscountable = reader.GetBoolean(reader.GetOrdinal("is_discountable")),
                            CenaKartotekowa = reader.GetDouble(reader.GetOrdinal("cena_kartotekowa")),
                            IsNewProduct = reader.GetBoolean(reader.GetOrdinal("is_new_product")),
                            SaleInPercent = reader.IsDBNull(reader.GetOrdinal("sale_in_percent")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("sale_in_percent")),
                            SubiektGtId = reader.GetInt32(reader.GetOrdinal("subiekt_gt_id")),
                            KatalogId = reader.IsDBNull(reader.GetOrdinal("katalog_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("katalog_id")),
                            ParentId = reader.IsDBNull(reader.GetOrdinal("parent_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_id")),
                            ProducentId = reader.IsDBNull(reader.GetOrdinal("producent_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("producent_id")),
                            ProductClassId = reader.IsDBNull(reader.GetOrdinal("product_class_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("product_class_id")),
                            Zablokowany = reader.GetBoolean(reader.GetOrdinal("zablokowany")),
                            NumerArtykuluProducenta = reader.IsDBNull(reader.GetOrdinal("numer_artykulu_producenta")) ? null : reader.GetString(reader.GetOrdinal("numer_artykulu_producenta")),
                            WagaWKg = reader.IsDBNull(reader.GetOrdinal("waga_w_kg")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("waga_w_kg")),
                            PodstawowaJednostkaMiary = reader.IsDBNull(reader.GetOrdinal("podstawowa_jednostka_miary")) ? null : reader.GetString(reader.GetOrdinal("podstawowa_jednostka_miary")),
                            OpisPodstawowaJednostkaMiary = reader.IsDBNull(reader.GetOrdinal("opis_podstawowa_jednostka_miary")) ? null : reader.GetString(reader.GetOrdinal("opis_podstawowa_jednostka_miary")),
                            Format = reader.IsDBNull(reader.GetOrdinal("format")) ? null : reader.GetString(reader.GetOrdinal("format")),
                            Struktura = reader.IsDBNull(reader.GetOrdinal("struktura")) ? null : reader.GetString(reader.GetOrdinal("struktura")),
                            DluzycaDo29 = reader.GetBoolean(reader.GetOrdinal("dluzyca_do_2_9")),
                            DluzycaOd3 = reader.GetBoolean(reader.GetOrdinal("dluzyca_od_3")),
                            SledzStan = reader.GetBoolean(reader.GetOrdinal("sledz_stan")),
                            TerminDostawy = reader.IsDBNull(reader.GetOrdinal("termin_dostawy")) ? null : reader.GetString(reader.GetOrdinal("termin_dostawy")),
                            HafeleApi = reader.GetBoolean(reader.GetOrdinal("hafele_api")),
                            HafeleApiCatalogLink = reader.IsDBNull(reader.GetOrdinal("hafele_api_catalog_link")) ? null : reader.GetString(reader.GetOrdinal("hafele_api_catalog_link")),
                            GtvApi = reader.GetBoolean(reader.GetOrdinal("gtv_api")),
                            GtvApiCatalogLink = reader.IsDBNull(reader.GetOrdinal("gtv_api_catalog_link")) ? null : reader.GetString(reader.GetOrdinal("gtv_api_catalog_link"))
                        };
                    }
                }
            });

            //_dbConnector.CloseConnection();

            return product;
        }
        private DjanogCategory Django_GetCategoryByDjangoProductId(int djangoProductId)
        {
            string query = $"""
            SELECT id, "path", "depth", numchild, "name", description, image, slug, czy_ma_sie_wyswietlac_w_menu
            FROM public.catalogue_category
            WHERE id = (SELECT category_id FROM public.catalogue_productcategory WHERE product_id = {djangoProductId});
            """;
            var category = new DjanogCategory();
            //_dbConnector.OpenConnection();
            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    category = new DjanogCategory
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Path = reader.GetString(reader.GetOrdinal("path")),
                        Depth = reader.GetInt32(reader.GetOrdinal("depth")),
                        NumChild = reader.GetInt32(reader.GetOrdinal("numchild")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
                        Slug = reader.GetString(reader.GetOrdinal("slug")),
                        CzyMaSieWyswietlacWMenu = reader.GetBoolean(reader.GetOrdinal("czy_ma_sie_wyswietlac_w_menu"))
                    };
                }
            });
            //_dbConnector.CloseConnection();
            return category;
        }
        private static Dictionary<int, string> AttributeGroups = new Dictionary<int, string>
            {
                { 3, "rodzaj zastosowań" },
                { 5, "materiał" },
                { 6, "mocowanie puszki" },
                { 7, "kolor / powierzchnia" },
                { 8, "średnica(mm)" },
                { 9, "cichy domyk" },
                { 4, "kąt otwarcia(stopni)" },
                { 10, "wysokość" },
                { 11, "szerokość" },
                { 12, "głębokość" },
                { 13, "front - grubość" },
                { 14, "długość" },
                { 15, "średnica otworu montażowego" },
                { 16, "system montażu" },
                { 17, "ilość półek" },
                { 19, "element podnośnika" },
                { 20, "wymaga zawiasu" },
                { 21, "podblatowe" },
                { 22, "szafki narożnej" },
                { 23, "wysokie" },
                { 24, "pojemność(ml)" },
                { 25, "profil" },
                { 26, "typ montażu" },
                { 27, "USB" },
                { 28, "ilość koszy" },
                { 30, "udźwig" },
                { 31, "mocowanie prowadnika" },
                { 18, "szerokość szafki(cm)" },
                { 32, "rodzaj półki" },
                { 33, "kolor" },
                { 34, "rozstaw(mm)" },
                { 35, "rodzaj" },
                { 36, "wysuw" },
                { 37, "mechanizm otwierania/ zamykania" },
                { 38, "komplet" },
                { 39, "element składowy" },
                { 40, "barwa światła" },
                { 41, "barwa światła" },
                { 42, "barwa światła" },
                { 43, "kolor oprawy" },
                { 44, "szerkość frontu" },
                { 45, "szerokość frontu(cm)" },
                { 46, "drzwi" },
                { 47, "moc podnośnika" },
                { 48, "typ" },
                { 49, "typ" },
                { 50, "typ gniazda" },
                { 51, "napięcie" }
            };


        public class DjangoRecommendedProduct
        {
            public int Id { get; set; }
            public int Ranking { get; set; }
            public int PrimaryId { get; set; }
            public int RecommendationId { get; set; }
        }

        // product connection
        public List<DjangoRecommendedProduct> Django_GetRecommendedProducts(int djangoProductId)
        {
            string query = $"""
                SELECT id, ranking, primary_id, recommendation_id
                FROM public.catalogue_productrecommendation
                WHERE primary_id = {djangoProductId}
            """;

            var recommendedProducts = new List<DjangoRecommendedProduct>();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var recommendedProduct = new DjangoRecommendedProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Ranking = reader.GetInt32(reader.GetOrdinal("ranking")),
                        PrimaryId = reader.GetInt32(reader.GetOrdinal("primary_id")),
                        RecommendationId = reader.GetInt32(reader.GetOrdinal("recommendation_id")) 
                    };
                    recommendedProducts.Add(recommendedProduct);
                }
            });

            return recommendedProducts;
        }


        private (string attributeName, string attributeValue) Django_Attribute(int productId, int optionGrouId)
        {
            
            // zwraca - name e.g. : 84mm, 50 cm, sprężyna, chrom ... 

            // zwraca wartość atrybutu po optionGrouId Uwaga różnie z tymi wartościami bywa, problem może być z int, czasem są np cm mm a czasem nie
            string query =
                $"""
                    SELECT name
                    FROM public.catalogue_productattribute as attribute
                    where option_group_id = {optionGrouId}
                    and product_class_id = (select product_class_id from catalogue_product where id = {productId})
                    and id = (select attribute_id FROM public.catalogue_productattributevalue WHERE product_id = {productId} and attribute_id = attribute.id and value_boolean = true)
                """;

            //_dbConnector.OpenConnection();

            string attributeValue = string.Empty;

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    attributeValue = reader.GetString(reader.GetOrdinal("name"));
                }
            });

            //_dbConnector.CloseConnection();

            return (attributeValue, AttributeGroups.GetValueOrDefault(optionGrouId));
        }
        private List<DjanogCategory> Django_GetCategory()
        {
            string query = """
            SELECT id, "path", "depth", numchild, "name", description, image, slug, czy_ma_sie_wyswietlac_w_menu
            FROM public.catalogue_category;
            """;

            var categories = new List<DjanogCategory>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var category = new DjanogCategory
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Path = reader.GetString(reader.GetOrdinal("path")),
                        Depth = reader.GetInt32(reader.GetOrdinal("depth")),
                        NumChild = reader.GetInt32(reader.GetOrdinal("numchild")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
                        Slug = reader.GetString(reader.GetOrdinal("slug")),
                        CzyMaSieWyswietlacWMenu = reader.GetBoolean(reader.GetOrdinal("czy_ma_sie_wyswietlac_w_menu"))
                    };
                    categories.Add(category);
                }
            });

            //_dbConnector.CloseConnection();

            return categories;
        }
        private List<DjangoManufatorer> Django_GetManufacturer()
        {
            string query = """
                SELECT id, nazwa, logo, duze_logo_na_stronie_glownej, male_logo_na_strine_glownej, odnosnik_do_strony_producenta
                FROM public.catalogue_producent;
                """;

            var manufacturers = new List<DjangoManufatorer>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var manufacturer = new DjangoManufatorer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nazwa = reader.GetString(reader.GetOrdinal("nazwa")),
                        Logo = reader.IsDBNull(reader.GetOrdinal("logo")) ? null : reader.GetString(reader.GetOrdinal("logo")),
                        DuzeLogoNaStronieGlownej = reader.IsDBNull(reader.GetOrdinal("duze_logo_na_stronie_glownej")) ? null : reader.GetBoolean(reader.GetOrdinal("duze_logo_na_stronie_glownej")),
                        MaleLogoNaStrineGlownej = reader.IsDBNull(reader.GetOrdinal("male_logo_na_strine_glownej")) ? null : reader.GetBoolean(reader.GetOrdinal("male_logo_na_strine_glownej")),
                        OdnosnikDoStronyProducenta = reader.IsDBNull(reader.GetOrdinal("odnosnik_do_strony_producenta")) ? null : reader.GetString(reader.GetOrdinal("odnosnik_do_strony_producenta"))
                    };
                    manufacturers.Add(manufacturer);
                }
            });

            //_dbConnector.CloseConnection();

            return manufacturers;
        }
        private List<DjangoManufatorer> Django_GetManufacturerGetByProductId(int djangoProductId)
        {
            string query = $"""
            SELECT p.id, p.nazwa, p.logo, p.duze_logo_na_stronie_glownej, p.male_logo_na_strine_glownej, p.odnosnik_do_strony_producenta
            FROM public.catalogue_producent p
            JOIN public.catalogue_product pr ON p.id = pr.producent_id
            WHERE pr.id = {djangoProductId};
            """;

            var manufacturers = new List<DjangoManufatorer>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var manufacturer = new DjangoManufatorer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nazwa = reader.GetString(reader.GetOrdinal("nazwa")),
                        Logo = reader.IsDBNull(reader.GetOrdinal("logo")) ? null : reader.GetString(reader.GetOrdinal("logo")),
                        DuzeLogoNaStronieGlownej = reader.IsDBNull(reader.GetOrdinal("duze_logo_na_stronie_glownej")) ? null : reader.GetBoolean(reader.GetOrdinal("duze_logo_na_stronie_glownej")),
                        MaleLogoNaStrineGlownej = reader.IsDBNull(reader.GetOrdinal("male_logo_na_strine_glownej")) ? null : reader.GetBoolean(reader.GetOrdinal("male_logo_na_strine_glownej")),
                        OdnosnikDoStronyProducenta = reader.IsDBNull(reader.GetOrdinal("odnosnik_do_strony_producenta")) ? null : reader.GetString(reader.GetOrdinal("odnosnik_do_strony_producenta"))
                    };
                    manufacturers.Add(manufacturer);
                }
            });

            //_dbConnector.CloseConnection();

            return manufacturers;
        }
        private List<DjangoPicture> Django_GetPicture(int djangoProductId, int djangoPictureId)
        {
            string query = $"""
        SELECT id, original, caption, display_order, date_created, watermark, product_id
        FROM public.catalogue_productimage where product_id = {djangoProductId} and id = {djangoPictureId} order by display_order ASC;
        """;

            var pictures = new List<DjangoPicture>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var picture = new DjangoPicture
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Original = reader.GetString(reader.GetOrdinal("original")),  // url
                        Caption = reader.IsDBNull(reader.GetOrdinal("caption")) ? null : reader.GetString(reader.GetOrdinal("caption")),
                        DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
                        Watermark = reader.IsDBNull(reader.GetOrdinal("watermark")) ? null : reader.GetString(reader.GetOrdinal("watermark")),  // url
                        ProductId = reader.GetInt32(reader.GetOrdinal("product_id"))
                    };
                    pictures.Add(picture);
                }
            });

            //_dbConnector.CloseConnection();

            return pictures;
        }
        public List<DjangoPicture> Django_GetPictureByProductId(int djangoProductId)
        {
            string query = $"""
        SELECT id, original, caption, display_order, date_created, watermark, product_id
        FROM public.catalogue_productimage
        WHERE product_id = {djangoProductId};
        """;

            var pictures = new List<DjangoPicture>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var picture = new DjangoPicture
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Original = "https://stolargo.pl/media/" + reader.GetString(reader.GetOrdinal("original")),
                        Caption = reader.IsDBNull(reader.GetOrdinal("caption")) ? null : reader.GetString(reader.GetOrdinal("caption")),
                        DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
                        Watermark = reader.IsDBNull(reader.GetOrdinal("watermark")) ? null : reader.GetString(reader.GetOrdinal("watermark")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("product_id"))
                    };
                    pictures.Add(picture);
                }
            });

            //_dbConnector.CloseConnection();

            return pictures;
        }

        public List<DjangoCataloguProduct> Django_GetAllProducts()
        {
            string query = 
                """
                SELECT
                    id, 
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
                FROM public.catalogue_product where is_public = true;
                """;

            var products = new List<DjangoCataloguProduct>();

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var product = new DjangoCataloguProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        IsPublic = reader.GetBoolean(reader.GetOrdinal("is_public")),
                        Upc = reader.GetString(reader.GetOrdinal("upc")),
                        Title = reader.GetString(reader.GetOrdinal("title")),
                        Slug = reader.GetString(reader.GetOrdinal("slug")),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        Rating = reader.IsDBNull(reader.GetOrdinal("rating")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("rating")),
                        DateCreated = reader.IsDBNull(reader.GetOrdinal("date_created")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("date_created")),
                        DateUpdated = reader.IsDBNull(reader.GetOrdinal("date_updated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("date_updated")),
                        IsDiscountable = reader.GetBoolean(reader.GetOrdinal("is_discountable")),
                        CenaKartotekowa = reader.GetDouble(reader.GetOrdinal("cena_kartotekowa")),
                        IsNewProduct = reader.GetBoolean(reader.GetOrdinal("is_new_product")),
                        SaleInPercent = reader.IsDBNull(reader.GetOrdinal("sale_in_percent")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("sale_in_percent")),
                        SubiektGtId = reader.GetInt32(reader.GetOrdinal("subiekt_gt_id")),
                        KatalogId = reader.IsDBNull(reader.GetOrdinal("katalog_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("katalog_id")),
                        ParentId = reader.IsDBNull(reader.GetOrdinal("parent_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_id")),
                        ProducentId = reader.IsDBNull(reader.GetOrdinal("producent_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("producent_id")),
                        ProductClassId = reader.IsDBNull(reader.GetOrdinal("product_class_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("product_class_id")),
                        Zablokowany = reader.GetBoolean(reader.GetOrdinal("zablokowany")),
                        NumerArtykuluProducenta = reader.IsDBNull(reader.GetOrdinal("numer_artykulu_producenta")) ? null : reader.GetString(reader.GetOrdinal("numer_artykulu_producenta")),
                        WagaWKg = reader.IsDBNull(reader.GetOrdinal("waga_w_kg")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("waga_w_kg")),
                        PodstawowaJednostkaMiary = reader.IsDBNull(reader.GetOrdinal("podstawowa_jednostka_miary")) ? null : reader.GetString(reader.GetOrdinal("podstawowa_jednostka_miary")),
                        OpisPodstawowaJednostkaMiary = reader.IsDBNull(reader.GetOrdinal("opis_podstawowa_jednostka_miary")) ? null : reader.GetString(reader.GetOrdinal("opis_podstawowa_jednostka_miary")),
                        Format = reader.IsDBNull(reader.GetOrdinal("format")) ? null : reader.GetString(reader.GetOrdinal("format")),
                        Struktura = reader.IsDBNull(reader.GetOrdinal("struktura")) ? null : reader.GetString(reader.GetOrdinal("struktura")),
                        DluzycaDo29 = reader.GetBoolean(reader.GetOrdinal("dluzyca_do_2_9")),
                        DluzycaOd3 = reader.GetBoolean(reader.GetOrdinal("dluzyca_od_3")),
                        SledzStan = reader.GetBoolean(reader.GetOrdinal("sledz_stan")),
                        TerminDostawy = reader.IsDBNull(reader.GetOrdinal("termin_dostawy")) ? null : reader.GetString(reader.GetOrdinal("termin_dostawy")),
                        HafeleApi = reader.GetBoolean(reader.GetOrdinal("hafele_api")),
                        HafeleApiCatalogLink = reader.IsDBNull(reader.GetOrdinal("hafele_api_catalog_link")) ? null : reader.GetString(reader.GetOrdinal("hafele_api_catalog_link")),
                        GtvApi = reader.GetBoolean(reader.GetOrdinal("gtv_api")),
                        GtvApiCatalogLink = reader.IsDBNull(reader.GetOrdinal("gtv_api_catalog_link")) ? null : reader.GetString(reader.GetOrdinal("gtv_api_catalog_link"))
                    };
                    products.Add(product);
                }
            });

            //_dbConnector.CloseConnection();

            return products;
        }

        public (decimal price, int stock) Django_ProductPriceAndStock(int djangoProductId)
        {
            string query =
                $"""
                    SELECT price_excl_tax, num_in_stock 
                    FROM public.partner_stockrecord 
                    WHERE product_id = {djangoProductId} AND partner_sku LIKE 'HURT%';
                """;

            decimal price = 0;
            int stock = 0;

            //_dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    price = reader.GetDecimal(reader.GetOrdinal("price_excl_tax"));
                    stock = reader.GetInt32(reader.GetOrdinal("num_in_stock"));
                }
            });

            //_dbConnector.CloseConnection();

            return (price, stock);
        }






    }


}

public class DjangoPicture
{
    public int Id { get; set; }
    public string Original { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public System.DateTime DateCreated { get; set; }
    public string? Watermark { get; set; }
    public int ProductId { get; set; }
}



public class DjangoManufatorer
{
    public int Id { get; set; }
    public string Nazwa { get; set; }
    public string? Logo { get; set; }
    public bool? DuzeLogoNaStronieGlownej { get; set; }
    public bool? MaleLogoNaStrineGlownej { get; set; }
    public string? OdnosnikDoStronyProducenta { get; set; }
}


public class DjanogCategory
{
    public int Id { get; set; }
    public string Path { get; set; }
    public int Depth { get; set; }
    public int NumChild { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string Slug { get; set; }
    public bool CzyMaSieWyswietlacWMenu { get; set; }
}

// main table catalogue_product
public class DjangoCataloguProduct
{
    public int Id { get; set; }
    public string? Structure { get; set; }
    public bool IsPublic { get; set; }
    public string Upc { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public double? Rating { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public bool IsDiscountable { get; set; }
    public double CenaKartotekowa { get; set; }
    public bool IsNewProduct { get; set; }
    public double? SaleInPercent { get; set; }
    public int SubiektGtId { get; set; }
    public int? KatalogId { get; set; }
    public int? ParentId { get; set; }
    public int? ProducentId { get; set; }
    public int? ProductClassId { get; set; }
    public bool Zablokowany { get; set; }
    public string? NumerArtykuluProducenta { get; set; }
    public double? WagaWKg { get; set; }
    public string? PodstawowaJednostkaMiary { get; set; }
    public string? OpisPodstawowaJednostkaMiary { get; set; }
    public string? Format { get; set; }
    public string? Struktura { get; set; }
    public bool DluzycaDo29 { get; set; }
    public bool DluzycaOd3 { get; set; }
    public bool SledzStan { get; set; }
    public string? TerminDostawy { get; set; }
    public bool HafeleApi { get; set; }
    public string? HafeleApiCatalogLink { get; set; }
    public bool GtvApi { get; set; }
    public string? GtvApiCatalogLink { get; set; }
}
