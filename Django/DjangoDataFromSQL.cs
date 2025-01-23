using Microsoft.Extensions.DependencyInjection;
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
using System.Diagnostics;
using System.Net.Http.Json;

namespace nopCommerceReplicatorServices.Django
{

    /// <summary>
    /// 0 - najpierw skopiować zdjęcia na bloba, później dodać zdjęcia do produktów lllllll
    /// 1 - O_AddCategory najpierw trzeba utworzyć kategorie i je ustawić ręcznie w nopCommerce display order to z django zagniezdzenie
    /// STOP - ustaw kategorie 
    /// 2 - O_ProductCreateMinimalDto
    /// // 3 - O_PictureCreate - pozniej nie wiem jeszcze jak
    /// 3 - O_SpecificationAttributeCreateDto
    /// 4 - O_ProductAvailabilityRangeCreateDto
    /// 5 - O_ProductCategoryMappingDtoCreateDto
    /// 6 - O_ManufacturerCreateDto
    /// 7 - O_ProductManufacturerMappingCreateDto
    /// </summary>

    internal class DjangoDataFromSQL
    {
        private readonly DBConnector _dbConnector;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiConfigurationServices _apiServices;

        public DjangoDataFromSQL(IServiceProvider provider)
        {
            _dbConnector = new DBConnector("Django", "postgresql");
            _serviceProvider = provider;
            _dbConnector.Initialize();

            _apiServices = new ApiConfigurationServices();

            return;
        }

        /// <summary>
        /// ad data from django picture to django db
        /// it doesn't store information about file uri
        /// </summary>
        /// <returns></returns>
        public async Task<bool> O_PictureCreate()
        {
            // Use Django_GetPicture to get picture data
            List<DjangoPicture> pictures = Django_GetPicture();

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var picture in pictures)
                {
                    var pictureCreateDto = new PictureCreateDto
                    {
                        MimeType = "image/jpeg", // Assuming JPEG format, adjust as necessary
                        SeoFilename = picture.Original,
                        AltAttribute = "",
                        TitleAttribute = "",
                        IsNew = true,
                        VirtualPath = picture.Original // Assuming the original path is the virtual path
                    };

                    HttpResponseMessage? response = await _apiServices.PictureService.CreateAsync(pictureCreateDto);

                    if (response.IsSuccessStatusCode)
                    {
                        var createdPicture = await response.Content.ReadFromJsonAsync<PictureDto>();
                        Console.WriteLine($"Added Picture: {createdPicture.Id} for Django picture ID: {picture.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add Picture for Django picture ID: {picture.Id}");
                        return false;
                    }
                }

