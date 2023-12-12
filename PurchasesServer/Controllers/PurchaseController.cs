using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace PurchasesServer.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseDataAccess purchaseDataAccess;
        private readonly PurchaseFilter purchaseFilter;

        public PurchaseController()
        {
            purchaseDataAccess = PurchaseDataAccess.GetInstance();
            purchaseFilter = new PurchaseFilter();
        }

        [HttpGet("filtered")]
        public ActionResult<List<Purchase>> GetFilteredPurchases(string username = null, string product = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var purchases = purchaseDataAccess.GetPurchases(); // Obtener todas las compras

            var filteredPurchases = purchaseFilter.FilterPurchases(purchases, username, product, startDate, endDate);

            return Ok(filteredPurchases);
        }
    }
}