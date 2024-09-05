using nopCommerceWebApiClient.Objects.Product;

namespace nopCommerceReplicatorServices.Actions
{
    public interface IProduct
    {
        Task<ProductDto?> GetProductById(int productId);
    }
}