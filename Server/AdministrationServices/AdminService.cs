using AdministrationServer;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Server.BusinessLogic;
using Product = Models.Product;

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
        Console.WriteLine("Request para traer todos los productos");
        List<Product> products = productService.GetProducts(null);
        return Task.FromResult(new ProductList() { Products = { products } });
    }
}