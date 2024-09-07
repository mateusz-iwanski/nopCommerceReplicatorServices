using nopCommerceReplicatorServices.Actions;
using nopCommerceWebApiClient.Interfaces.Tax;
using nopCommerceWebApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nopCommerceWebApiClient.Objects.Tax;
using nopCommerceWebApiClient.Objects.TaxCategory;
using nopCommerceWebApiClient.Interfaces;
using nopCommerceReplicatorServices.SubiektGT;
using Refit;
using System.Net.Http;
using nopCommerceWebApiClient.Helpers;

namespace nopCommerceReplicatorServices.nopCommerce
{
    internal class TaxNopCommerce : ITax
    {        
        private ITaxCategoryService _taxCategoryService { get; set; }

        public TaxNopCommerce(IApiConfigurationServices apiServices)
        {
            _taxCategoryService = apiServices.TaxCategoryService;
        }

        public async Task<int> GetCategoryByNameAsync(VatLevel percentage)
        {
            var taxCategory = await _taxCategoryService.GetByName(percentage.ToString());
            return taxCategory.Id;
        }

    }
}
