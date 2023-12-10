namespace PurchaseServer
{
    public class PurchaseProcessor
    {
        private readonly PurchaseDataAccess purchaseDataAccess;

        public PurchaseProcessor()
        {
            purchaseDataAccess = PurchaseDataAccess.GetInstance();
        }

        public void ProcessPurchase(string purchaseEvent)
        {
            try
            {
                Console.WriteLine("Processing purchase event: {0}", purchaseEvent);
                purchaseDataAccess.AddPurchase(purchaseEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing purchase event: {0}", ex.Message);
            }
        }
    }
}