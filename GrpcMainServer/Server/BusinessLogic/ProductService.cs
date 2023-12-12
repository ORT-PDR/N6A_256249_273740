using System.Text;
using AdministrationServer;
using GrpcMainServer.Server.Models;
using RabbitMQ.Client;

namespace GrpcMainServer.Server.BusinessLogic;

public class ProductService
{
    private Storage storage;
    private static ProductService instance;

    private static readonly object singletonlock = new object();


    public static ProductService GetInstance()
    {
        lock (singletonlock)
        {
            if (instance == null)
                instance = new ProductService();
            instance.storage = Storage.GetInstance();
        }
        return instance;
    }

    public void PublishProduct(Product product)
    {
        ValidateProduct(product);
        product.Id = Guid.NewGuid().ToString();

        storage.AddProduct(product);
    }

    public ProductList GetProducts(string productNameFilter = null)
    {
        var allProducts = storage.GetAllProducts();

        if (string.IsNullOrWhiteSpace(productNameFilter))
        {
            return allProducts;
        }

        var filteredProducts = new ProductList();
        foreach (var product in allProducts.Products)
        {
            if (product.Name.Contains(productNameFilter, StringComparison.OrdinalIgnoreCase))
            {
                filteredProducts.Products.Add(product);
            }
        }

        return filteredProducts;
    }

    public Product GetProductById(string productId)
    {
        return storage.GetProductById(productId);
    }

    public Product GetProductByName(string name, string user)
    {
        return storage.GetUserProductByName(name, user);
    }

    public Product UpdateProduct(Product updatedProduct)
    {
        Product existingProduct = storage.GetProductByName(updatedProduct.Name);

        if (existingProduct == null)
        {
            throw new ServerException("Product not found.");
        }

        lock (new object())
        {
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Stock = updatedProduct.Stock;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.ImagePath = updatedProduct.ImagePath;

            ValidateProduct(existingProduct);
        }

        return existingProduct;
    }

    public void DeleteProduct(string product, string user)
    {
        Product existingProduct = storage.GetUserProductByName(product, user);
        if (existingProduct == null)
        {
            throw new ServerException("Product not found.");
        }
        storage.DeleteProduct(existingProduct.Id);
    }

    public void BuyProduct(string product, string creator, string buyer)
    {
        Product existingProduct = storage.GetUserProductByName(product, creator);
        if (existingProduct == null)
        {
            throw new ServerException("Product not found.");
        }
        
        string purchaseEventData = buyer + "," + product + "," + DateTime.Now.ToString();
        SendPurchaseEventToExchange(purchaseEventData);
        
        storage.BuyProduct(existingProduct.Id, buyer);
    }
    
    public ProductList GetPurchases(string username)
    {
        User user = storage.GetUser(username);

        return user.purchases;
    }
    
    public void AddReview(string product, string user, int score, string comment, string creator)
    {
        Product existingProduct = storage.GetUserProductByName(product, user);
        if (existingProduct == null)
        {
            throw new ServerException("Product not found.");
        }

        Review review = new Review()
        {
            Id = Guid.NewGuid().ToString(),
            Score = score,
            Comment = comment,
            User = creator
        };
        storage.AddReview(existingProduct, review);
    }

    public ReviewList GetReviews(string product, string user)
    {
        Product existingProduct = storage.GetUserProductByName(product, user);
        if (existingProduct == null)
        {
            throw new ServerException("Product not found.");
        }

        return existingProduct.Reviews;
    }
    
    public string ReviewToString(Review review)
    {
        if (review == null)
        {
            return "Review is null";
        }

        return $"{review.User}#{review.Comment}#{review.Score}";
    }
    
    public ProductList GetProductsByUser(string userName)
    {
        var allProducts = storage.GetAllProducts();
        var productsByUser = new ProductList();

        foreach (var product in allProducts.Products)
        {
            if (product.Creator.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                productsByUser.Products.Add(product);
            }
        }

        return productsByUser;
    }
    
    public string ProductToString(Product product)
    {
        if (product == null)
        {
            return "Product is null";
        }

        return $"{product.Name}#{product.Description}#{product.Stock}#{product.Price}#{product.ImagePath}#{product.Creator}";
    }

    private void ValidateProduct(Product product)
    {
        if (product == null)
        {
            throw new ServerException("Product cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ServerException("Product name cannot be empty or whitespace.");
        }

        if (product.Stock < 0)
        {
            throw new ServerException("Stock must be a non-negative number.");
        }

        if (product.Price <= 0)
        {
            throw new ServerException("Price must be a positive number.");
        }
    }
    
    public void SendPurchaseEventToExchange(string purchaseEventData)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declara un intercambio de tipo "fanout"
            channel.ExchangeDeclare(exchange: "purchase_events_exchange", type: ExchangeType.Fanout);

            // Publica el mensaje en el intercambio "purchase_events_exchange"
            var body = Encoding.UTF8.GetBytes(purchaseEventData);
            channel.BasicPublish(exchange: "purchase_events_exchange", routingKey: "", basicProperties: null,
                body: body);

            Console.WriteLine("Sent purchase event to Exchange: {0}", purchaseEventData);
        }
    }
}