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
    public record PriceDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string CardCode { get; init; }

        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public string BasePrice { get; init; }

        [FirestoreProperty]
        public string FinalPrice { get; init; }

        [FirestoreProperty]
        public string Discount { get; init; }

        [FirestoreProperty]
        public string Currency { get; init; }

        public string CollectionName { get => "ItemPrice"; }
        public string DocumentUniqueField { get => "Price_" + ItemCode; }
    }
}
