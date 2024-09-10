
namespace nopCommerceReplicatorServices.nopCommerce
{
    [Serializable]
    internal class CusomtException : Exception
    {
        public CusomtException()
        {
        }

        public CusomtException(string? message) : base(message)
        {
        }

        public CusomtException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}