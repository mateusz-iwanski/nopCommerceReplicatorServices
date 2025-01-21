namespace nopCommerceReplicatorServices.DataBinding
{
    public interface IProductDataBinder
    {
        Task BindAsync(int nopCommerceProductId, int gtvId);
        //void RemoveStockReplication(int productNopCommerceId);
        //void SetPriceReplicationAsync(int externalProductId);
        //void RemovePriceReplication(int externalProductId);
        //void SetStockReplication(int productNopCommerceId);
    }
}