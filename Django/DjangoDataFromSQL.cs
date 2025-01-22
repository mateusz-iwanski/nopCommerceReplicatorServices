using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nopCommerceReplicatorServices.Django
{
    internal class DjangoDataFromSQL
    {

        public static ProductCreateMinimalDto O_ProductCreateMinimalDto(int djangoId)
        {
            var product = new ProductCreateMinimalDto()
            {
                Name = "Product name",
                ShortDescription = "Short description",
                FullDescription = "Full description",
                Sku = "SKU",
                ManufacturerPartNumber = "Manufacturer part number",
                Gtin = "GTIN",
                IsGiftCard = false,
                IsDownload = false,
                IsRecurring = false,
                IsShipEnabled = true,
                IsFreeShipping = false,
                AdditionalShippingCharge = 0,
                IsTaxExempt = false,
                TaxCategoryId = 0,
                ManageInventoryMethodId = 0,
                StockQuantity = 0,
                DisplayStockAvailability = true,
                DisplayStockQuantity = true,
                MinStockQuantity = 0,
                LowStockActivityId = 0,
                NotifyAdminForQuantityBelow = 0,
                BackorderModeId = 0,
                AllowBackInStockSubscriptions = false,
                OrderMinimumQuantity = 0,
                OrderMaximumQuantity = 0,
                AllowedQuantities = "1,2,3,4,5",
                DisableBuyButton = false,
                DisableWishlistButton = false,
                AvailableForPreOrder = false,
                CallForPrice = false,
                Price = 0,
                OldPrice = 0,
                ProductCost = 0,
                CustomerEntersPrice = false,
                MinimumCustomerEnteredPrice = 0,
                MaximumCustomerEnteredPrice = 0,
                Weight = 0,
                Length = 0,
                Width = 0,
                Height = 0,
                DisplayOrder = 0,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductTypeId = 0,
                VisibleIndividually = true,
                IsFeatured = false,
                MarkAsNew = false,
                MarkAsNewStartDateTimeUtc = DateTime.UtcNow,
                MarkAsNewEndDateTimeUtc = DateTime.UtcNow,
                LimitedToStores = "all",
                ProductTags = "tag1,tag2,tag3",
                ProductCollections = "collection1,collection2,collection3",
                ProductSpecifications = "spec1,spec2,spec3",
                ProductAttributes = "attr1,attr2,attr3",
                ProductManufacturers = "man1,man2,man3",
                ProductCategories = "cat1,cat2,cat3",
                ProductImages = "img1,img2,img3",
                ProductSe
            };

        }



        // atrybuty
        public void attribute(int productId, int optionGrouId)
        {
            /// wymiary
            // option_group_id = '10' = wysokosc
            // option_group_id = '11' = szerokosc
            // option_group_id = '12' = glebokosc
            // option_group_id = '14' = dlugosc
            // rest:
            // 3	rodzaj zastosowań
            //5   materiał
            //6   mocowanie puszki
            //7   kolor / powierzchnia
            //8   średnica(mm)
            //9   cichy domyk
            //4   kąt otwarcia(stopni)
            //10  wysokość
            //11  szerokość
            //12  głębokość
            //13  front - grubość
            //14  długość
            //15  średnica otworu montażowego
            //16  system montażu
            //17  ilość półek
            //19  element podnośnika
            //20  wymaga zawiasu
            //21  podblatowe
            //22  szafki narożnej
            //23  wysokie
            //24  pojemność(ml)
            //25  profil
            //26  typ montażu
            //27  USB
            //28  ilość koszy
            //30  udźwig
            //31  mocowanie prowadnika
            //18  szerokość szafki(cm)
            //32  rodzaj półki
            //33  kolor
            //34  rozstaw(mm)
            //35  rodzaj
            //36  wysuw
            //37  mechanizm otwierania/ zamykania
            //38  komplet
            //39  element składowy
            //40  barwa światła
            //41  barwa światła
            //42  barwa światła
            //43  kolor oprawy
            //44  szerkość frontu
            //45  szerokość frontu(cm)
            //46  drzwi
            //47  moc podnośnika
            //48  typ
            //49  typ
            //50  typ gniazda
            //51  napięcie

            // name e.g. : 84mm, 50 cm, sprężyna, chrom ... 

            // zwraca wartość atrybutu po optionGrouId Uwaga różnie z tymi wartościami bywa, problem może być z int, czasem są np cm mm a czasem nie
            //SELECT name
            //FROM public.catalogue_productattribute as attribute
            //where option_group_id = 10
            //and product_class_id = (select product_class_id from catalogue_product where id = '11476')
            //and id = (select attribute_id FROM public.catalogue_productattributevalue WHERE product_id = '11475' and attribute_id = attribute.id and value_boolean = true)
        }
    }
}
