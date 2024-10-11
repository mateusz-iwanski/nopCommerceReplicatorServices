using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Interfaces.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{

    /// <summary>
    /// Option for attribute specification
    /// 
    /// For example: Color, Opening angle, etc.
    /// </summary>
    public class AttributeSpecificationOptionNopCommerce
    {
        private ISpecificationAttributeOptionService _specificationAttributeOptionApi { get; set; }

        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationOptionNopCommerce(IApiConfigurationServices apiServices, IServiceProvider serviceProvider)
        {
            _specificationAttributeOptionApi = apiServices.SpecificationAttributeOptionService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a new spec attribute option. If there is such an option, just return it.
        /// </summary>
        /// <param name="attributeSpecificationOptionNopCommerce">SpecificationAttributeOptionCreateDto</param>
        /// <returns>SpecificationAttributeOptionDto or throw CustomException</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeOptionDto> CreateAsync(SpecificationAttributeOptionCreateDto attributeSpecificationOptionNopCommerce)
        {
            try
            {
                // if exists return
                var existing = await GetByNameAsync(attributeSpecificationOptionNopCommerce.Name);                
                if (existing != null) return existing;

                // if not exists add new
                var apiResponse = await _specificationAttributeOptionApi.CreateAsync(attributeSpecificationOptionNopCommerce);
                if (apiResponse.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new Exceptions.CustomException($"Failed to create a new specification attribute option. {apiResponse.ReasonPhrase}");
                }

                return await apiResponse.Content.ReadFromJsonAsync<SpecificationAttributeOptionDto>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.CustomException($"Failed to create a new specification attribute option. {ex.Message}");
            }
        }

        /// <summary>
        /// Get a spec attribute option by name
        /// </summary>
        /// <param name="name">name of attribute option</param>
        /// <returns>If exists SpecificationAttributeOptionDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeOptionDto>? GetByNameAsync(string name)
        {
            var allSpecAttrOption =  await _specificationAttributeOptionApi.GetAllAsync();
            
            return allSpecAttrOption.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Get a spec attribute option by specification attribute id
        /// </summary>
        /// <param name="name">option ID</param>
        /// <returns>If exists SpecificationAttributeOptionDto, null if not</returns>
        [DeserializeWebApiNopCommerceResponse]
        public async Task<SpecificationAttributeOptionDto>? GetBySpecificationAttributeIdAsync(int specificationAttributeId)
        {
            var allSpecAttrOption = await _specificationAttributeOptionApi.GetAllAsync();

            return allSpecAttrOption.FirstOrDefault(x => x.SpecificationAttributeId == specificationAttributeId);
        }
    }
}
