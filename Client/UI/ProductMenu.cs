using System;
using System.Linq;
using Server.BusinessLogic;
using Communication;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Communication.FileHandlers;

namespace Client.UI
{
    public class ProductMenu
    {
        private ConversionHandler conversionHandler;
        private SocketHelper socketHelper;
        private string user;
        private Socket socketClient;

        public ProductMenu(Socket _socketClient)
        {
            conversionHandler = new ConversionHandler();
            socketHelper = new SocketHelper(_socketClient);
            socketClient = _socketClient;
        }

        public void ShowMainMenu(string _user)
        {
            Console.Clear();
            while (true)
            {
                user = _user;
                
                Console.WriteLine("Product Menu:");
                Console.WriteLine("1. Publish Product");
                Console.WriteLine("2. Update Product");
                Console.WriteLine("3. Delete Product");
                Console.WriteLine("4. View Products");
                Console.WriteLine("5. My Purchases");
                Console.WriteLine("6. Exit");
                

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
                        ViewProductsMenu();
                        break;
                    case "5":
                        ViewPurchases();
                        break;
                    case "6":
                        Console.WriteLine("Logging out...");

                        socketClient.Shutdown(SocketShutdown.Both);
                        socketClient.Close();
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
            Console.WriteLine("Absolute path of the product picture: ");
            String imageAbsPath = Console.ReadLine();

            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.PublishProduct));

                if (imageAbsPath != null)
                {
                    var fileCommonHandler = new FileCommsHandler(socketHelper);
                    fileCommonHandler.SendFile(imageAbsPath);
                }

                string data = $"{name}:{description}:{price}:{stock}:{user}";
                Send(data);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                if (response == "Success")
                {
                    Console.WriteLine("Product published successfully.");
                    System.Console.WriteLine("Press any key to continue");
                    System.Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine(response);
                    System.Console.WriteLine("Press any key to continue");
                    System.Console.ReadKey();
                    Console.Clear();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
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

            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
            byte[] listBytes = socketHelper.Receive(dataLength);
            string response = conversionHandler.ConvertBytesToString(listBytes);

            if (response == "Success")
            {
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
            }
            else
            {
                Console.WriteLine(response);
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        private string[] RetrieveAllUserProducts()
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllUserProducts));
                Send(user);

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
        
