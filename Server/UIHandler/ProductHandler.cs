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

        private void SendResponse(byte[] responseBytes)
        {
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            socketHelper.Send(lengthBytes);
            socketHelper.Send(responseBytes);
        }
    }
}