                return true;
            }
        }

        public async Task<bool> O_ProductPictureMappingCreateDto(int djangoProductId, int nopCommerceProductId)
        {
            throw new Exception("Not implemented");
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

                    Debug.WriteLine($"Failed to add Category: {category.Name}");
                    Debug.WriteLine($"Status Code: {response.StatusCode}");
                    Debug.WriteLine($"Reason Phrase: {response.ReasonPhrase}");
                    Debug.WriteLine($"Response Content: {response}");

                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Added Category: {category.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add Category: {category.Name}");
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
                        Console.WriteLine($"Failed to parse length in cm: {lengthString}");
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
                        Console.WriteLine($"Failed to parse length in mm: {lengthString}");
                    }
                }
                else if (!decimal.TryParse(lengthString, out length))
                {
                    // Handle the case where parsing fails
                    Console.WriteLine($"Failed to parse length: {lengthString}");
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
                HttpResponseMessage? response = await _apiServices.ProductService.CreateMinimalAsync(product);

                if (response.IsSuccessStatusCode)
                {
                    var specificationAttributeGroupDto = await response.Content.ReadFromJsonAsync<SpecificationAttributeGroupDto>();
                    addedNopCommerceId = specificationAttributeGroupDto.Id;
                    Console.WriteLine($"Added O_ProductCreateMinimalDto : {product.ToString()}");
                    return addedNopCommerceId;
                }
                else
                {
                    Console.WriteLine($"Added FAILED!!!! O_ProductCreateMinimalDto: {product.ToString()}");
                    //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                }
            }
            
            return null;
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
                    // step 1
                    var (attributeValue, attributeName) = Django_Attribute(djangoId, optionGrID.Key);
                    specificationAttributeGroupId = new SpecificationAttributeGroupCreateDto
                    {
                        Name = "Producy",
                        DisplayOrder = 0
                    };

                    HttpResponseMessage? response = await _apiServices.SpecificationAttributeGroupService.CreateAsync(specificationAttributeGroupId);

                    if (response.IsSuccessStatusCode)
                    {
                        createdGroup = await response.Content.ReadFromJsonAsync<SpecificationAttributeGroupDto>();
                        Console.WriteLine($"Added SpecificationAttributeGroupCreateDto : {specificationAttributeGroupId.ToString()}");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Added FAILED!!!! SpecificationAttributeGroupCreateDto: {specificationAttributeGroupId.ToString()}");
                        //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                    }

                    // step 2
                    specificationAttributeCreateDto = new SpecificationAttributeCreateDto
                    {
                        Name = attributeName,
                        DisplayOrder = 0,
                        SpecificationAttributeGroupId = createdGroup.Id
                    };

                    response = await _apiServices.SpecificationAttributeService.CreateAsync(specificationAttributeCreateDto);

                    if (response.IsSuccessStatusCode)
                    {
                        createdAttribute = await response.Content.ReadFromJsonAsync<SpecificationAttributeDto>();
                        Console.WriteLine($"Added SpecificationAttributeCreateDto : {specificationAttributeCreateDto.ToString()}");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Added FAILED!!!! SpecificationAttributeCreateDto: {specificationAttributeCreateDto.ToString()}");
                        //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                    }

                    // step 3
                    specificationAttributeOptionCreateDto = new SpecificationAttributeOptionCreateDto
                    {
                        Name = attributeValue,
                        DisplayOrder = 0,
                        SpecificationAttributeId = createdAttribute.Id
                    };

                    response = await _apiServices.SpecificationAttributeOptionService.CreateAsync(specificationAttributeOptionCreateDto);

                    if (response.IsSuccessStatusCode)
                    {
                        createdOption = await response.Content.ReadFromJsonAsync<SpecificationAttributeOptionDto>();
                        Console.WriteLine($"Added SpecificationAttributeOptionCreateDto : {specificationAttributeOptionCreateDto.ToString()}");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Added FAILED!!!! SpecificationAttributeOptionCreateDto: {specificationAttributeOptionCreateDto.ToString()}");
                        //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                    }

                    // step 4
                    productSpecificationAttributeMapping = new ProductSpecificationAttributeMappingCreateDto
                    {
                        ProductId = nopCommerceId,
                        SpecificationAttributeOptionId = createdOption.Id,
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };

                    response = await _apiServices.ProductSpecificationAttributeMappingService.CreateAsync(productSpecificationAttributeMapping);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Added ProductSpecificationAttributeMappingCreateDto : {productSpecificationAttributeMapping.ToString()}");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Added FAILED!!!! ProductSpecificationAttributeMappingCreateDto: {productSpecificationAttributeMapping.ToString()}");
                        //AttributeHelper.DeserializeWebApiNopCommerceResponseAsync<ProductNopCommerce>("ProductCreateMinimalDto", responseList);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// create name of Termin dostawy in django
        /// </summary>
        /// <param name="djangoId"></param>
        /// <returns>availability range id</returns>
        public async Task<int?> O_ProductAvailabilityRangeCreateDto(string terminDostawy)
        {
            // get name of Termin dostawy in django
            // if not exists add and return id else retun id
            IEnumerable<ProductAvailabilityRangeDto> responseQuestion = await _apiServices.ProductAvailabilityRangeService.GetAllAsync();
            var reponseList = responseQuestion.ToList().FirstOrDefault(x => x.Name == terminDostawy);

            if (reponseList == default)
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
                    Console.WriteLine($"Added ProductAvailabilityRangeCreateDto : {productAvailabilityRangeCreateDto.ToString()}");
                    return createdProductAvailabilityRange.Id;
                }
                else
                {
                    Console.WriteLine($"Added FAILED!!!! ProductAvailabilityRangeCreateDto: {productAvailabilityRangeCreateDto.ToString()}");
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

            // Create ProductCategoryMappingCreateDto
            var productCategoryMappingCreateDto = new ProductCategoryMappingCreateDto
            {
                ProductId = nopCommerceProductId,
                CategoryId = djangoCategory.Id,
                IsFeaturedProduct = false, // Assuming default value
                DisplayOrder = djangoCategory.Depth // Assuming depth as display order
            };

            // Call the API to create the mapping
            ProductCategoryMappingDto? response = await _apiServices.ProductCategoryMappingService.CreateAsync(productCategoryMappingCreateDto);

            if (response != null)
            {
                Console.WriteLine($"Added ProductCategoryMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to add ProductCategoryMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
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
                        DisplayOrder = 0 // Default display order
                    };

                    ManufacturerDto? response = await _apiServices.ManufacturerService.CreateAsync(manufacturerCreateDto);

                    if (response != null)
                    {
                        Console.WriteLine($"Added Manufacturer: {manufacturer.Nazwa}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add Manufacturer: {manufacturer.Nazwa}");
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
                        Console.WriteLine($"Added ProductManufacturerMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add ProductManufacturerMapping for Django product ID: {djangoProductId} to nopCommerce product ID: {nopCommerceProductId}");
                        return false;
                    }
                }
            }

            return true;
        }

        // DJANGO OBJECTS for above class to use it

        //main class product
        private DjangoCataloguProduct Django_CataloguProduct(int djangoProductId)
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
                WHERE id = {djangoProductId};
                """;

            var product = new DjangoCataloguProduct();

            _dbConnector.OpenConnection();

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

            _dbConnector.CloseConnection();

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
            _dbConnector.OpenConnection();
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
            _dbConnector.CloseConnection();
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
        private (string attributeName, string attributeValue) Django_Attribute(int productId, int optionGrouId)
        {
            
            // zwraca - name e.g. : 84mm, 50 cm, sprężyna, chrom ... 

            // zwraca wartość atrybutu po optionGrouId Uwaga różnie z tymi wartościami bywa, problem może być z int, czasem są np cm mm a czasem nie
            string query =
                """
                    SELECT name
                    FROM public.catalogue_productattribute as attribute
                    where option_group_id = 10
                    and product_class_id = (select product_class_id from catalogue_product where id = '11476')
                    and id = (select attribute_id FROM public.catalogue_productattributevalue WHERE product_id = '11475' and attribute_id = attribute.id and value_boolean = true)
                """;

            _dbConnector.OpenConnection();

            string attributeValue = string.Empty;

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    attributeValue = reader.GetString(reader.GetOrdinal("name"));
                }
            });

            _dbConnector.CloseConnection();

            return (attributeValue, AttributeGroups.GetValueOrDefault(optionGrouId));
        }
        private List<DjanogCategory> Django_GetCategory()
        {
            string query = """
            SELECT id, "path", "depth", numchild, "name", description, image, slug, czy_ma_sie_wyswietlac_w_menu
            FROM public.catalogue_category;
            """;

            var categories = new List<DjanogCategory>();

            _dbConnector.OpenConnection();

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

            _dbConnector.CloseConnection();

            return categories;
        }
        private List<DjangoManufatorer> Django_GetManufacturer()
        {
            string query = """
                SELECT id, nazwa, logo, duze_logo_na_stronie_glownej, male_logo_na_strine_glownej, odnosnik_do_strony_producenta
                FROM public.catalogue_producent;
                """;

            var manufacturers = new List<DjangoManufatorer>();

            _dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var manufacturer = new DjangoManufatorer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nazwa = reader.GetString(reader.GetOrdinal("nazwa")),
                        Logo = reader.IsDBNull(reader.GetOrdinal("logo")) ? null : reader.GetString(reader.GetOrdinal("logo")),
                        DuzeLogoNaStronieGlownej = reader.IsDBNull(reader.GetOrdinal("duze_logo_na_stronie_glownej")) ? null : reader.GetString(reader.GetOrdinal("duze_logo_na_stronie_glownej")),
                        MaleLogoNaStrineGlownej = reader.IsDBNull(reader.GetOrdinal("male_logo_na_strine_glownej")) ? null : reader.GetString(reader.GetOrdinal("male_logo_na_strine_glownej")),
                        OdnosnikDoStronyProducenta = reader.IsDBNull(reader.GetOrdinal("odnosnik_do_strony_producenta")) ? null : reader.GetString(reader.GetOrdinal("odnosnik_do_strony_producenta"))
                    };
                    manufacturers.Add(manufacturer);
                }
            });

            _dbConnector.CloseConnection();

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

            _dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var manufacturer = new DjangoManufatorer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nazwa = reader.GetString(reader.GetOrdinal("nazwa")),
                        Logo = reader.IsDBNull(reader.GetOrdinal("logo")) ? null : reader.GetString(reader.GetOrdinal("logo")),
                        DuzeLogoNaStronieGlownej = reader.IsDBNull(reader.GetOrdinal("duze_logo_na_stronie_glownej")) ? null : reader.GetString(reader.GetOrdinal("duze_logo_na_stronie_glownej")),
                        MaleLogoNaStrineGlownej = reader.IsDBNull(reader.GetOrdinal("male_logo_na_strine_glownej")) ? null : reader.GetString(reader.GetOrdinal("male_logo_na_strine_glownej")),
                        OdnosnikDoStronyProducenta = reader.IsDBNull(reader.GetOrdinal("odnosnik_do_strony_producenta")) ? null : reader.GetString(reader.GetOrdinal("odnosnik_do_strony_producenta"))
                    };
                    manufacturers.Add(manufacturer);
                }
            });

            _dbConnector.CloseConnection();

            return manufacturers;
        }
        private List<DjangoPicture> Django_GetPicture()
        {
            string query = """
        SELECT id, original, caption, display_order, date_created, watermark, product_id
        FROM public.catalogue_productimage;
        """;

            var pictures = new List<DjangoPicture>();

            _dbConnector.OpenConnection();

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

            _dbConnector.CloseConnection();

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

            _dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    var picture = new DjangoPicture
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Original = reader.GetString(reader.GetOrdinal("original")),
                        Caption = reader.IsDBNull(reader.GetOrdinal("caption")) ? null : reader.GetString(reader.GetOrdinal("caption")),
                        DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
                        Watermark = reader.IsDBNull(reader.GetOrdinal("watermark")) ? null : reader.GetString(reader.GetOrdinal("watermark")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("product_id"))
                    };
                    pictures.Add(picture);
                }
            });

            _dbConnector.CloseConnection();

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
                FROM public.catalogue_product;
                """;

            var products = new List<DjangoCataloguProduct>();

            _dbConnector.OpenConnection();

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

            _dbConnector.CloseConnection();

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

            _dbConnector.OpenConnection();

            _dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    price = reader.GetDecimal(reader.GetOrdinal("price_excl_tax"));
                    stock = reader.GetInt32(reader.GetOrdinal("num_in_stock"));
                }
            });

            _dbConnector.CloseConnection();

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
    public string? DuzeLogoNaStronieGlownej { get; set; }
    public string? MaleLogoNaStrineGlownej { get; set; }
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
