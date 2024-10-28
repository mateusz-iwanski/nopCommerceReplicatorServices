using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase
{
    /// <summary>
    /// GTV has two warehouses. This enum represents them.
    /// </summary>
    public enum WarehouseCode
    {
        M_MLP,  // default warehouse
        M_PROFIL  // warehouse with long elements
    }
}
