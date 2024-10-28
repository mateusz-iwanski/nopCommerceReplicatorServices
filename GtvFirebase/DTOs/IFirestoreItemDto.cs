namespace nopCommerceReplicatorServices.GtvFirebase.DTOs
{
    public interface IFirestoreItemDto
    {
        /// <summary>
        /// This field is used to name the collection for the dto document in Firestore.
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// This field is used to uniquely identify a document in Firestore.
        /// The best idea is to use a field that is unique in the DTO object.
        /// </summary>
        string DocumentUniqueField { get; }
    }
}