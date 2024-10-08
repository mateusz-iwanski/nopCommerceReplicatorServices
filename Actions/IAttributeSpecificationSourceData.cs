using nopCommerceReplicatorServices.nopCommerce;

namespace nopCommerceReplicatorServices.Actions
{
    public interface IAttributeSpecificationSourceData
    {
        IEnumerable<AttributeSpecificationMapperDto>? Get(int productId);
    }
}