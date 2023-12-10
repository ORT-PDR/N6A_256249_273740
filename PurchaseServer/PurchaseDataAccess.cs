using System;
using System.Collections.Generic;

namespace PurchaseServer
{
    public class PurchaseDataAccess
    {
        private List<string> purchases;
        private object padlock;
        private static PurchaseDataAccess instance;

        private static object singletonPadlock = new object();

        public static PurchaseDataAccess GetInstance()
        {
            lock (singletonPadlock)
            {
                if (instance == null)
                {
                    instance = new PurchaseDataAccess();
                }
            }
            return instance;
        }

        private PurchaseDataAccess()
        {
            purchases = new List<string>();
            padlock = new object();
        }

        public void AddPurchase(string purchaseEvent)
        {
            lock (padlock)
            {
                purchases.Add(purchaseEvent);
            }
        }

        public List<string> FilterPurchasesByCriteria(string criteria)
        {
            List<string> filteredPurchases = new List<string>();

            lock (padlock)
            {
                foreach (var purchase in purchases)
                {
                    if (purchase.Contains(criteria))
                    {
                        filteredPurchases.Add(purchase);
                    }
                }
            }

            return filteredPurchases;
        }
    }
}