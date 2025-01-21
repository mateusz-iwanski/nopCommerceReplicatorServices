using FirebaseManager.Firestore;
using Google.Cloud.Firestore;
using GtvApiHubnopCommerceReplicatorServices.GtvFirebase.DTOs;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.nopCommerce;
using Refit;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase
{
    /// <summary>
    /// Attribute from GTV Firestore with GTV Api data
    /// </summary>
    public class AttributeSpecificationGtvFirebase : IAttributeSpecificationSourceData
    {
        private readonly IFirestoreService _firestoreService;
        private readonly IProductBaseSourceData _productSourceGt;
        private readonly IServiceProvider _serviceProvider;
        private readonly DataBinding.DataBinding _dataBinding;

        public AttributeSpecificationGtvFirebase(IFirestoreService firestoreService, IProductBaseSourceData productGt, DataBinding.DataBinding dataBinding)
        {
            _firestoreService = firestoreService;
            _productSourceGt = productGt;
            _dataBinding = dataBinding;
        }

        /// <summary>
        /// Get attribute by SubiektGT product ID
        /// </summary>
        /// <param name="subiektProductId">SubiektGT service product ID</param>
        /// <returns>List of AttributeSpecificationMapperDto, null if not exists</returns>s
        public IEnumerable<AttributeSpecificationMapperDto>? Get(int subiektProductId)
        {
            var attributeSpecfificationList = new List<AttributeSpecificationMapperDto>();

            var gtvProductId = new GtvDataBinding(_dataBinding).GetGtvIdBySubiektAsync(subiektProductId);  

            var document = _firestoreService.ReadDocumentAsync<FirestoreItemDto>(new FirestoreItemDto().CollectionName, gtvProductId.ToString()).Result;

            foreach (var attr in document.Attributes)
            {
                var attributeSpecfification = new AttributeSpecificationMapperDto(
                    "Product",
                    attr.AttributeName.ToUpper(),
                    attr.Value.ToUpper()
                    );

                attributeSpecfificationList.Add(attributeSpecfification);
            }

            return attributeSpecfificationList.Count > 0 ? attributeSpecfificationList : null;
        }
    }
}
