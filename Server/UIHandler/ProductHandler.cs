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

                Product p = productService.GetProductByName(productName);
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

                    case "image":
                        p.imagePath = newValue;
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
                productNames = productService.ProductToString(products[i]) + ";";
            }
            if (!string.IsNullOrEmpty(productNames))
            {
                productNames = productNames.TrimEnd(';');
            }

            Send(productNames);
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

