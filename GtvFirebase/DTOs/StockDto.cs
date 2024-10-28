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
    public record StockDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string WarehouseCode { get; init; }

        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public int InStock { get; init; }

        public string CollectionName { get => "ItemStock"; }
        public string DocumentUniqueField { get => "Stock_" + WarehouseCode + "_" + ItemCode; }        
        public WarehouseCode GetWarehouse => Enum.Parse<WarehouseCode>(WarehouseCode);
    }
}
