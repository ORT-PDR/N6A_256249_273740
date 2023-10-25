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
            public const string CreateUser = "CUSR";
            public const string PublishProduct = "PUBL";
            public const string BuyProduct = "BUYP";
            public const string UpdateProduct = "UPDT";
            public const string UpdateProductImage = "UIMG";
            public const string DeleteProduct = "DELT";
            public const string SearchProducts = "SRCH";
            public const string GetProductDetails = "DETL";
            public const string RateProduct = "RATE";
            public const string DownloadProductImage = "DWLD";
            public const string GetAllProducts = "GALL";
            public const string GetAllUserProducts = "GUPR";
            public const string GetAllPurchases = "GPCH";
            public const string GetAllProductReviews = "GPRV";
            public const string Exit = "EXIT";
            public const string Logout = "LOUT";
        }
        
        public static long CalculateFileParts(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1;
        }
    }
}
