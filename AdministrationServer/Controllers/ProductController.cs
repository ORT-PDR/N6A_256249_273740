using System;
using System.Threading.Tasks;
using AdministrationServer.Filters;
using Communication;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationServer.Controllers
{
    [Route("admin")]
    [ApiController]
    [TypeFilter(typeof(ExceptionFilter))]
    public class ProductController : ControllerBase
    {
        private Admin.AdminClient client;

        static readonly SettingsManager SettingsMgr = new SettingsManager();
        public ProductController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        [HttpGet("products")]
        public async Task<ActionResult> GetAllProducts()
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5257");
            client = new Admin.AdminClient(channel);
            var emptyMessage = new EmptyMessage();
            var reply = await client.GetAllProductsAsync(emptyMessage);
            return Ok(reply);
        }

        [HttpPost("product")]
        public async Task<ActionResult> PostProduct([FromBody] ProductDTO product)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5257");
            client = new Admin.AdminClient(channel);
            var reply = await client.PostProductAsync(product);
            return Ok(reply.Message);
        }
        //
        // [HttpDelete("users/{id}")]
        // public async Task<ActionResult> DeleteUser([FromRoute] int id)
        // {
        //     using var channel = GrpcChannel.ForAddress("http://localhost:5240");
        //     client = new Admin.AdminClient(channel);
        //     var reply = await client.DeleteProductAsync(new Id { Id_ = id });
        //     return Ok(reply.Message);
        // }


    }
}
