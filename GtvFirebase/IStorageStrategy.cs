using FirebaseManager.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GtvApiHub.WebApi.DTOs
{
    public interface IStorageStrategy
    {
        string? GetFilePath();
    }
}
