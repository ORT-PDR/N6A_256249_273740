using System;
using System.Linq;
using Server.BusinessLogic;
using Models;
using Communication;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace Server.UI
{
    public class ProductMenu
    {
        private Socket socketClient;
        private ConversionHandler conversionHandler;
        private SocketHelper socketHelper;
        private string user;

        public ProductMenu()
        {
            conversionHandler = new ConversionHandler();
            socketHelper = new SocketHelper(socketClient);
        }

        public void ShowMainMenu(string _user, Socket _socketClient)
        {
            while (true)
            {
                socketClient = _socketClient;
                user = _user;

                Console.Clear();
                Console.WriteLine("Product Menu:");
                Console.WriteLine("1. Publish Product");
                Console.WriteLine("2. Update Product");
                Console.WriteLine("3. Delete Product");
                Console.WriteLine("4. Search Products");
                Console.WriteLine("5. Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        PublishProduct();
                        break;
                    case "2":
                        UpdateProduct();
                        break;
                    case "3":
                        DeleteProduct();
                        break;
                    case "4":
                        SearchProducts();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void PublishProduct()
        {
            Console.Clear();
            Console.WriteLine("Enter product details:");

            Console.WriteLine("Name: ");
            string? name = Console.ReadLine();
            Console.WriteLine("Description: ");
            string? description = Console.ReadLine();
            Console.WriteLine("Price: ");
            string? price = Console.ReadLine();
            Console.WriteLine("Stock available: ");
            string ? stock = Console.ReadLine();

            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.PublishProduct));

                string data = $"{name}:{description}:{price}:{stock}:{user}";
                Send(data);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                if (response == "Success")
                {
                    Console.WriteLine("Product published successfully.");
                    Console.ReadKey();
                    System.Console.WriteLine("Press any key to continue");
                    System.Console.ReadKey();
                }
                else
                {
                    PublishProduct();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (ServerException e)
            {
                Console.Write(e.Message);
            }
            catch(FormatException formatEx)
            {
                Console.WriteLine("Format exception. Stock and price must be integer.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }

        private void UpdateProduct()
        {
            Console.Clear();
            var userProducts = RetrieveAllUserProducts();

            if (userProducts.Length > 0)
            {
                Console.WriteLine("Select a product to update:");
                for (int i = 0; i < userProducts.Length; i++)
                {
                    string[] data = userProducts[i].Split(":");
                    Console.WriteLine($"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                }

                if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= userProducts.Length)
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

            Console.ReadKey();
        }

        private string[] RetrieveAllUserProducts()
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllUserProducts));
                string data = $"{user}";

                Send(data);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] listBytes = socketHelper.Receive(dataLength);
                string list = conversionHandler.ConvertBytesToString(listBytes);
                string[] products = list.Split(";");
                return products;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return new string[0];
        }

        private void ModifyProductMenu(string selectedProduct)
        {
            Console.Clear();
            Console.WriteLine("Select an attribute to update:");
            Console.WriteLine("1. Description");
            Console.WriteLine("2. Stock Available");
            Console.WriteLine("3. Price");
            Console.WriteLine("4. Image Path");
            Console.WriteLine("5. Done (Back)");

            bool back = false;

            string[] product = selectedProduct.Split(":");

            while (!back)
            {
                string userInput = Console.ReadLine();

                if (userInput.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    back = true;
                    break;
                }

                if (int.TryParse(userInput, out int attributeChoice) && attributeChoice >= 1 && attributeChoice <= 5)
                {
                    string newValue = string.Empty;

                    switch (attributeChoice)
                    {
                        case 1:
                            Console.WriteLine($"Description (current: {product[1]}): ");
                            Console.WriteLine("Enter new description or BACK to go back");
                            newValue = Console.ReadLine();
                            SendModifiedValue(product[0], "description", newValue);
                            break;
                        case 2:
                            Console.WriteLine($"Stock Available (current: {product[2]}): ");
                            if (int.TryParse(Console.ReadLine(), out int newStock) && newStock >= 0)
                            {
                                SendModifiedValue(product[0], "stock", newStock.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Invalid stock value. Stock must be a non-negative number.");
                            }
                            break;
                        case 3:
                            Console.WriteLine($"Price (current: {product[3]}): ");
                            if (double.TryParse(Console.ReadLine(), out double newPrice) && newPrice > 0)
                            {
                                SendModifiedValue(product[0], "price", newPrice.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Invalid price value. Price must be a positive number.");
                            }
                            break;
                        case 4:
                            Console.WriteLine($"Image Path (current: {product[4]}): ");
                            newValue = Console.ReadLine();
                            product[4] = !string.IsNullOrWhiteSpace(newValue) ? newValue : product[4];
                            SendModifiedValue(product[0], "image", newValue);
                            break;
                        case 5:
                            try
                            {
                                byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
                                int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
                                byte[] dataBytes = socketHelper.Receive(dataLength);
                                string response = conversionHandler.ConvertBytesToString(dataBytes);
                                if (response == "Success")
                                {
                                    Console.WriteLine("Product updated successfully.");
                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            back = true; // Salir del bucle cuando el usuario elige "Done"
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid attribute selection. Please select a valid option (1-5) or type 'back' to quit.");
                }
            }
        }

        private void SendModifiedValue(string productName, string attribute, string newValue)
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.UpdateProduct));

                string data = $"{productName}:{attribute}:{newValue}";

                Send(data);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                if (response == "Success")
                {
                    Console.WriteLine("Product updated successfully.");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (ServerException e)
            {
                Console.Write(e.Message);
            }
            catch (FormatException formatEx)
            {
                Console.WriteLine("Format exception. Stock and price must be integer.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }

        private void DeleteProduct()
        {
            Console.Clear();
            try
            {
                var userProducts = RetrieveAllUserProducts();

                if (userProducts.Length > 0)
                {
                    Console.WriteLine("Select a product to delete:");
                    for (int i = 0; i < userProducts.Length; i++)
                    {
                        string[] data = userProducts[i].Split(":");
                        Console.WriteLine($"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                    }

                    if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= userProducts.Length)
                    {
                        Console.WriteLine($"Are you sure you want to delete this product? (yes/no)");

                        string confirmation = Console.ReadLine();
                        if (confirmation.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.DeleteProduct));

                            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                            byte[] responseBytes = socketHelper.Receive(dataLength);
                            string response = conversionHandler.ConvertBytesToString(responseBytes);

                            if (response == "Success")
                            {
                                Console.WriteLine("Product deleted successfully.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Product not deleted.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please select a valid product number or type 'back' to quit.");
                    }
                }
                else
                {
                    Console.WriteLine("You have no products to delete.");
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (ServerException e)
            {
                Console.Write(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }

        private void SearchProducts()
        {
            Console.Clear();
            Console.WriteLine("Enter product name to search:");
            string productName = Console.ReadLine();

            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.SearchProducts));
                Send(productName);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                string[] products = response.Split(";");

                if (products.Length > 0)
                {
                    Console.WriteLine("Select a product to delete:");
                    for (int i = 0; i < products.Length; i++)
                    {
                        string[] data = products[i].Split(":");
                        Console.WriteLine($"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                    }
                }
                else
                {
                    Console.WriteLine("No products match your search");
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (ServerException e)
            {
                Console.Write(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
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
    }
}