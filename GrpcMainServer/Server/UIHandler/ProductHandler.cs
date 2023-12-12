using System;
using System.Net;
using System.Text;
using AdministrationServer;
using Communication;
using System.Text;
using Communication.FileHandlers;
using GrpcMainServer.Server.BusinessLogic;
using RabbitMQ.Client;

namespace GrpcMainServer.Server.UIHandler;

public class ProductHandler
{
    private readonly NetworkDataHelper networkDataHelper;
    private readonly ConversionHandler conversionHandler;
    private readonly ProductService productService;

    public ProductHandler(NetworkDataHelper networkDataHelper, ConversionHandler conversionHandler,
        ProductService productService)
    {
        this.networkDataHelper = networkDataHelper;
        this.conversionHandler = conversionHandler;
        this.productService = productService;
    }

    public async Task PublishProduct()
    {
        Console.WriteLine("Product publication requested by client.");

        var fileCommonHandler = new FileCommsHandler(networkDataHelper);
        string path = await fileCommonHandler.ReceiveFile();
        if (path == "")
        {
            Console.WriteLine("File does not exist. Product will not have an image");
        }
        else
        {
            Console.WriteLine("File recieved");
        }

        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string data = conversionHandler.ConvertBytesToString(dataBytes);
        string[] dataParts = data.Split("#");

        try
        {
            if (dataParts.Length == 5)
            {
                Product product = new Product()
                {
                    Name = dataParts[0],
                    Description = dataParts[1],
                    Price = double.Parse(dataParts[2]),
                    Stock = int.Parse(dataParts[3]),
                    Creator = dataParts[4],
                    ImagePath = path
                };

                productService.PublishProduct(product);
                Console.WriteLine("Product published by client.");
            }
            else
            {
                string resp = "All product fields must have a value!";
                byte[] respBytes = conversionHandler.ConvertStringToBytes(resp);
                await SendResponse(respBytes);
            }

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string resp = "There was an error: " + e.Message;
            byte[] respBytes = conversionHandler.ConvertStringToBytes(resp);
            await SendResponse(respBytes);
        }
    }

    public async Task UpdateProduct()
    {

        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string data = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string[] dataArray = data.Split("#");
            string productName = dataArray[0];
            string attribute = dataArray[1];
            string newValue = dataArray[2];
            string user = dataArray[3];

            Product p = productService.GetProductByName(productName, user);
            switch (attribute)
            {
                case "description":
                    p.Description = newValue;
                    break;

                case "stock":
                    p.Stock = int.Parse(newValue);
                    break;

                case "price":
                    p.Price = int.Parse(newValue);
                    break;
            }

            productService.UpdateProduct(p);
            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task UpdateProductImage()
    {
        try
        {
            var fileCommonHandler = new FileCommsHandler(networkDataHelper);
            string path = await fileCommonHandler.ReceiveFile();
            if (path == "")
            {
                Console.WriteLine("File does not exist. Product will no longer have an image");
            }
            else
            {
                Console.WriteLine("File received");
            }

            byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);

            string product = data.Split("#")[0];
            string user = data.Split("#")[1];

            Product p = productService.GetProductByName(product, user);
            string aux = p.ImagePath;
            p.ImagePath = path;

            productService.UpdateProduct(p);
            if (aux != "")
            {
                File.Delete(aux);
            }

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task SendAllUserProducts()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string username = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            ProductList products = productService.GetProductsByUser(username);
            string productNames = "";

            for (int i = 0; i < products.Products.Count; i++)
            {
                productNames += productService.ProductToString(products.Products[i]) + ";";
            }

            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            await Send(productNames);

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task SendAllProducts()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string username = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            ProductList products = productService.GetProducts();
            string productNames = "";

            for (int i = 0; i < products.Products.Count; i++)
            {
                productNames += productService.ProductToString(products.Products[i]) + ";";
            }

            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            await Send(productNames);

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task DeleteProduct()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string data = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string product = data.Split("#")[0];
            string user = data.Split("#")[1];

            productService.DeleteProduct(product, user);
            Console.WriteLine("Product deleted by client.");

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task SearchProducts()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string name = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            ProductList products = productService.GetProducts(name);
            string productNames = "";

            for (int i = 0; i < products.Products.Count; i++)
            {
                productNames += productService.ProductToString(products.Products[i]) + ";";
            }

            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            await Send(productNames);

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task BuyProduct()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string data = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string product = data.Split("#")[0];
            string username = data.Split("#")[1];
            string buyer = data.Split("#")[2];

            productService.BuyProduct(product, username, buyer);

            string purchaseEventData = buyer + "," + product + "," + DateTime.Now.ToString();
            SendPurchaseEventToExchange(purchaseEventData);


            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task SendAllPurchases()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string username = conversionHandler.ConvertBytesToString(dataBytes);
        try
        {
            ProductList products = productService.GetPurchases(username);
            string productNames = "";

            for (int i = 0; i < products.Products.Count; i++)
            {
                productNames += productService.ProductToString(products.Products[i]) + ";";
            }

            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            await Send(productNames);

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task RateProduct()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string review = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string product = review.Split("#")[0];
            string user = review.Split("#")[1];
            string score = review.Split("#")[2];
            string reviewText = review.Split("#")[3];
            string creator = review.Split("#")[4];

            if (int.TryParse(score, out int scoreInt))
            {
                productService.AddReview(product, user, scoreInt, reviewText, creator);
                string response = "Success";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                await SendResponse(responseBytes);
            }
            else
            {
                throw new ServerException("Score must be an integer.");
            }
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task SendAllProductReviews()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string data = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string product = data.Split("#")[0];
            string creator = data.Split("#")[1];

            ReviewList reviews = productService.GetReviews(product, creator);
            string reviewsString = "";

            for (int i = 0; i < reviews.Reviews.Count; i++)
            {
                reviewsString += productService.ReviewToString(reviews.Reviews[i]) + ";";
            }

            if (!string.IsNullOrEmpty(reviewsString))
            {
                reviewsString = reviewsString.TrimEnd(';');
            }

            await Send(reviewsString);

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
        catch (Exception e)
        {
            string response = e.Message;
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            await SendResponse(responseBytes);
        }
    }

    public async Task DownloadProductImage()
    {
        byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
        int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
        byte[] dataBytes = await networkDataHelper.ReceiveAsync(dataLength);
        string imagePath = conversionHandler.ConvertBytesToString(dataBytes);

        try
        {
            string fileName = Path.GetFileName(imagePath);
            string destinationFolderPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string destinationPath = Path.Combine(destinationFolderPath, fileName);

            lock (new object())
            {
                File.Copy(imagePath, destinationPath);
            }

            byte[] responseBytes = conversionHandler.ConvertStringToBytes("Success");
            await SendResponse(responseBytes);
            Console.WriteLine("Image download was successful.");
        }
        catch (Exception e)
        {
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(e.Message);
            await SendResponse(responseBytes);
        }
    }

    private async Task Send(string response)
    {
        byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
        int responseLength = responseBytes.Length;

        byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
        await networkDataHelper.SendAsync(lengthBytes);
        await networkDataHelper.SendAsync(responseBytes);
    }

    private async Task SendResponse(byte[] responseBytes)
    {
        int responseLength = responseBytes.Length;

        byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
        await networkDataHelper.SendAsync(lengthBytes);
        await networkDataHelper.SendAsync(responseBytes);
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