using nopCommerceReplicatorServices.DataBinding;
using nopCommerceReplicatorServices.nopCommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.SubiektGT
{
    /// <summary>
    /// Helpful class to bind Subiekt GT product with nopCommerce product in BindingData database.
    /// </summary>
    public class SubiektGtProductDataBinder : ProductDataBinderBase, IProductDataBinder
    {
        private readonly IServiceProvider _serviceProvider;

        public SubiektGtProductDataBinder(DataBinding.DataBinding dataBinding, IServiceProvider service)
           : base(dataBinding, Service.SubiektGT)
        {
            _serviceProvider = service;
            return;
        }

        /// <summary>
        /// Link in BindData database Subiekt GT product with nopCommerce product.
        /// 
        /// Before link a nopCommerce product with Subiekt GT product, product has to exist in nopCommerce 
        /// and nopCommerce product has to be linked with SubiektGT service.
        /// </summary>
        /// <param name="nopCommerceProductId"></param>
        /// <param name="gtvId"></param>
        /// <exception cref="Exceptions.CustomException"></exception>
        public override async Task BindAsync(int nopCommerceProductId, int gtvId)
        {
            await BindProductAsync(_serviceProvider, nopCommerceProductId, gtvId);
        }
    }
}
