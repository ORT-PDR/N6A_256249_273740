using System;
using System.Linq;
using Server.BusinessLogic;
using Models;
using Communication;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Linq;

namespace Server.UI
{
    public class ProductMenu
    {
        private Socket socketClient;
        private string user;

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
                        DeleteProduct(user);
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
                var conversionHandler = new ConversionHandler();
                var socketHelper = new SocketHelper(socketClient);

                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.PublishProduct));

                string data = $"{name}:{description}:{price}:{stock}:{user}";

                byte[] dataBytes = conversionHandler.ConvertStringToBytes(data);
                byte[] lengthBytes = conversionHandler.ConvertIntToBytes(dataBytes.Length);
                socketHelper.Send(lengthBytes);
                socketHelper.Send(dataBytes);

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
            string currentUser = user;
            var userProducts = RetrieveAllUserProducts();

            if (userProducts.Length > 0)
            {
                Console.WriteLine("Select a product to update:");
                for (int i = 0; i < userProducts.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {userProducts[i]}");
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
                var conversionHandler = new ConversionHandler();
                var socketHelper = new SocketHelper(socketClient);

                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllUserProducts));
                string data = $"{user}";

                byte[] dataBytes = conversionHandler.ConvertStringToBytes(data);
                byte[] lengthBytes = conversionHandler.ConvertIntToBytes(dataBytes.Length);
                socketHelper.Send(lengthBytes);
                socketHelper.Send(dataBytes);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] listBytes = socketHelper.Receive(dataLength);
                string list = conversionHandler.ConvertBytesToString(listBytes);
                string[] products = list.Split(":");
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
            Console.WriteLine("5. Done (Exit)");

            bool exit = false;

            while (!exit)
            {
                string userInput = Console.ReadLine();

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    exit = true;
                    break;
                }

                if (int.TryParse(userInput, out int attributeChoice) && attributeChoice >= 1 && attributeChoice <= 5)
                {
                    string newValue = string.Empty;

                    switch (attributeChoice)
                    {
                        case 1:
                            Console.WriteLine($"Description (current: {selectedProduct.description}): ");
                            newValue = Console.ReadLine();
                            selectedProduct.description = !string.IsNullOrWhiteSpace(newValue) ? newValue : selectedProduct.description;
                            break;
                        case 2:
                            Console.WriteLine($"Stock Available (current: {selectedProduct.stock}): ");
                            if (int.TryParse(Console.ReadLine(), out int newStock) && newStock >= 0)
                            {
                                selectedProduct.stock = newStock;
                            }
                            else
                            {
                                Console.WriteLine("Invalid stock value. Stock must be a non-negative number.");
                            }
                            break;
                        case 3:
                            Console.WriteLine($"Price (current: {selectedProduct.price}): ");
                            if (double.TryParse(Console.ReadLine(), out double newPrice) && newPrice > 0)
                            {
                                selectedProduct.price = newPrice;
                            }
                            else
                            {
                                Console.WriteLine("Invalid price value. Price must be a positive number.");
                            }
                            break;
                        case 4:
                            Console.WriteLine($"Image Path (current: {selectedProduct.imagePath}): ");
                            newValue = Console.ReadLine();
                            selectedProduct.imagePath = !string.IsNullOrWhiteSpace(newValue) ? newValue : selectedProduct.imagePath;
                            break;
                        case 5:
                            productService.UpdateProduct(selectedProduct);
                            Console.WriteLine("Product updated successfully.");
                            exit = true; // Salir del bucle cuando el usuario elige "Done"
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid attribute selection. Please select a valid option (1-5) or type 'exit' to quit.");
                }
            }
        }

        private void DeleteProduct(String user)
        {
            string currentUser = user;
            var userProducts = productService.GetProductsByUser(currentUser);

            if (userProducts.Count > 0)
            {
                Console.WriteLine("Select a product to delete:");
                for (int i = 0; i < userProducts.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {productService.ProductToString(userProducts[i])}");
                }

                bool exit = false;

                while (!exit)
                {
                    string userInput = Console.ReadLine();

                    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        exit = true;
                        break;
                    }

                    if (int.TryParse(userInput, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= userProducts.Count)
                    {
                        Product selectedProduct = userProducts[selectedIndex - 1];
                        Console.WriteLine($"Are you sure you want to delete this product? (yes/no)\n{productService.ProductToString(selectedProduct)}");

                        string confirmation = Console.ReadLine();
                        if (confirmation.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            productService.DeleteProduct(selectedProduct.id);
                            Console.WriteLine("Product deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Product not deleted.");
                        }

                        exit = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please select a valid product number or type 'exit' to quit.");
                    }
                }
            }
            else
            {
                Console.WriteLine("You have no products to delete.");
            }

            Console.ReadKey();
        }

        private void SearchProducts()
        {
            Console.WriteLine("Enter product name to search:");
            string productName = Console.ReadLine();

            var products = productService.GetProducts(productName);
            if (products.Any())
            {
                Console.WriteLine("Search results:");
                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.id}, Name: {product.name}, Description: {product.description}");
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }

            Console.ReadKey();
        }
    }
}