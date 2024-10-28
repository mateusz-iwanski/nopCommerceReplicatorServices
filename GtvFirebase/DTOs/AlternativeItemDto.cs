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
    public record AlternativeItemDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string BaseItemCode { get; init; }

        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public double Match { get; init; }

        [FirestoreProperty]
        public string Remarks { get; init; }

        public string CollectionName { get => "ItemAlternativeItems"; }
        public string DocumentUniqueField { get => $"{BaseItemCode}#{ItemCode}"; }
    }
}
