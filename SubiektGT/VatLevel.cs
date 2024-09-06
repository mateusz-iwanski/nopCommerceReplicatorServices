using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.SubiektGT
{
    /// <summary>
    /// In nopCommerce, the VAT level is a percentage value.
    ///
    /// Default in Polish e-commerce is 23% and 8%.
    /// In WebAPI, the VAT level is seeding by the enum name and percentage value
    /// In nopCommerce, the VAT level is a percentage.
    /// By default in Polish e-commerce it is 23% and 8%.
    /// In WebAPI, the VAT level is seeding at runtime with the same data as the VatLevel enum type.
    /// </summary>
    public enum VatLevel
    {
        PL_23_procent_podstawowa = 23,
        PL_8_procent_podstawowa = 8,
    }
}
