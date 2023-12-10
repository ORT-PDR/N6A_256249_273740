using Microsoft.AspNetCore.Mvc;

namespace PurchaseServer.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseDataAccess purchaseDataAccess;

        public PurchaseController()
        {
            purchaseDataAccess = PurchaseDataAccess.GetInstance();
        }

        [HttpGet("filter")]
        public ActionResult<List<string>> FilterPurchases(string criteria)
        {
            var filteredPurchases = purchaseDataAccess.FilterPurchasesByCriteria(criteria);
            return Ok(filteredPurchases);
        }
    } 
}