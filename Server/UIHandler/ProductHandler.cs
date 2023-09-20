using System;
using System.Net;
using Communication;
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

            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] dataBytes = socketHelper.Receive(dataLength);
            string data = conversionHandler.ConvertBytesToString(dataBytes);
            string[] dataParts = data.Split(':');

            if (dataParts.Length == 5)
            {
                Product product = new Product()
                {
                    name = dataParts[0],
                    description = dataParts[1],
                    price = int.Parse(dataParts[2]),
                    stock = int.Parse(dataParts[3]),
                    creator = dataParts[4]
                };

                productService.PublishProduct(product);
            }
            else
            {
                throw new ServerException("All product fields must have a value!");
            }

            string response = "Success";
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            SendResponse(responseBytes);
        }

        public void UpdateProduct()
        {
            string currentUser = user;
            var userProducts = productService.GetProductsByUser(currentUser);

            if (userProducts.Count > 0)
            {
                Console.WriteLine("Select a product to update:");
                for (int i = 0; i < userProducts.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {productService.ProductToString(userProducts[i])}");
                }

                if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= userProducts.Count)
                {
                    ModifyProductMenu(userProducts[selectedIndex - 1]);
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
            else
            {
                Console.WriteLine("You have no products to update.");
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
                productNames = productService.ProductToString(products[i]) + ":";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(':');
            }

            byte[] listBytes = conversionHandler.ConvertStringToBytes(productNames);
            byte[] lengthListBytes = conversionHandler.ConvertIntToBytes(dataBytes.Length);
            socketHelper.Send(lengthListBytes);
            socketHelper.Send(listBytes);
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

