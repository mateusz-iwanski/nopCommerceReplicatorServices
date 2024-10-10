using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttributeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Group for attribute specification
    /// 
    /// For example: Product, Accessories, Board etc
    /// </summary>
    public class AttributeSpecificationGroupNopCommerce
    {
        private ISpecificationAttributeGroupService _specificationAttributeGroupApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationGroupNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributeGroupApi = apiServices.SpecificationAttributeGroupService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a new spec attribute group. If there is such an option, just return it.
        /// </summary>
        /// <param name="attributeSpecificationOptionNopCommerce">SpecificationAttributeOptionCreateDto</param>
        /// <returns>SpecificationAttributeOptionDto or throw CustomException</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeGroupDto> CreateAsync(SpecificationAttributeGroupCreateDto attributeSpecificationGroupNopCommerce)
        {
            try
            {
                // if exists return
                var existing = await GetByNameAsync(attributeSpecificationGroupNopCommerce.Name);
                if (existing != null) return existing;

                // if not exists add new
                var apiResponse = await _specificationAttributeGroupApi.CreateAsync(attributeSpecificationGroupNopCommerce);
                if (apiResponse.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new Exceptions.CustomException($"Failed to create a new specification attribute group. {apiResponse.ReasonPhrase}");
                }

                return await apiResponse.Content.ReadFromJsonAsync<SpecificationAttributeGroupDto>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.CustomException($"Failed to create a new specification attribute group. {ex.Message}");
            }
        }

        /// <summary>
        /// Get a spec attribute group by name
        /// </summary>
        /// <param name="name">name of attribute option</param>
        /// <returns>If exists SpecificationAttributeOptionDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeGroupDto>? GetByNameAsync(string name)
        {
            var allSpecAttrGroup = await _specificationAttributeGroupApi.GetAllAsync();

            return allSpecAttrGroup.FirstOrDefault(x => x.Name == name);
        }
    }
}
