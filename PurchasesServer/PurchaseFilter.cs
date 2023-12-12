using System;
using System.Collections.Generic;
using System.Linq;

namespace PurchasesServer
{
    public class PurchaseFilter
    {
        public List<Purchase> FilterPurchases(List<Purchase> purchases, string username = null, string product = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var filteredPurchases = purchases;

            if (!string.IsNullOrEmpty(username))
            {
                filteredPurchases = filteredPurchases.Where(p => p.Username == username).ToList();
            }

            if (!string.IsNullOrEmpty(product))
            {
                filteredPurchases = filteredPurchases.Where(p => p.Product.Contains(product)).ToList();
            }

            if (startDate != null)
            {
                filteredPurchases = filteredPurchases.Where(p => p.Date.Date >= startDate.Value.Date).ToList();
            }

            if (endDate != null)
            {
                filteredPurchases = filteredPurchases.Where(p => p.Date.Date <= endDate.Value.Date).ToList();
            }

            return filteredPurchases.ToList();
        }
    }
}