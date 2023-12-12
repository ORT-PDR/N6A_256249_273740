using AdministrationServer;
using GrpcMainServer.Server.Models;

namespace GrpcMainServer.Server;

public class Storage
	{
		private static Storage instance = null;
		private static readonly object lockObject = new object();

		public List<User> users = new List<User>();
		public ProductList products = new ProductList();
		
		public static Storage GetInstance()
		{
			lock (lockObject)
			{
				if (instance == null)
				{
					instance = new Storage();
					instance.users = new List<User>();
					instance.products = new ProductList();
				}	
			}
			return instance;
		} 

		public void AddUser(User user)
		{
			lock (lockObject)
			{
				ValidatePassword(user.password);
				ValidateUsername(user.username);
				users.Add(user);
			}
        }

		public void AddProduct(Product product)
		{
			lock (lockObject)
			{
                ValidateProduct(product);
                products.Products.Add(product);
			}
		}

        public void AddReview(Product product, Review review)
        {
            if (review.Score < 1 || review.Score > 5)
            {
				throw new ServerException("Score must be between 1 and 5");
            }

            lock (lockObject)
            {
	            product.Reviews.Reviews.Add(review);
            }
        }

        private void ValidatePassword(string pass)
		{
			if (pass.Length < 8 && !pass.Any(char.IsUpper))
			{
				throw new ServerException("Password must be at least 8 characters long and must have an UpperCase");
			}
		}

		private void ValidateUsername(string uname)
		{
            if (users.Any(u => u.username == uname))
            {
                throw new ServerException("Username must be unique");
            }
        }

        private void ValidateProduct(Product p)
        {
            if (products.Products.Any(u => u.Name == p.Name && u.Creator == p.Creator))
            {
                throw new ServerException("Product name must be unique");
            }
        }

        public ProductList GetAllProducts()
		{
			lock (lockObject)
			{
				return products;
			}
		}

		public Product GetProductById(string productId)
		{
			lock (lockObject)
			{
				return products.Products.FirstOrDefault(p => p.Id == productId);
			}
		}

        public Product GetProductByName(string productName)
        {
            lock (lockObject)
            {
                return products.Products.FirstOrDefault(p => p.Name == productName);
            }
        }
        
        public Product GetUserProductByName(string productName, string user)
        {
	        lock (lockObject)
	        {
		        return products.Products.FirstOrDefault(p => p.Name == productName && p.Creator == user);
	        }
        }

        public User GetUser(string username)
        {
	        return users.FirstOrDefault(u => u.username == username);
        }

        public void DeleteProduct(string id)
		{
			lock (lockObject)
			{
				Product productToRemove = products.Products.FirstOrDefault(p => p.Id == id);

				if (productToRemove != null)
				{
					if (!string.IsNullOrWhiteSpace(productToRemove.ImagePath))
					{
						File.Delete(productToRemove.ImagePath);
					}
					products.Products.Remove(productToRemove);
				}
				else
				{
					throw new ServerException("Product not found.");
				}
			}
		}
        
        public void BuyProduct(string productId, string buyer)
        {
	        lock (lockObject)
	        {
		        Product productToBuy = products.Products.FirstOrDefault(p => p.Id == productId);

		        if (productToBuy == null)
		        {
			        throw new ServerException("Product not found.");
		        }

		        if (productToBuy.Stock <= 0)
		        {
			        throw new ServerException("Product out of stock.");
		        }
                
		        productToBuy.Stock--;
		        
		        User user = users.FirstOrDefault(u => u.username == buyer);
		        if (user != null)
		        {
			        user.purchases.Products.Add(productToBuy);
		        }
	        }
        }
    }