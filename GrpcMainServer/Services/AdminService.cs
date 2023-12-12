using AdministrationServer;
using Grpc.Core;
using GrpcMainServer.Server.BusinessLogic;

namespace GrpcMainServer.Services;

public class AdminService: Admin.AdminBase
{
    public override Task<ProductList> GetAllProducts(EmptyMessage emptyMessage, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
        Console.WriteLine("Client requested all products");
        ProductList products = productService.GetProducts(null);
        return Task.FromResult(products);
    }

    public override Task<MessageReply> PostProduct(ProductDTO productDto, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
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

    public override Task<MessageReply> DeleteProduct(ProductIdUser request, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
        Console.WriteLine("Client requested to delete a product");
        try
        {
            productService.DeleteProduct(request.Id, request.Username);
            return Task.FromResult(new MessageReply { Message = "Product deleted successfully." });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new MessageReply { Message = $"Error deleting product: {ex.Message}" });
        }
    }

    public override Task<Product> UpdateProduct(Product product, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
        Console.WriteLine("Client requested to update a product");
        try
        {
            Product newprod = productService.UpdateProduct(product);
            return Task.FromResult(newprod);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating product: {ex.Message}");
            return Task.FromResult(new Product { Id = "", Name = "Error" });
        }
    }

    public override Task<MessageReply> BuyProduct(PurchaseInfo purchaseInfo, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
        Console.WriteLine("Client requested to buy a product");
        try
        {
            productService.BuyProduct(purchaseInfo.Product, purchaseInfo.Creator, purchaseInfo.Username);
            return Task.FromResult(new MessageReply { Message = "Product bought successfully." });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new MessageReply { Message = $"Error buying product: {ex.Message}" });
        }
    }

    public override Task<ReviewList> GetAllProductReviews(ProductInfo productinfo, ServerCallContext context)
    {
        ProductService productService = ProductService.GetInstance();
        Console.WriteLine("Client requested all reviews from a product");
        ReviewList reviews = productService.GetReviews(productinfo.ProductName, productinfo.Creator);
        return Task.FromResult(reviews);
    }
}