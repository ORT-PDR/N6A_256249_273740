using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace PurchasesServer.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    [ServiceFilter(typeof(CustomExceptionFilter))]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseDataAccess purchaseDataAccess = PurchaseDataAccess.GetInstance();
        private readonly PurchaseFilter purchaseFilter = new PurchaseFilter();

        [HttpGet]
        public ActionResult<List<Purchase>> GetFilteredPurchases(string? username = null, string? product = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var purchases = purchaseDataAccess.GetPurchases();

            var filteredPurchases = purchaseFilter.FilterPurchases(purchases, username, product, startDate, endDate);

            return Ok(filteredPurchases);
        }
    }
}