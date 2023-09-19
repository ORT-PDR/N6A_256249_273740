using System.Text;

namespace Communication
{
    public static class Protocol
    {
        public static readonly int FixedDataSize = 4;

        public const int FixedFileSize = 8;
        public const int MaxPacketSize = 32768;
        
        public static class ProtocolCommands
        {
            public const string Authenticate = "AUTH";
            public const string PublishProduct = "PUBLISH";
            public const string BuyProduct = "BUY";
            public const string UpdateProduct = "UPDATE";
            public const string DeleteProduct = "DELETE";
            public const string SearchProducts = "SEARCH";
            public const string GetProductDetails = "DETAILS";
            public const string RateProduct = "RATE";
        }
        
        public static long CalculateFileParts(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1;
        }
    }
}