        private string[] RetrieveAllProducts()
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllProducts));
                Send(user);

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
            bool back = false;
            
            string[] product = selectedProduct.Split(":");

            while (!back)
            {
                Console.WriteLine("Select an attribute to update:");
                Console.WriteLine("1. Description");
                Console.WriteLine("2. Stock Available");
                Console.WriteLine("3. Price");
                Console.WriteLine("4. Image Path");
                Console.WriteLine("5. Done (Back)");
                
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
                            Console.Clear();
                            Console.WriteLine($"Description (current: {product[1]}): ");
                            Console.WriteLine("Enter new description or BACK to go back");
                            newValue = Console.ReadLine();
                            SendModifiedValue(product[0], "description", newValue);
                            break;
                        case 2:
                            Console.Clear();
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
                            Console.Clear();
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
                            Console.Clear();
                            Console.WriteLine($"Image Path (current: {product[4]}): ");
                            newValue = Console.ReadLine();
                            product[4] = !string.IsNullOrWhiteSpace(newValue) ? newValue : product[4];
                            SendModifiedValue(product[0], "image", newValue);
                            break;
                        case 5:
                            back = true;
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
                if (attribute == "image")
                {
                    SendNewImage(productName, newValue);
                }

                else
                {
                    socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.UpdateProduct));

                    string data = $"{productName}:{attribute}:{newValue}:{user}";

                    Send(data);
                }

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
                else
                {
                    Console.WriteLine("Product updated error.");
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
                Console.ReadKey();
            }
            catch (FormatException formatEx)
            {
                Console.WriteLine("Format exception. Stock and price must be integer.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
                Console.ReadKey();
            }
        }

        private void SendNewImage(string productName, string newImagePath)
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.UpdateProductImage));
                var fileCommonHandler = new FileCommsHandler(socketHelper);
                fileCommonHandler.SendFile(newImagePath);
                Send($"{productName}:{user}");
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
                Console.ReadKey();
            }
            catch (FormatException formatEx)
            {
                Console.WriteLine("Format exception. Stock and price must be integer.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
                Console.ReadKey();
            }
        }

        private void DeleteProduct()
        {
            Console.Clear();
            try
            {
                var userProducts = RetrieveAllUserProducts();

                if (!userProducts.All(string.IsNullOrEmpty) && userProducts.Length > 0)
                {
                    Console.WriteLine("Select a product to delete:");
                    for (int i = 0; i < userProducts.Length; i++)
                    {
                        string[] data = userProducts[i].Split(":");
                        Console.WriteLine($"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                    }
                    
                    var selectedProduct = Console.ReadLine();

                    if (int.TryParse(selectedProduct, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= userProducts.Length)
                    {
                        Console.WriteLine($"Are you sure you want to delete this product? (yes/no)");

                        string confirmation = Console.ReadLine();
                        if (confirmation.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.DeleteProduct));
                            Send(userProducts[selectedIndex-1].Split(":")[0]+":"+user);
                            
                            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                            byte[] responseBytes = socketHelper.Receive(dataLength);
                            string response = conversionHandler.ConvertBytesToString(responseBytes);

                            if (response == "Success")
                            {
                                Console.WriteLine("Product deleted successfully.");
                            }
                            else
                            {
                                Console.WriteLine(response);
                                Console.WriteLine("Press any key to continue");
                                Console.ReadKey();
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
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
                Console.ReadKey();
            }
        }

        private void ViewProductsMenu()
        {
            Console.Clear();
            var products = RetrieveAllProducts();

            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
            byte[] responseBytes = socketHelper.Receive(dataLength);
            string response = conversionHandler.ConvertBytesToString(responseBytes);

            if (response == "Success")
            {
                if (!products.All(string.IsNullOrEmpty) && products.Length > 0)
                {
                    Console.WriteLine("If you want to search a product, type the name of the product, else select a product to expand");
                    for (int i = 0; i < products.Length; i++)
                    {
                        string[] data = products[i].Split(":");
                        Console.WriteLine(
                            $"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                    }
                    var option = Console.ReadLine();

                    if (int.TryParse(option, out int selectedIndex) && selectedIndex >= 1 &&
                        selectedIndex <= products.Length)
                    {
                        var product = products[selectedIndex - 1].Split(":");
                        ViewProduct(product);
                    }
                    else
                    {
                        SearchProducts(option);
                    }
                }
                else
                {
                    Console.WriteLine("You have no products to view.");
                }
            }
            else
            {
                Console.WriteLine(response);
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }
        
        private void ViewProduct(string[] product)
        {
            Console.Clear();
            Console.WriteLine($"Name: {product[0]}");
            Console.WriteLine($"Description: {product[1]}");
            Console.WriteLine($"Stock: {product[2]}");
            Console.WriteLine($"Price: {product[3]}");
            Console.WriteLine($"Image Path: {product[4]}");
            Console.WriteLine($"Owner: {product[5]}");
            Console.WriteLine("----Menu----");
            Console.WriteLine("1. Buy Product");
            Console.WriteLine("2. Add Review");
            Console.WriteLine("3. Explore Reviews");
            Console.WriteLine("4. Download Product Image");
            Console.WriteLine("5. Exit");

            bool exit = false;
            while (!exit)
            {
                var userInput = Console.ReadLine();
                if (int.TryParse(userInput, out int attributeChoice) && attributeChoice >= 1 && attributeChoice <= 5)
                {
                    switch (attributeChoice)
                    {
                        case 1:
                            BuyProduct(product);
                            break;
                        case 2:
                            ReviewProduct(product);
                            break;
                        case 3:
                            ExploreReviews(product);
                            break;
                        case 4:
                            DownloadImage(product);
                            break;
                        case 5:
                            return;
                    }
                    exit = true;
                }
                else
                {
                    if (userInput == "exit")
                    {
                        return;
                    }
                    Console.WriteLine("Invalid attribute selection. Please select a valid option (1-5) or type 'exit' to quit.");
                }
            }
        }

        private void BuyProduct(string[] product)
        {
            if (int.Parse(product[2]) <= 0)
            {
                Console.WriteLine("Product out of stock");
                return;
            }
            if (product[5] == user)
            {
                Console.WriteLine("ERROR: You can't buy your product");
                return;
            }
            
            Console.WriteLine($"Are you sure you want to Buy this product for {product[3]}$? (yes/no)");

            string confirmation = Console.ReadLine();
            if (confirmation.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.BuyProduct));
                Send(product[0]+":"+product[5]+":"+user);
                            
                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                if (response == "Success")
                {
                    Console.WriteLine("Product bought successfully.");
                }
                else
                {
                    Console.WriteLine(response);
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Product not bought.");
            }
        }

        private void ViewPurchases()
        {
            Console.Clear();
            var products = RetrieveAllPurchases();


            byte[] lb = socketHelper.Receive(Protocol.FixedDataSize);
            int dl = conversionHandler.ConvertBytesToInt(lb);
            byte[] responseBytes = socketHelper.Receive(dl);
            string response = conversionHandler.ConvertBytesToString(responseBytes);

            if (response == "Success")
            {

                if (!products.All(string.IsNullOrEmpty) && products.Length > 0)
                {
                    Console.WriteLine("Select a product to expand:");

                    bool exit = false;
                    while (!exit)
                    {
                        for (int i = 0; i < products.Length; i++)
                        {
                            string[] data = products[i].Split(":");
                            Console.WriteLine(
                                $"{i + 1}. Name: {data[0]} | Description: {data[1]} | Units: {data[2]} | Bought for: {data[3]}");
                        }

                        var option = Console.ReadLine();

                        if (int.TryParse(option, out int selectedIndex) && selectedIndex >= 1 &&
                            selectedIndex <= products.Length)
                        {
                            var product = products[selectedIndex - 1].Split(":");
                            ViewProduct(product);
                        }
                        else if (option == "exit")
                        {
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Invalid selection. Please select a valid product number or type 'exit' to quit.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("You have no products to view.");
                }
            }
            else
            {
                Console.WriteLine(response);
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }

        private void DownloadImage(string[] product)
        {
            socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.DownloadProductImage));
            Send(product[4]);

            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
            byte[] responseBytes = socketHelper.Receive(dataLength);
            string response = conversionHandler.ConvertBytesToString(responseBytes);

            if (response == "Success")
            {
                Console.WriteLine("Image has been downloaded to your Downloads folder.");
            }
            else
            {
                Console.Write("There was an error downloading the image: " + response);
            }
            Console.Clear();
        }

        private string[] RetrieveAllPurchases()
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllPurchases));
                Send(user);

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
                Console.ReadKey();
            }
            
            return new string[0];
        }
        
        private void ReviewProduct(string [] product)
        {
            Console.WriteLine("Enter your Score from 1 to 5:");
            string score = Console.ReadLine();
            Console.WriteLine("Enter your Review:");
            string review = Console.ReadLine();

            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.RateProduct));
                Send(product[0]+":"+product[5]+":"+score+":"+review+":"+user);
            
                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                if (response == "Success")
                {
                    Console.WriteLine("Product published successfully.");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine(response);
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
                Console.ReadKey();
            }
        }

        private void ExploreReviews(string[] product)
        {
            var reviews = RetrieveProductReviews(product);

            byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
            byte[] responseBytes = socketHelper.Receive(dataLength);
            string response = conversionHandler.ConvertBytesToString(responseBytes);

            if (response == "Success")
            {
                if (!reviews.All(string.IsNullOrEmpty) && reviews.Length > 0)
                {
                    for (int i = 0; i < reviews.Length; i++)
                    {
                        string[] data = reviews[i].Split(":");
                        Console.WriteLine(
                            $"Score: {data[0]} | Review: {data[1]} | User: {data[2]}");
                    }
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("You have no products to view.");
                }
            }
            else
            {
                Console.WriteLine(response);
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }

        private string[] RetrieveProductReviews(string[] product)
        {
            try
            {
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.GetAllProductReviews));
                Send(product[0]+":"+product[5]);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] listBytes = socketHelper.Receive(dataLength);
                string list = conversionHandler.ConvertBytesToString(listBytes);
                string[] reviews = list.Split(";");
                return reviews;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            
            return new string[0];
        }

        private void SearchProducts(string productName)
        {
            try
            {
                Console.Clear();
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.SearchProducts));
                Send(productName);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                byte[] lb = socketHelper.Receive(Protocol.FixedDataSize);
                int dl = conversionHandler.ConvertBytesToInt(lb);
                byte[] rb = socketHelper.Receive(dl);
                string r = conversionHandler.ConvertBytesToString(rb);

                if (r == "Success")
                {
                    string[] products = response.Split(";");

                    if (products.Length > 0)
                    {
                        Console.WriteLine("Select a product to expand:");
                        for (int i = 0; i < products.Length; i++)
                        {
                            string[] data = products[i].Split(":");
                            Console.WriteLine($"{i + 1}. Name: {data[0]} | Description: {data[1]} | Stock: {data[2]} | Price: {data[3]}");
                        }

                        bool exit = false;
                        while (!exit)
                        {
                            var option = Console.ReadLine();

                            if (option == "exit")
                            {
                                exit = true;
                                break;
                            }

                            if (int.TryParse(option, out int selectedIndex) && selectedIndex >= 1 &&
                                selectedIndex <= products.Length)
                            {
                                var product = products[selectedIndex - 1].Split(":");
                                exit = true;
                                ViewProduct(product);
                            }
                            else
                            {
                                Console.WriteLine("Invalid selection. Please select a valid product number or type 'exit' to quit.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No products match your search");
                        Console.WriteLine("Returning to previous window");
                        ViewProductsMenu();
                    }
                }
                else
                {
                    Console.WriteLine(r);
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
                Console.ReadKey();
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