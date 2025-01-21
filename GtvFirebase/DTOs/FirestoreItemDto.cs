using FirebaseManager.Firestore;
using Google.Cloud.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nopCommerceReplicatorServices.GtvFirebase.DTOs;
using GtvApiHub;
using nopCommerceReplicatorServices.GtvFirebase;

namespace GtvApiHubnopCommerceReplicatorServices.GtvFirebase.DTOs
{
    [FirestoreData]
    public record FirestoreItemDto : IBaseDto, IResponseDto, IFirestoreDto, IFirestoreItemDto, IFirestoreDtoCompareStrategy
    {
        [FirestoreProperty]
        public int Id { get; init; }

        [FirestoreProperty]
        public List<ItemDto> Item { get; init; }

        [FirestoreProperty]
        public PriceDto Price { get; init; }

        [FirestoreProperty]
        public List<StockDto> Stocks { get; init; }

        [FirestoreProperty]
        public List<AttributeDto>? Attributes { get; init; }

        [FirestoreProperty]
        public List<CategoryTreeDto>? CategoryTrees { get; init; }

        [FirestoreProperty]
        public List<PackageTypeDto>? PackageTypes { get; init; }

        [FirestoreProperty]
        public List<AlternativeItemDto> AlternateItems { get; init; }

        [FirestoreProperty]
        public string ItemCode { get; init; }  

        public string CollectionName => "Gtv_Items";
        public string? DocumentUniqueField => ItemCode;

        /// <summary>
        /// Compare every records and list with records. If any of them are different, return false.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true if exactly the same otherwise false</returns>
        public bool IsEqualTo(object obj)
        {
            if (obj is not FirestoreItemDto other || EqualityContract != other.EqualityContract)
                return false;
            
            try
            {
                var compare = Item.SequenceEqual(other.Item) &&
                       Price.Equals(other.Price) &&
                       Stocks.SequenceEqual(other.Stocks) &&
                       (Attributes == null && other.Attributes == null || Attributes != null && other.Attributes != null && Attributes.SequenceEqual(other.Attributes)) &&
                       (CategoryTrees == null && other.CategoryTrees == null || CategoryTrees != null && other.CategoryTrees != null && CategoryTrees.SequenceEqual(other.CategoryTrees)) &&
                       (PackageTypes == null && other.PackageTypes == null || PackageTypes != null && other.PackageTypes != null && PackageTypes.SequenceEqual(other.PackageTypes)) &&
                       ItemCode == other.ItemCode;

                return compare;
            }
            catch (Exception)
            {
                // if record has no fields which should be check it will
                // raise an error so return false
                return false;
            }
        }

        /// <summary>
        /// Compare the same type of objects IFirestoreDto.
        /// 
        /// If objects has the same item code is the same object even if the other fields are different.
        /// If fields are different so one of object was updated.
        /// </summary>
        /// <returns>false if two different objects the same type, true if are the same objects</returns>
        /// <exception cref="Exception">When comparing two objects with diffrent type</exception>
        public bool Compare(IFirestoreDto other)
        {
            if (other.GetType() != typeof(FirestoreItemDto)) throw new Exception("Invalid type.");

            FirestoreItemDto toCompare = (FirestoreItemDto)other;

            if (ItemCode != toCompare.ItemCode) return false;  // if item code is different, return false

            return true;
        }

        /// <summary>
        /// Check if the DTO object from API is updated relative to Firestore DTO.
        /// </summary>
        /// <exception cref="Exception">
        /// Invalid objects. Check only the same objects, 
        /// before using IsUpdated function use Compare function to check it.
        /// </exception>
        public bool IsUpdated(IFirestoreDto other)
        {
            if (Compare(other) == false)
                throw new Exception("Invalid objects. Check only the same objects, before using IsUpdated function use Compare function to check it.");

            if (other.GetType() != typeof(FirestoreItemDto)) throw new Exception("Invalid type.");

            if ((FirestoreItemDto)other == this) return false;  // the same so is not updated

            return true;
        }        

        /// <summary>
        /// Override HashCode for Equals method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(EqualityContract);
            hash.Add(ItemCode);
            hash.Add(Price);
            foreach (var item in Item)
                hash.Add(item);
            foreach (var stock in Stocks)
                hash.Add(stock);
            if (Attributes != null)
                foreach (var attribute in Attributes)
                    hash.Add(attribute);
            if (CategoryTrees != null)
                foreach (var categoryTree in CategoryTrees)
                    hash.Add(categoryTree);
            if (PackageTypes != null)
                foreach (var packageType in PackageTypes)
                    hash.Add(packageType);
            return hash.ToHashCode();
        }
    }
}
