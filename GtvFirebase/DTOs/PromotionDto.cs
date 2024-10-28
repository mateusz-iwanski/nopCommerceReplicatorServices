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
    public record PromotionDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public string CardCode { get; init; }

        [FirestoreProperty]
        public string PromotionName { get; init; }

        public string CollectionName { get => "ItemPromotion"; }
        public string DocumentUniqueField { get => "Promotion_" + ItemCode; }
    }
}
