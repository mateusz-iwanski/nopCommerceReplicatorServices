using FirebaseManager.Firestore;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase.DTOs
{
    [FirestoreData]
    public record CategoryTreeDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public string Name { get; init; }

        [FirestoreProperty]
        public int Depth { get; init; }

        [FirestoreProperty]
        public bool IsCollective { get; init; }

        [FirestoreProperty]
        public string LanguageCode { get; init; }

        public string CollectionName { get => "ItemCategoryTree"; }
        public string DocumentUniqueField { get => "CategoryTree_" + ItemCode + "_" + Depth + "_" + LanguageCode; }
    }
}
