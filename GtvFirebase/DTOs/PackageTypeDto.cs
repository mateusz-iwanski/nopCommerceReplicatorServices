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
    /// Package Type DTO represents product package types from Api.
    /// </summary>
    [FirestoreData]
    public record PackageTypeDto : IBaseDto, IResponseDto, IFirestoreDto
    {
        [FirestoreProperty]
        public string EAN { get; init; }

        [FirestoreProperty]
        public string PackingUnitName { get; init; }

        [FirestoreProperty]
        public string ItemCode { get; init; }

        [FirestoreProperty]
        public double QuantityPerPackage { get; init; }

        [FirestoreProperty]
        public double LengthInCm { get; init; }

        [FirestoreProperty]
        public double WidthInCm { get; init; }

        [FirestoreProperty]
        public double HeightInCm { get; init; }

        [FirestoreProperty]
        public double VolumeInCubeCm { get; init; }

        [FirestoreProperty]
        public double GrossWeightInKg { get; init; }

        [FirestoreProperty]
        public bool isBaseUnit { get; init; }

        [FirestoreProperty]
        public bool isDefaultUnit { get; init; }

        [FirestoreProperty]
        public int Order { get; init; }

        public string CollectionName { get => "ItemPackageTypes"; }
        public string DocumentUniqueField { get => EAN; }
    }
}
