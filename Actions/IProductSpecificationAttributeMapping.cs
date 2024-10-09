using nopCommerceWebApiClient.Objects.ProductSpecificationAttributeMapping;

namespace nopCommerceReplicatorServices.Actions
{
    public interface IProductSpecificationAttributeMapping
    {
        string ServiceKeyName { get; }
        Task<List<HttpResponseMessage>> CreateAsync(int productId, IAttributeSpecificationSourceData attributeSpecExternal, Service setService);
        Task<ProductSpecificationAttributeMappingDto>? GetByIdsAsync(int productId, int attributeSpecificationOptionId);
    }
}