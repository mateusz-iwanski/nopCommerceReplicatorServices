using Google.Api;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    // inherited by ProductDataBinder, CustomerDataBinder ...
    public abstract class ProductDataBinderBase : IProductDataBinder
    {
        protected readonly DataBinding _dataBinding;
        protected readonly Service _service;

        public ProductDataBinderBase(DataBinding dataBinding, Service service)
        {
            _dataBinding = dataBinding;
            _service = service;
        }

        /// <summary>
        /// Unmark stock replication from nopCommerceID in all services.
        /// </summary>
        /// <param name="productNopCommerceId"></param>
        /// <returns></returns>
        private async Task RemoveStockReplicationFromProductInServicesAsync(int productNopCommerceId)
        {
            foreach (var service in Enum.GetNames(typeof(Service)))
            {
                var productKeyBinding = await _dataBinding.GetKeyBindingByExternalIdAsync(Enum.Parse<Service>(service), ObjectToBind.Product, productNopCommerceId);
                if (productKeyBinding != null)
                {
                    productKeyBinding.IsStockReplicated = false;
                }
            }
        }

        /// <summary>
        /// Link in BindData database external product with nopCommerce product.
        /// 
        /// Before link a nopCommerce product with external product, product has to exist in nopCommerce 
        /// and nopCommerce product has to be linked with SubiektGT serviceProvider.
        /// </summary>
        /// <param name="nopCommerceProductId">nopCommerce product ID</param>
        /// <param name="externalProductId">Product id from external service</param>
        /// <exception cref="Exceptions.CustomException"></exception>
        public async Task BindProductAsync(IServiceProvider serviceProvider, int nopCommerceProductId, int externalProductId)
        {
            using var scope = serviceProvider.CreateScope();
            var productNopCommerceService = scope.ServiceProvider.GetRequiredService<Func<string, IProduct>>()("ProductNopCommerce");
            ProductDto? productFromNopCommerce = await productNopCommerceService.GetProductByIdAsync(nopCommerceProductId) ??
                throw new Exceptions.CustomException($"Product with nopCommerce ID '{nopCommerceProductId}' doesn't exist in nopCommerce.");

            var dataBindingObject = await _dataBinding.GetKeyBindingByNopCommerceIdAsync(Service.SubiektGT, ObjectToBind.Product, nopCommerceProductId) ??
                throw new Exceptions.CustomException($"Can't find product by nopCommerce Id - '{nopCommerceProductId}' in DataBinding for serviceProvider {Service.SubiektGT.ToString()}. You have to map it first.");

            await _dataBinding.BindKeyAsync(nopCommerceProductId, _service, ObjectToBind.Product, externalProductId);
        }

        /// <summary>
        /// Set the stock replication status for the product.
        /// Product in all services will be unmarked as stock replicated before setting a new one.
        /// Can't mark more than one serviceProvider as stock replicated for one product.
        /// </summary>
        /// <param name="service">SubiektGT, Django, GTVAPI ETC.</param>
        /// <param name="productNopCommerceId">nopCommerce product ID</param>
        /// <exception cref="CustomException">If not bind in DataBinding</exception>
        public virtual async Task SetStockReplicationaAsync(int productNopCommerceId, bool replicatedOrNot)
        {
            var productKeyBinding = await _dataBinding.GetKeyBindingByNopCommerceIdAsync(_service, ObjectToBind.Product, productNopCommerceId);

            if (productKeyBinding != null)
            {
                // remove all stock replication from product if has to be replicated,
                // because we can't replicate stock from two services for one product
                if (replicatedOrNot) 
                    await RemoveStockReplicationFromProductInServicesAsync(productNopCommerceId);
             
                productKeyBinding.IsStockReplicated = replicatedOrNot;
            }
            else
            {
                throw new CustomException($"The product from {_service.ToString()} with binded nopCommerce ID '{productNopCommerceId}' " +
                    $"is not link with any {_service.ToString()} product. BindAsync data before you want to mark as stock replicated");
            }
        }

        public virtual async Task SetPriceReplicationAsync(int externalProductId, bool replicatedOrNot)
        {
            var productKeyBinding = await _dataBinding.GetKeyBindingByExternalIdAsync(_service, ObjectToBind.Product, externalProductId);

            if (productKeyBinding != null)
            {
                productKeyBinding.IsPriceReplicated = replicatedOrNot;
            }
            else
            {
                throw new CustomException($"The product from {_service.ToString()} with ID '{externalProductId}' " +
                    $"is not link with any nopCommerce product. BindAsync data before you want to mark as price replicated");
            }
        }

        public abstract Task BindAsync(int nopCommerceProductId, int gtvId);
    }
}
