using nopCommerceReplicatorServices.SubiektGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Actions
{
    public interface ITax
    {        
        Task<int> GetCategoryByNameAsync(VatLevel percentage);
    }
}
