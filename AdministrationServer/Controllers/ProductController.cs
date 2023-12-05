using System;
using System.Threading.Tasks;
using Communication;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationServer.Controllers
{
    [Route("admin")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private Admin.AdminClient client;

        static readonly SettingsManager SettingsMgr = new SettingsManager();
        public ProductController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetAllProducts()
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5240");
            client = new Admin.AdminClient(channel);
            var emptyMessage = new EmptyMessage();
            var reply = await client.GetAllProductsAsync(emptyMessage);
            return Ok(reply);
        }

        [HttpPost("users")]
        public async Task<ActionResult> PostUser([FromBody] ProductDTO user)
        {
            //using var channel = GrpcChannel.ForAddress(grpcURL);
            using var channel = GrpcChannel.ForAddress("http://localhost:5240");
            client = new Admin.AdminClient(channel);
            var reply = await client.PostProductAsync(user);
            return Ok(reply.Message);
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser([FromRoute] int id)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5240");
            client = new Admin.AdminClient(channel);
            var reply = await client.DeleteProductAsync(new Id { Id_ = id });
            return Ok(reply.Message);
        }


    }
}
