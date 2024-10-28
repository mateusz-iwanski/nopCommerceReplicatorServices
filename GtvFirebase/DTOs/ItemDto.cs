using FirebaseManager.Firestore;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase.DTOs
{
    /// <summary>
    /// Item DTO represents products from Api.
    /// </summary>
    [FirestoreData]
    public record ItemDto : IBaseDto, IResponseDto, IFirestoreDto, IFirestoreDtoCompareStrategy
    {
        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public string ItemName { get; init; }

        [FirestoreProperty]
        public string LanguageCode { get; init; }

        public string CollectionName { get => "Items_" + LanguageCode; }

        public string DocumentUniqueField { get => ItemCode; }

        /// <summary>
        /// Compare the same type of objects IFirestoreDto.
        /// 
        /// If DTO was updated in API we can compare it with Firestore document by 
        /// immutable fields.
        /// </summary>
        /// <returns>false if two different objects the same type, true if are the same objects</returns>
        public bool Compare(IFirestoreDto other)
        {
            if (other.GetType() != typeof(ItemDto)) throw new Exception("Invalid type.");

            ItemDto toCompare = (ItemDto)other;

            if (toCompare == this) return true;  // if all fields are the same, return true

            // if fields which must be updated are different return false
            // otherwise return true    
            return ItemCode == toCompare.ItemCode
                && LanguageCode == toCompare.LanguageCode
                && CollectionName == toCompare.CollectionName
                && DocumentUniqueField == toCompare.DocumentUniqueField;
        }

        /// <summary>
        /// Check if the DTO object from API is updated relative to Firestore DTO.
        /// 
        /// Only compare the same type of DTOs
        /// </summary>
        public bool IsUpdated(IFirestoreDto other)
        {
            if (Compare(other) == false)
                throw new Exception("Invalid objects. Check only the same objects, before using IsUpdated function use Compare function to check it.");

            if (other.GetType() != typeof(ItemDto)) throw new Exception("Invalid type.");

            if ((ItemDto)other == this) return false;  // the same so is not updated

            return true;
        }

    }
}
