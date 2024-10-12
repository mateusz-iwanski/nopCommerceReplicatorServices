using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Exceptions
{
    /// <summary>
    /// Thrown the exception when the data hasn't been replicated yet. 
    /// It is used to prevent data replication when 
    /// for example you want to update object which hasn't been replicated yet.
    /// </summary>
    public class UnreplicatedDataException : CustomException
    {
        public UnreplicatedDataException(string? message) : base(
            $"{message} Check the DataBinding database. If you are deleting or adding a new object " +
            $"via nopCommerce and not via replicator, you need to manually add the data to the DataBinding database " +
            $"for correct replication.")
        {
        }
    }
}
