using AdministrationServer;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Server.BusinessLogic;

namespace Server.AdministrationServices;

public class AdminService : Admin.AdminBase
{
    private readonly ProductService productService;
    public AdminService(ProductService _productService)
    {
        this.productService = _productService;
    }
    public override Task<ProductList> GetAllProducts(EmptyMessage emptyMessage, ServerCallContext context)
    {
        Console.WriteLine("Client requested all products");
        ProductList products = productService.GetProducts(null);
        return Task.FromResult(products);
    }

    public override Task<MessageReply> PostProduct(ProductDTO productDto, ServerCallContext context)
    {
        Console.WriteLine("Client requested to post a product");
        Product p = new Product()
        {
            Id = "",
            Name = productDto.Name,
            Description = productDto.Description,
            Stock = productDto.Stock,
            Price = productDto.Price,
            ImagePath = productDto.ImagePath,
            Creator = productDto.Creator,
            Reviews = new ReviewList()
        };
        try
        {
            productService.PublishProduct(p);
            return Task.FromResult(new MessageReply { Message = "Product published successfully." });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new MessageReply { Message = $"Error publishing product: {ex.Message}" });
        }
    } 
}