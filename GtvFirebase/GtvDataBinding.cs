using nopCommerceReplicatorServices.Exceptions;
using nopCommerceReplicatorServices.SubiektGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase
{
    internal class GtvDataBinding
    {
        private readonly DataBinding.DataBinding _dataBinding;

        public GtvDataBinding(DataBinding.DataBinding dataBinding)
        {
            _dataBinding = dataBinding;
            return;
        }

        /// <summary>
        /// Return GTV product id by Subiekt GT id.
        /// 
        /// First it finds nopCommerce id by Subiekt GT id, and next by nopCommerce id it finds GTV id.
        /// </summary>
        /// <param name="subiektGtProductId">ID from subiekt gt product</param>
        /// <returns></returns>
        /// <exception cref="CustomException">If not find subiekt gt product, if not find Gtv product id</exception>
        public int GetGtvIdBySubiekt(int subiektGtProductId)
        {
            // find nopCommerce id by Subiekt GT id
            var bindingDataNopCommerce = _dataBinding.GetKeyBindingByExternalId(Service.SubiektGT, ObjectToBind.Product, subiektGtProductId) ??
                throw new CustomException($"Can't find product by Id - '{subiektGtProductId}' in DataBinding for service {Service.SubiektGT.ToString()}. You have to map it.");

            // find GTV product id by nopCommerce id
            var bindingDataGtv = _dataBinding.GetKeyBindingByNopCommerceId(Service.GtvApi, ObjectToBind.Product, bindingDataNopCommerce.NopCommerceId) ??
                throw new CustomException($"Can't find product by Id - '{bindingDataNopCommerce.NopCommerceId}' in DataBinding for service {Service.GtvApi.ToString()}. You have to map it.");

            return bindingDataGtv.ExternalId;
        }

        public int GetGtvIdByNopCommerce(int nopCommerceProductId)
        {
            var bindingDataNopCommerce = _dataBinding.GetKeyBindingByExternalId(Service.SubiektGT, ObjectToBind.Product, nopCommerceProductId) ??
                throw new CustomException($"Can't find product by Id - '{nopCommerceProductId}' in DataBinding for service {Service.GtvApi.ToString()}. You have to map it.");

            return bindingDataNopCommerce.ExternalId;
        }

    }
}
