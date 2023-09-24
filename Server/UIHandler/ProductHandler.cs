using System;
using System.Net;
using Communication;
using Communication.FileHandlers;
using Models;
using Server.BusinessLogic;

namespace Server.UIHandler
{
	public class ProductHandler
	{
        private readonly SocketHelper socketHelper;
        private readonly ConversionHandler conversionHandler;
        private readonly ProductService productService;

        public ProductHandler(SocketHelper socketHelper, ConversionHandler conversionHandler, ProductService productService)
		{
			this.socketHelper = socketHelper;
			this.conversionHandler = conversionHandler;
			this.productService = productService;
		}

		public void PublishProduct()
		{
            Console.WriteLine("Product publication requested by client.");

            var fileCommonHandler = new FileCommsHandler(socketHelper);
            string path = fileCommonHandler.ReceiveFile();
            Console.WriteLine("File recieved");

            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);
            string[] dataParts = data.Split(':');

            try
            {
                if (dataParts.Length == 5)
                {
                    Product product = new Product()
                    {
                        name = dataParts[0],
                        description = dataParts[1],
                        price = double.Parse(dataParts[2]),
                        stock = int.Parse(dataParts[3]),
                        creator = dataParts[4],
                        imagePath = path
                    };

                    productService.PublishProduct(product);
                    Console.WriteLine("Product published by client.");
                }
                else
                {
                    string resp = "All product fields must have a value!";
                    byte[] respBytes = conversionHandler.ConvertStringToBytes(resp);
                    SendResponse(respBytes);
                }

                string response = "Success";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
            catch(Exception e)
            {
                string resp = "There was an error: " + e.Message;
                byte[] respBytes = conversionHandler.ConvertStringToBytes(resp);
                SendResponse(respBytes);
            }
        }

        public void UpdateProduct()
        {
            try
            {
                byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
                byte[] dataBytes = socketHelper.Receive(dataLength);
                string data = conversionHandler.ConvertBytesToString(dataBytes);

                string[] dataArray = data.Split(":");
                string productName = dataArray[0];
                string attribute = dataArray[1];
                string newValue = dataArray[2];
                string user = dataArray[3];

                Product p = productService.GetProductByName(productName, user);
                switch (attribute)
                {
                    case "description":
                        p.description = newValue;
                        break;

                    case "stock":
                        p.stock = int.Parse(newValue);
                        break;

                    case "price":
                        p.price = int.Parse(newValue);
                        break;
                }

                productService.UpdateProduct(p);
                string response = "Success";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
            catch(Exception e)
            {
                string response = e.Message;
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
        }

        public void UpdateProductImage()
        {
            try
            {
                var fileCommonHandler = new FileCommsHandler(socketHelper);
                string path = fileCommonHandler.ReceiveFile();
                Console.WriteLine("File received");

                byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
                byte[] dataBytes = socketHelper.Receive(dataLength);
                string data = conversionHandler.ConvertBytesToString(dataBytes);

                string product = data.Split(":")[0];
                string user = data.Split(":")[1];

                Product p = productService.GetProductByName(product, user);
                string aux = p.imagePath;
                p.imagePath = path;

                productService.UpdateProduct(p);
                File.Delete(aux);

                string response = "Success";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
            catch (Exception e)
            {
                string response = e.Message;
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
        }

        public void SendAllUserProducts()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string username = conversionHandler.ConvertBytesToString(dataBytes);

            List<Product> products = productService.GetProductsByUser(username);
            string productNames = "";

            for(int i = 0; i < products.Count; i++)
            {
                productNames += productService.ProductToString(products[i]) + ";";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            Send(productNames);
        }
        
        public void SendAllProducts()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string username = conversionHandler.ConvertBytesToString(dataBytes);

            List<Product> products = productService.GetProducts();
            string productNames = "";

            for(int i = 0; i < products.Count; i++)
            {
                productNames += productService.ProductToString(products[i]) + ";";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            Send(productNames);
        }

        public void DeleteProduct()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);

            string product = data.Split(":")[0];
            string user = data.Split(":")[1];
            
            productService.DeleteProduct(product, user);
            Console.WriteLine("Product deleted by client.");

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            SendResponse(responseBytes);
        }

        public void SearchProducts()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string name = conversionHandler.ConvertBytesToString(dataBytes);

            List<Product> products = productService.GetProducts(name);
            string productNames = "";

            for (int i = 0; i < products.Count; i++)
            {
                productNames += productService.ProductToString(products[i]) + ";";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }
            Send(productNames);
        }

        public void BuyProduct()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);
            
            string product = data.Split(":")[0];
            string username = data.Split(":")[1];
            string buyer = data.Split(":")[2];
            
            productService.BuyProduct(product, username, buyer);
            
            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            SendResponse(responseBytes);
        }
        
        public void SendAllPurchases()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string username = conversionHandler.ConvertBytesToString(dataBytes);

            List<Product> products = productService.GetPurchases(username);
            string productNames = "";

            for(int i = 0; i < products.Count; i++)
            {
                productNames += productService.ProductToString(products[i]) + ";";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            Send(productNames);
        }

        public void RateProduct()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string review = conversionHandler.ConvertBytesToString(dataBytes);
            
            string product = review.Split(":")[0];
            string user = review.Split(":")[1];
            string score = review.Split(":")[2];
            string reviewText = review.Split(":")[3];
            string creator = review.Split(":")[4];

            if (int.TryParse(score, out int scoreInt))
            {
                productService.AddReview(product, user, scoreInt, reviewText, creator);
                string response = "Success";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
            else
            {
                throw new ServerException("Score must be an integer.");
            }
        }

        public void SendAllProductReviews()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);
            
            string product = data.Split(":")[0];
            string creator = data.Split(":")[1];

            List<Review> reviews = productService.GetReviews(product, creator);
            string reviewsString = "";

            for(int i = 0; i < reviews.Count; i++)
            {
                reviewsString += productService.ReviewToString(reviews[i]) + ";";
            }
            if (!string.IsNullOrEmpty(reviewsString))
            {
                reviewsString = reviewsString.TrimEnd(';');
            }

            Send(reviewsString);
        }

        public void DownloadProductImage()
        {
            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string imagePath = conversionHandler.ConvertBytesToString(dataBytes);

            try
            {
                string fileName = Path.GetFileName(imagePath);
                string destinationFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string destinationPath = Path.Combine(destinationFolderPath, fileName);

                lock(new object())
                {
                    File.Copy(imagePath, destinationPath);
                }
                
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Success");
                SendResponse(responseBytes);
                Console.WriteLine("Image download was successful.");
            }
            catch(Exception e)
            {
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(e.Message);
                SendResponse(responseBytes);
            }
        }

        private void Send(string response)
        {
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            socketHelper.Send(lengthBytes);
            socketHelper.Send(responseBytes);
        }

        private void SendResponse(byte[] responseBytes)
        {
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            socketHelper.Send(lengthBytes);
            socketHelper.Send(responseBytes);
        }
    }
}

