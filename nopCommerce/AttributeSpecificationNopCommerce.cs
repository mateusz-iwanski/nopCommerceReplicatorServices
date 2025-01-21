using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.Product;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttributeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Attribute specification object. Consists of:
    /// AttributeSpecificationOption
    /// AttributeSpecificationGroup
    /// 
    /// For example: Product, Accessories, Board etc
    /// </summary>
    public class AttributeSpecificationNopCommerce
    {
        private readonly ISpecificationAttributeService _specificationAttributerApi;
        private readonly AttributeSpecificationOptionNopCommerce _attributeSpecificationOptionNopCommerce;
        private readonly AttributeSpecificationGroupNopCommerce _attributeSpecificationGroupNopCommerce;


        public AttributeSpecificationNopCommerce(
            IApiConfigurationServices apiServices,
            AttributeSpecificationOptionNopCommerce attributeSpecificationOptionNopCommerce,
            AttributeSpecificationGroupNopCommerce attributeSpecificationGroupNopCommerce
            )
        {
            _specificationAttributerApi = apiServices.SpecificationAttributeService;
            _attributeSpecificationGroupNopCommerce = attributeSpecificationGroupNopCommerce;
            _attributeSpecificationOptionNopCommerce = attributeSpecificationOptionNopCommerce;
        }

        /// <summary>
        /// Create a new specification attribute with group and value. If exists return it.
        /// </summary>
        /// <param name="groupName">Product, Accessories, Board etc.</param>        
        /// <param name="optionName">Color, Opening angle, etc.</param>
        /// <param name="value">Red, Black, Left corner etc.</param>        
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<SpecificationAttributeDto> CreateSetAsync(string groupName, string value, string optionName)
        {
            // create or get group
            var atrrSpecGroupObjectDto = await _attributeSpecificationGroupNopCommerce.CreateAsync(
                new SpecificationAttributeGroupCreateDto
                {
                    Name = groupName
                });

            // create or get attribute
            var attrObjectDto = await createAsync(
                new SpecificationAttributeCreateDto
                {
                    SpecificationAttributeGroupId = atrrSpecGroupObjectDto.Id,
                    Name = value
                });

            // create or get option
            var attrValueObjectDto = await _attributeSpecificationOptionNopCommerce.CreateAsync(
                new SpecificationAttributeOptionCreateDto
                {
                    SpecificationAttributeId = attrObjectDto.Id,
                    Name = optionName
                });

            return attrObjectDto;
        }

        /// <summary>
        /// Create a new spec attribute group. If there is such an option, just return it.
        /// </summary>
        /// <param name="attributeSpecificationNopCommerce">SpecificationAttributeOptionCreateDto</param>
        /// <returns>SpecificationAttributeDto or throw CustomException</returns>
        [DeserializeWebApiNopCommerceResponse]
        private async Task<SpecificationAttributeDto> createAsync(SpecificationAttributeCreateDto attributeSpecificationNopCommerce)
        {
            try
            {
                // if exists return
                var existing = await GetByNameAsync(attributeSpecificationNopCommerce.Name);
                if (existing != null) return existing;

                // if not exists add new
                var apiResponse = await _specificationAttributerApi.CreateAsync(attributeSpecificationNopCommerce);
                if (apiResponse.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new Exceptions.CustomException($"Failed to create a new specification attribute. {apiResponse.ReasonPhrase}");
                }

                return await apiResponse.Content.ReadFromJsonAsync<SpecificationAttributeDto>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.CustomException($"Failed to create a new specification attribute group. {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specification attribute
        /// </summary>
        /// <param name="name">name of attribute</param>
        /// <returns>If exists SpecificationAttributeDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeDto>? GetByNameAsync(string name)
        {
            var allSpecAttrGroup = await _specificationAttributerApi.GetAllAsync();

            return allSpecAttrGroup.FirstOrDefault(x => x.Name == name);
        }

    }
}
