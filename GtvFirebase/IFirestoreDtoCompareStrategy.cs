using FirebaseManager.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GtvApiHub
{
    public interface IFirestoreDtoCompareStrategy
    {
        /// <summary>
        /// Compare the same type of objects IFirestoreDto.
        /// 
        /// If objects has the same item code is the same object even if the other fields are different.
        /// If fields are different so one of object was updated.
        /// </summary>
        /// <returns>false if two different objects the same type, true if are the same objects</returns>
        bool Compare(IFirestoreDto other);

        /// <summary>
        /// Check if the DTO object from API is updated relative to Firestore DTO.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IsUpdated(IFirestoreDto other);
    }
}
