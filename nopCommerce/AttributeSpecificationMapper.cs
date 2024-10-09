﻿using Microsoft.Extensions.DependencyInjection;
using nopCommerceWebApiClient.Objects.Customer;
using nopCommerceWebApiClient.Objects.Product;
using nopCommerceWebApiClient.Objects.ProductSpecificationAttributeMapping;
using nopCommerceWebApiClient.Objects.SpecificationAttribute;
using nopCommerceWebApiClient.Objects.SpecyficationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.nopCommerce
{
    /// <summary>
    /// Class for mapping attribute specification to product.
    /// 
    /// Uses the AttributeSpecificationNopCommerce, AttributeSpecificationOptionNopCommerce, ProductSpecificationAttributeMappingNopCommerce attributes.
    /// The minimal data class creates a new specification attribute mapping to the product with all needed dependent classes.
    /// </summary>
    public class AttributeSpecificationMapper
    {
        private readonly ProductDto _productDto;
        private readonly IServiceProvider _serviceProvider;

        public AttributeSpecificationMapper(ProductDto productDto, IServiceProvider serviceProvider)
        {
            _productDto = productDto;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Map attribute specification to product.
        /// </summary>
        /// <param name="groupName">Product, Accessories, Board etc.</param>        
        /// <param name="value">Color, Opening angle, etc.</param>
        /// <param name="optionName">Red, Black, Left corner etc.</param>
        public async Task Map(AttributeSpecificationMapperDto attributeSpecificationMapperDto)
        {
            var attributeSpecificationService = _serviceProvider.GetService<AttributeSpecificationNopCommerce>();
            SpecificationAttributeDto attributeSpecificationDto = await attributeSpecificationService.CreateSetAsync(
                attributeSpecificationMapperDto.groupName,
                attributeSpecificationMapperDto.value,
                attributeSpecificationMapperDto.optionName
                );

            var attributeSpecificationOptionService = _serviceProvider.GetService<AttributeSpecificationOptionNopCommerce>();
            var specificationAttributeOptionDto = await attributeSpecificationOptionService.GetBySpecificationAttributeIdAsync(attributeSpecificationDto.Id) ??
                throw new CustomException("SpecificationAttributeOption with specification attribute Id not exists");

            await createAsync(specificationAttributeOptionDto);
        }

        /// <summary>
        /// Create a new specification attribute mapping with the product.
        /// </summary>
        /// <param name="specificationAttributeOptionDto">SpecificationAttributeOptionDto</param>
        /// <returns></returns>
        private async Task createAsync(SpecificationAttributeOptionDto specificationAttributeOptionDto)
        {
            var productSpecificationAttributeMappingNopCommerceService = _serviceProvider.GetService<ProductSpecificationAttributeMappingNopCommerce>();
            await productSpecificationAttributeMappingNopCommerceService.CreateAsync(_productDto, specificationAttributeOptionDto, true, true, 0);
        }
    }
}