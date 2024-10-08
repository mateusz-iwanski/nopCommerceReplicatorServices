using nopCommerceReplicatorServices.nopCommerce;

namespace nopCommerceReplicatorServices.Django
{
    public interface IAttributeSpecificationSourceData
    {
        IEnumerable<AttributeSpecificationMapperDto>? Get(int productId);
    }
}