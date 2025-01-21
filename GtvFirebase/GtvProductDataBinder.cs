using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase
{
    /// <summary>
    /// Helpful class to bind GTV product with nopCommerce product in BindingData database.
    /// </summary>
    public class GtvProductDataBinder : ProductDataBinderBase, IProductDataBinder
    {
        private readonly IServiceProvider _service;

        public GtvProductDataBinder(DataBinding.DataBinding dataBinding, IServiceProvider service)
            : base(dataBinding, Service.GtvApi)
        {
            _service = service;
            return;
        }

        /// <summary>
        /// Link in BindData database GTV product with nopCommerce product.
        /// 
        /// Before link a nopCommerce product with GTV product, product has to exist in nopCommerce 
        /// and nopCommerce product has to be linked with SubiektGT service.
        /// </summary>
        /// <param name="nopCommerceProductId"></param>
        /// <param name="gtvId"></param>
        /// <exception cref="Exceptions.CustomException"></exception>
        public override async Task BindAsync(int nopCommerceProductId, int gtvId)
        {
            await BindProductAsync(_service, nopCommerceProductId, gtvId);
        }

        public override async Task SetPriceReplicationAsync(int externalProductId, bool replicatedOrNotd)
        {
            throw new Exceptions.CustomException("Only Subiekt GT service can replicate prices");
        }

    }
}
